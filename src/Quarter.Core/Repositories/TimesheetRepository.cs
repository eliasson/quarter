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

public record ProjectTotalUsage(IdOf<Project> ProjectId, int TotalMinutes, List<ActivityUsage> Activities, UtcDateTime LastUsed)
{
    public static readonly ProjectTotalUsage Zero = new(IdOf<Project>.None, 0, new List<ActivityUsage>(), UtcDateTime.MinValue);

    public virtual bool Equals(ProjectTotalUsage? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return ProjectId.Equals(other.ProjectId) &&
               TotalMinutes.Equals(TotalMinutes) &&
               Activities.SequenceEqual(other.Activities);
    }

    public override int GetHashCode()
        => HashCode.Combine(ProjectId);
}

public class UsageOverTime
{
    public Date From { get; }
    public Date To { get; }
    public int TotalMinutes { get; }
    public IReadOnlyDictionary<Date, IList<ProjectTotalUsage>> Usage { get; }

    public UsageOverTime(Date from, Date to, int totalMinutes, IReadOnlyDictionary<Date, IList<ProjectTotalUsage>> usage)
    {
        From = from;
        To = to;
        TotalMinutes = totalMinutes;
        Usage = usage;
    }
}

public interface ITimesheetRepository : IRepository<Timesheet>
{
    Task<Timesheet> GetOrNewTimesheetAsync(Date date, CancellationToken ct);
    Task<Timesheet> GetByDateAsync(Date date, CancellationToken ct);
    Task<RemoveResult> RemoveSlotsForProjectAsync(IdOf<Project> projectId, CancellationToken ct);
    Task<RemoveResult> RemoveSlotsForActivityAsync(IdOf<Activity> activityId, CancellationToken ct);
    Task<ProjectTotalUsage> GetProjectTotalUsageAsync(IdOf<Project> projectId, CancellationToken ct);
    Task<UsageOverTime> GetUsageForPeriodAsync(Date fromDate, Date toDate, CancellationToken ct);
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
        var activities = new Dictionary<IdOf<Activity>, ActivityUsage>();
        var soonestTimestamp = DateTime.MinValue;

        var slotsForProject = Storage.Values
            .SelectMany(t => t.Slots().Where(s => s.ProjectId.Equals(projectId)));

        foreach (var slot in slotsForProject)
        {
            if (slot.Created.DateTime > soonestTimestamp)
                soonestTimestamp = slot.Created.DateTime;

            if (activities.TryGetValue(slot.ActivityId, out var existingUsage))
                activities[slot.ActivityId] = new ActivityUsage(slot.ActivityId, existingUsage.TotalMinutes + slot.Duration * 15, slot.Created);
            else
                activities[slot.ActivityId] = new ActivityUsage(slot.ActivityId, slot.Duration * 15, slot.Created);
            total += slot.Duration * 15;
        }

