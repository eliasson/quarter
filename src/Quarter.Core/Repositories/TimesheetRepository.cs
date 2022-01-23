using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.Core.Repositories;

public record ActivityUsage(IdOf<Activity> ActivityId, int TotalMinutes, UtcDateTime LastUsed);
public record ProjectTotalUsage(int TotalMinutes, List<ActivityUsage> Activities, UtcDateTime LastUsed);

public interface ITimesheetRepository : IRepository<Timesheet>
{
    Task<Timesheet> GetOrNewTimesheetAsync(Date date, CancellationToken ct);
    Task<Timesheet> GetByDateAsync(Date date, CancellationToken ct);
    Task<RemoveResult> RemoveSlotsForProjectAsync(IdOf<Project> projectId, CancellationToken ct);
    Task<RemoveResult> RemoveSlotsForActivityAsync(IdOf<Activity> activityId, CancellationToken ct);
    Task<ProjectTotalUsage> GetProjectTotalUsageAsync(IdOf<Project> projectId, CancellationToken ct);
}

public class InMemoryTimesheetRepository : InMemoryRepositoryBase<Timesheet>, ITimesheetRepository
{
    public async Task<Timesheet> GetOrNewTimesheetAsync(Date date, CancellationToken ct)
    {
        // TODO: Implement a proper GetOrCreate
        try
        {
            return await GetByDateAsync(date, ct);
        }
        catch (NotFoundException)
        {
            var ts = Timesheet.CreateForDate(date);
            await CreateAsync(ts, ct);
            return ts;
        }
    }

    public Task<RemoveResult> RemoveSlotsForProjectAsync(IdOf<Project> projectId, CancellationToken ct)
    {
        var result = RemoveResult.NotRemoved;
        foreach (var (_, timesheet) in Storage)
        {
            var slots = timesheet.Slots().ToList();
            var originalCount = slots.Count;

            var updatedSlots = slots.Where(s => !Equals(s.ProjectId, projectId)).ToList();
            timesheet.SetSlots(updatedSlots);

            if (originalCount > updatedSlots.Count)
                result = RemoveResult.Removed;
        }
        return Task.FromResult(result);
    }

    public Task<RemoveResult> RemoveSlotsForActivityAsync(IdOf<Activity> activityId, CancellationToken ct)
    {
        var result = RemoveResult.NotRemoved;
        foreach (var (_, timesheet) in Storage)
        {
            var slots = timesheet.Slots().ToList();
            var originalCount = slots.Count;

            var updatedSlots = slots.Where(s => !Equals(s.ActivityId, activityId)).ToList();
            timesheet.SetSlots(updatedSlots);

            if (originalCount > updatedSlots.Count)
                result = RemoveResult.Removed;
        }
        return Task.FromResult(result);
    }

    public Task<Timesheet> GetByDateAsync(Date date, CancellationToken ct)
    {
        var result = Storage.Values.FirstOrDefault(t => t.Date.Equals(date));
        if (result is null)
            throw new NotFoundException($"No timesheet for date {date.IsoString()} exists");
        return Task.FromResult(result);
    }

    public Task<ProjectTotalUsage> GetProjectTotalUsageAsync(IdOf<Project> projectId, CancellationToken ct)
    {
        var total = 0;
        var activities = new List<ActivityUsage>();
        var soonestTimestamp = DateTime.MinValue;

        var slotsForProject = Storage.Values
            .SelectMany(t => t.Slots().Where(s => s.ProjectId.Equals(projectId)));

        foreach (var slot in slotsForProject)
        {
            if (slot.Created.DateTime > soonestTimestamp)
                soonestTimestamp = slot.Created.DateTime;

            activities.Add(new ActivityUsage(slot.ActivityId, slot.Duration * 15, slot.Created));
            total += slot.Duration * 15;
        }

        var result = new ProjectTotalUsage(total, activities, new UtcDateTime(soonestTimestamp));
        return Task.FromResult(result);
    }
}

public class PostgresTimesheetRepository : PostgresRepositoryBase<Timesheet>, ITimesheetRepository
{
    private readonly IdOf<User> _userId;

    private const string TableName = "timesheet";
    private const string SlotTableName = "timeslot";
    private const string AggregateName = "Timesheet";
    private const string UserIdColumnName = "userid";
    private const string DateColumnName = "date";

    public PostgresTimesheetRepository(IPostgresConnectionProvider connectionProvider, IdOf<User> userId)
        : base(connectionProvider, TableName, AggregateName)
    {
        _userId = userId;
    }

    protected override IEnumerable<string> AdditionalColumns()
        => new [] { UserIdColumnName, DateColumnName };

    protected override object AdditionalColumnValue(string columnName, Timesheet aggregate)
        => columnName switch
        {
            UserIdColumnName => _userId.Id,
            DateColumnName => aggregate.Date.IsoString(),
            _ => throw new NotImplementedException($"Additional column named [{columnName}] is not implemented"),
        };

    protected override NpgsqlParameter WithAccessCondition()
        => new NpgsqlParameter(UserIdColumnName, _userId.Id);

    protected override Task PostCreateAsync(NpgsqlConnection connection, Timesheet timesheet, CancellationToken ct)
        => StoreTimeSlotsForTimesheet(connection, timesheet, ct);

    protected override Task PostUpdateByIdAsync(NpgsqlConnection connection, Timesheet timesheet, CancellationToken ct)
        => StoreTimeSlotsForTimesheet(connection, timesheet, ct);

    public async Task<Timesheet> GetOrNewTimesheetAsync(Date date, CancellationToken ct)
    {
        // TODO: Implement a proper GetOrCreate
        try
        {
            return await GetByDateAsync(date, ct);
        }
        catch (NotFoundException)
        {
            var ts = Timesheet.CreateForDate(date);
            await CreateAsync(ts, ct);
            return ts;
        }
    }

    public async Task<Timesheet> GetByDateAsync(Date date, CancellationToken ct)
    {
        await using var connection = await _connectionProvider.GetConnectionAsync(ct);
        var query = $"SELECT data FROM {TableName} WHERE date=@date";
        var parameters = new List<NpgsqlParameter>{ new ("date", date.IsoString()) };
        var result = await ExecuteQueryAsync(connection, query, ct, parameters).ToListAsync(ct);

        var timesheet = result.FirstOrThrow(new NotFoundException($"No timesheet for date {date.IsoString()} exists"));

        timesheet.SetSlots(await ReadTimeSlots(connection, timesheet.Id, ct));
        return timesheet;
    }

    public async Task<RemoveResult> RemoveSlotsForProjectAsync(IdOf<Project> projectId, CancellationToken ct)
    {
        var parameters = new List<NpgsqlParameter> { new("projectid", projectId.Id) };
        return await ExecuteRemoveStatement(
            $"DELETE FROM {SlotTableName} WHERE projectid=@projectId;", parameters, ct);
    }

    public async Task<RemoveResult> RemoveSlotsForActivityAsync(IdOf<Activity> activityId, CancellationToken ct)
    {
        var parameters = new List<NpgsqlParameter> { new("activityid", activityId.Id) };
        return await ExecuteRemoveStatement(
            $"DELETE FROM {SlotTableName} WHERE activityid=@activityId;", parameters, ct);
    }

    protected override async Task PostGetByIdAsync(NpgsqlConnection connection, Timesheet timesheet, CancellationToken ct)
    {
        var slots = await ReadTimeSlots(connection, timesheet.Id, ct);
        timesheet.SetSlots(slots);
    }

    public async Task<ProjectTotalUsage> GetProjectTotalUsageAsync(IdOf<Project> projectId, CancellationToken ct)
    {
        await using var connection = await _connectionProvider.GetConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT activityid, duration, created FROM {SlotTableName} WHERE projectid=@projectId;";
        command.Parameters.AddRange(new NpgsqlParameter[]
        {
            new("projectid", projectId.Id)
        });
        await using var reader = await command.ExecuteReaderAsync(ct);

        var total = 0;
        var activities = new List<ActivityUsage>();
        var soonestTimestamp = DateTime.MinValue;
        while (await reader.ReadAsync(ct))
        {
            var activityId = IdOf<Activity>.Of((Guid) reader[0]);
            var duration = Convert.ToInt32((short) reader[1]);
            var createdTimestamp = DateTime.Parse((string) reader[2]);

            var created = new UtcDateTime(createdTimestamp);
            if (createdTimestamp > soonestTimestamp)
                soonestTimestamp = createdTimestamp;

            activities.Add(new ActivityUsage(activityId, duration * 15, created));
            total += duration * 15;
        }

        return new ProjectTotalUsage(total, activities,  new UtcDateTime(soonestTimestamp));
    }

    private async Task StoreTimeSlotsForTimesheet(NpgsqlConnection connection, Timesheet timesheet, CancellationToken ct)
    {
        await using var deleteCommand = connection.CreateCommand();
        deleteCommand.CommandText = $"DELETE FROM {SlotTableName} WHERE id=@id;";
        deleteCommand.Parameters.AddRange(new NpgsqlParameter[] { new("id", timesheet.Id.Id) });
        await deleteCommand.ExecuteNonQueryAsync(ct);

        foreach (var slot in timesheet.Slots())
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"INSERT INTO {SlotTableName} (id, projectId, activityId, date, qoffset, duration, created, userid) VALUES (@id, @projectId, @activityId, @date, @qoffset, @duration, @created, @userid)";
            command.Parameters.AddRange(new NpgsqlParameter[]
            {
                new("id", timesheet.Id.Id),
                new("projectid", slot.ProjectId.Id),
                new("activityid", slot.ActivityId.Id),
                new("date", timesheet.Date.IsoString()),
                new("qoffset", slot.Offset),
                new("duration", slot.Duration),
                new("created", slot.Created.DateTime.ToString("yyyy-MM-ddTHH:mm:sszzz")),
                new(UserIdColumnName, _userId.Id)
            });
            await command.ExecuteNonQueryAsync(ct);
        }
    }

    private static async Task<IList<ActivityTimeSlot>> ReadTimeSlots(NpgsqlConnection connection, IdOf<Timesheet> id, CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT projectId, activityId, qoffset, duration, created FROM {SlotTableName} WHERE id=@id;";
        command.Parameters.AddRange(new NpgsqlParameter[] { new("id", id.Id) });

        var slots = new List<ActivityTimeSlot>();
        await using var reader = await command.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var projectId = IdOf<Project>.Of((Guid) reader[0]);
            var activityId = IdOf<Activity>.Of((Guid) reader[1]);
            var offset = Convert.ToInt32((short) reader[2]);
            var duration = Convert.ToInt32((short) reader[3]);
            var created = new UtcDateTime(DateTime.Parse((string) reader[4]));
            slots.Add(new ActivityTimeSlot(projectId, activityId, offset, duration, created));
        }

        return slots;
    }
}