        var result = new ProjectTotalUsage(projectId, total, activities.Values.ToList(), new UtcDateTime(soonestTimestamp));
        return Task.FromResult(result);
    }

    public Task<UsageOverTime> GetUsageForPeriodAsync(Date fromDate, Date toDate, CancellationToken ct)
    {
        var usage = new Dictionary<Date, IList<ProjectTotalUsage>>();
        var total = 0;

        foreach (var ts in Storage.Values)
        {
            if (ts.Date.DateTime < fromDate.DateTime || ts.Date.DateTime > toDate.DateTime) continue;

            foreach (var summary in ts.Summarize())
            {
                var activityUsage = summary.Activities
                    .Select(a => new ActivityUsage(a.ActivityId, a.Duration * 15, UtcDateTime.MinValue))
                    .ToList();

                if (usage.TryGetValue(ts.Date, out var existing))
                {
                    existing.Add(new ProjectTotalUsage(summary.ProjectId, summary.Duration * 15, activityUsage, UtcDateTime.MinValue));
                }
                else
                {
                    var projectUsage = new List<ProjectTotalUsage>
                    {
                        new(summary.ProjectId, summary.Duration * 15, activityUsage, UtcDateTime.MinValue)
                    };
                    usage.Add(ts.Date, projectUsage);
                }

                total += summary.Duration * 15;
            }
        }

        var result = new UsageOverTime(fromDate, toDate, total, usage);
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
    private const string DateTimestampColumnName = "date_ts";

    public PostgresTimesheetRepository(IPostgresConnectionProvider connectionProvider, IdOf<User> userId)
        : base(connectionProvider, TableName, AggregateName)
    {
        _userId = userId;
    }

    public override async Task Truncate(CancellationToken ct)
    {
        await base.Truncate(ct);
        await TruncateTableAsync(SlotTableName, ct);
    }

    protected override IEnumerable<string> AdditionalColumns()
        => new[] { UserIdColumnName, DateColumnName, DateTimestampColumnName };

    protected override object AdditionalColumnValue(string columnName, Timesheet aggregate)
        => columnName switch
        {
            UserIdColumnName => _userId.Id,
            DateColumnName => aggregate.Date.IsoString(),
            DateTimestampColumnName => aggregate.Date.DateTime,
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
        var query = $"SELECT data FROM {TableName} WHERE date_ts=@date AND {UserIdColumnName}=@userId";
        var parameters = new List<NpgsqlParameter> { new("date", date.DateTime), WithAccessCondition() };
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
        command.CommandText = $"SELECT activityid, duration, created_ts FROM {SlotTableName} WHERE projectid=@projectId;";
        command.Parameters.AddRange(new NpgsqlParameter[]
        {
            new("projectid", projectId.Id)
        });
        await using var reader = await command.ExecuteReaderAsync(ct);

        var total = 0;
        var activities = new Dictionary<IdOf<Activity>, ActivityUsage>();
        var soonestTimestamp = DateTime.MinValue;
        while (await reader.ReadAsync(ct))
        {
            var activityId = IdOf<Activity>.Of((Guid)reader[0]);
            var duration = Convert.ToInt32((short)reader[1]);
            var createdTimestamp = (DateTime)reader[2];

            var created = UtcDateTime.FromUtcDateTime(createdTimestamp);
            if (createdTimestamp > soonestTimestamp)
                soonestTimestamp = createdTimestamp;

            if (activities.TryGetValue(activityId, out var existingUsage))
                activities[activityId] = new ActivityUsage(activityId, existingUsage.TotalMinutes + duration * 15, created);
            else
                activities[activityId] = new ActivityUsage(activityId, duration * 15, created);
            total += duration * 15;
        }

        return new ProjectTotalUsage(projectId, total, activities.Values.ToList(), new UtcDateTime(soonestTimestamp));
    }

    public async Task<UsageOverTime> GetUsageForPeriodAsync(Date fromDate, Date toDate, CancellationToken ct)
    {
        await using var connection = await _connectionProvider.GetConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT projectId, activityid, duration, date_ts FROM {SlotTableName} WHERE {UserIdColumnName}=@userId AND date_ts >= @from_date AND date_ts <= @to_date;";
        command.Parameters.AddRange(new[]
        {
            new("from_date", fromDate.DateTime),
            new("to_date", toDate.DateTime),
            WithAccessCondition(),
        });
        await using var reader = await command.ExecuteReaderAsync(ct);

        var usage = new Dictionary<Date, IList<ProjectTotalUsage>>();

        var slots = new List<(Date Date, ActivityTimeSlot Slot)>();
        while (await reader.ReadAsync(ct))
        {
            var projectId = IdOf<Project>.Of((Guid)reader[0]);
            var activityId = IdOf<Activity>.Of((Guid)reader[1]);
            var duration = Convert.ToInt32((short)reader[2]);
            var date = new Date((DateTime)reader[3]);
            slots.Add((date, new ActivityTimeSlot(projectId, activityId, 0, duration)));
        }

        var total = 0;
        foreach (var slotsForDay in slots.GroupBy(s => s.Date))
        {
            var projectUsages = new List<ProjectTotalUsage>();
            var slotsPerProject = slotsForDay.Select(s => s.Slot).GroupBy(s => s.ProjectId);

            foreach (var sp in slotsPerProject)
            {
                var activityUsages = new List<ActivityUsage>();
                var perActivity = sp.GroupBy(a => a.ActivityId);
                var projectTotal = 0;

                foreach (var ap in perActivity)
                {
                    var activityTotal = ap.Aggregate(0, (acc, a) => acc + (a.Duration * 15));
                    projectTotal += activityTotal;
                    activityUsages.Add(new ActivityUsage(ap.Key, activityTotal, UtcDateTime.MinValue));
                }

                var projectUsage = new ProjectTotalUsage(sp.Key, projectTotal, activityUsages, UtcDateTime.MinValue);
                projectUsages.Add(projectUsage);

                total += projectTotal;
            }
            usage.Add(slotsForDay.Key, projectUsages);
        }

        return new UsageOverTime(fromDate, toDate, total, usage);
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
            command.CommandText = $"INSERT INTO {SlotTableName} (id, projectId, activityId, date, date_ts, qoffset, duration, created, created_ts, userid) VALUES (@id, @projectId, @activityId, @date, @date_ts, @qoffset, @duration, @created, @created_ts, @userid)";
            command.Parameters.AddRange(new NpgsqlParameter[]
            {
                new("id", timesheet.Id.Id),
                new("projectid", slot.ProjectId.Id),
                new("activityid", slot.ActivityId.Id),
                new("date", timesheet.Date.IsoString()),
                new("date_ts", timesheet.Date.DateTime),
                new("qoffset", slot.Offset),
                new("duration", slot.Duration),
                new("created", slot.Created.DateTime.ToString("yyyy-MM-ddTHH:mm:sszzz")),
                new("created_ts", slot.Created.DateTime),
                new(UserIdColumnName, _userId.Id)
            });
            await command.ExecuteNonQueryAsync(ct);
        }
    }

    private async Task<IList<ActivityTimeSlot>> ReadTimeSlots(NpgsqlConnection connection, IdOf<Timesheet> id, CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT projectId, activityId, qoffset, duration, created_ts FROM {SlotTableName} WHERE id=@id AND {UserIdColumnName}=@userId;";
        command.Parameters.AddRange(new NpgsqlParameter[] { new("id", id.Id), WithAccessCondition() });

        var slots = new List<ActivityTimeSlot>();
        await using var reader = await command.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var projectId = IdOf<Project>.Of((Guid)reader[0]);
            var activityId = IdOf<Activity>.Of((Guid)reader[1]);
            var offset = Convert.ToInt32((short)reader[2]);
            var duration = Convert.ToInt32((short)reader[3]);
            var created = new UtcDateTime((DateTime)reader[4]);
            slots.Add(new ActivityTimeSlot(projectId, activityId, offset, duration, created));
        }

        return slots;
    }
}
