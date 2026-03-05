using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.Core.Repositories;

public class SqliteTimesheetRepository(ISqliteConnectionProvider connectionProvider, IdOf<User> userId)
    : SqliteRepositoryBase<Timesheet>(connectionProvider, TableName, AggregateName), ITimesheetRepository
{
    private const string TableName = "timesheet";
    private const string SlotTableName = "timeslot";
    private const string AggregateName = "Timesheet";
    private const string UserIdColumnName = "userid";
    private const string DateColumnName = "date";
    private const string DateTimestampColumnName = "date_ts";

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
            UserIdColumnName => userId.Id.ToString(),
            DateColumnName => aggregate.Date.IsoString(),
            DateTimestampColumnName => aggregate.Date.IsoString(),
            _ => throw new NotImplementedException($"Additional column named [{columnName}] is not implemented"),
        };

    protected override SqliteParameter? WithAccessCondition()
        => new(UserIdColumnName, userId.Id.ToString());

    protected override Task PostCreateAsync(SqliteConnection connection, Timesheet timesheet, CancellationToken ct)
        => StoreTimeSlotsForTimesheet(connection, timesheet, ct);

    protected override Task PostUpdateByIdAsync(SqliteConnection connection, Timesheet timesheet, CancellationToken ct)
        => StoreTimeSlotsForTimesheet(connection, timesheet, ct);

    public async Task<Timesheet> GetOrNewTimesheetAsync(Date date, CancellationToken ct)
    {
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
        var query = $"SELECT data FROM {TableName} WHERE date_ts=@date AND {UserIdColumnName}=@{UserIdColumnName}";
        var parameters = new List<SqliteParameter>
        {
            new("date", date.IsoString()),
            WithAccessCondition()!
        };
        var result = await ExecuteQueryAsync(connection, query, ct, parameters).ToListAsync(ct);

        var timesheet = result.FirstOrThrow(new NotFoundException($"No timesheet for date {date.IsoString()} exists"));

        timesheet.SetSlots(await ReadTimeSlots(connection, timesheet.Id, ct));
        return timesheet;
    }

    public async Task<RemoveResult> RemoveSlotsForProjectAsync(IdOf<Project> projectId, CancellationToken ct)
    {
        var parameters = new List<SqliteParameter> { new("projectid", projectId.Id.ToString()) };
        return await ExecuteRemoveStatement(
            $"DELETE FROM {SlotTableName} WHERE projectid=@projectid;", parameters, ct);
    }

    public async Task<RemoveResult> RemoveSlotsForActivityAsync(IdOf<Activity> activityId, CancellationToken ct)
    {
        var parameters = new List<SqliteParameter> { new("activityid", activityId.Id.ToString()) };
        return await ExecuteRemoveStatement(
            $"DELETE FROM {SlotTableName} WHERE activityid=@activityid;", parameters, ct);
    }

    protected override async Task PostGetByIdAsync(SqliteConnection connection, Timesheet timesheet, CancellationToken ct)
    {
        var slots = await ReadTimeSlots(connection, timesheet.Id, ct);
        timesheet.SetSlots(slots);
    }

    public async Task<ProjectTotalUsage> GetProjectTotalUsageAsync(IdOf<Project> projectId, CancellationToken ct)
    {
        await using var connection = await _connectionProvider.GetConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT activityid, duration, created_ts FROM {SlotTableName} WHERE projectid=@projectid;";
        command.Parameters.Add(new SqliteParameter("projectid", projectId.Id.ToString()));
        await using var reader = await command.ExecuteReaderAsync(ct);

        var total = 0;
        var activities = new Dictionary<IdOf<Activity>, ActivityUsage>();
        var soonestTimestamp = DateTime.MinValue;

        while (await reader.ReadAsync(ct))
        {
            var activityId = IdOf<Activity>.Of(Guid.Parse(reader.GetString(0)));
            var duration = Convert.ToInt32((long)reader[1]);
            var createdStr = reader.IsDBNull(2) ? null : reader.GetString(2);
            var createdTimestamp = createdStr is null
                ? DateTime.MinValue
                : DateTime.Parse(createdStr).ToUniversalTime();

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
        command.CommandText = $"SELECT projectid, activityid, duration, date_ts FROM {SlotTableName} WHERE {UserIdColumnName}=@{UserIdColumnName} AND date_ts >= @from_date AND date_ts <= @to_date;";
        command.Parameters.Add(new SqliteParameter("from_date", fromDate.IsoString()));
        command.Parameters.Add(new SqliteParameter("to_date", toDate.IsoString()));
        command.Parameters.Add(WithAccessCondition()!);
        await using var reader = await command.ExecuteReaderAsync(ct);

        var slots = new List<(Date Date, ActivityTimeSlot Slot)>();
        while (await reader.ReadAsync(ct))
        {
            var projectId = IdOf<Project>.Of(Guid.Parse(reader.GetString(0)));
            var activityId = IdOf<Activity>.Of(Guid.Parse(reader.GetString(1)));
            var duration = Convert.ToInt32((long)reader[2]);
            var dateStr = reader.GetString(3);
            var date = Date.From(dateStr);
            slots.Add((date, new ActivityTimeSlot(projectId, activityId, 0, duration)));
        }

        var usage = new Dictionary<Date, IList<ProjectTotalUsage>>();
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

                projectUsages.Add(new ProjectTotalUsage(sp.Key, projectTotal, activityUsages, UtcDateTime.MinValue));
                total += projectTotal;
            }
            usage.Add(slotsForDay.Key, projectUsages);
        }

        return new UsageOverTime(fromDate, toDate, total, usage);
    }

    public async Task<IReadOnlyList<Timesheet>> GetTimesheetsForMonthAsync(int year, int month, CancellationToken ct)
    {
        var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.LastDayOfMonth();

        await using var connection = await _connectionProvider.GetConnectionAsync(ct);
        var query =
            $"SELECT data FROM {TableName} WHERE {UserIdColumnName}=@{UserIdColumnName} AND date_ts >= @startdate AND date_ts <= @enddate ORDER BY date_ts";

        var parameters = new List<SqliteParameter>
        {
            WithAccessCondition()!,
            new("startdate", startDate.ToString("yyyy-MM-dd")),
            new("enddate", endDate.ToString("yyyy-MM-dd")),
        };

        var timesheetLookup = await ExecuteQueryAsync(connection, query, ct, parameters)
            .ToDictionaryAsync(k => k.Date.IsoString(), v => v, ct);
        var timesheets = new List<Timesheet>();

        foreach (var date in startDate.RangeTo(endDate))
        {
            if (timesheetLookup.TryGetValue(date.IsoString(), out var timesheet))
            {
                timesheet.SetSlots(await ReadTimeSlots(connection, timesheet.Id, ct));
                timesheets.Add(timesheet);
            }
            else
            {
                timesheets.Add(Timesheet.CreateForDate(new Date(date)));
            }
        }

        return timesheets;
    }

    private async Task StoreTimeSlotsForTimesheet(SqliteConnection connection, Timesheet timesheet, CancellationToken ct)
    {
        await using var deleteCommand = connection.CreateCommand();
        deleteCommand.CommandText = $"DELETE FROM {SlotTableName} WHERE id=@id;";
        deleteCommand.Parameters.Add(new SqliteParameter("id", timesheet.Id.Id.ToString()));
        await deleteCommand.ExecuteNonQueryAsync(ct);

        foreach (var slot in timesheet.Slots())
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"INSERT INTO {SlotTableName} (id, projectid, activityid, date, date_ts, qoffset, duration, created, created_ts, userid) VALUES (@id, @projectid, @activityid, @date, @date_ts, @qoffset, @duration, @created, @created_ts, @userid)";
            command.Parameters.Add(new SqliteParameter("id", timesheet.Id.Id.ToString()));
            command.Parameters.Add(new SqliteParameter("projectid", slot.ProjectId.Id.ToString()));
            command.Parameters.Add(new SqliteParameter("activityid", slot.ActivityId.Id.ToString()));
            command.Parameters.Add(new SqliteParameter("date", timesheet.Date.IsoString()));
            command.Parameters.Add(new SqliteParameter("date_ts", timesheet.Date.IsoString()));
            command.Parameters.Add(new SqliteParameter("qoffset", slot.Offset));
            command.Parameters.Add(new SqliteParameter("duration", slot.Duration));
            command.Parameters.Add(new SqliteParameter("created", slot.Created.IsoString()));
            command.Parameters.Add(new SqliteParameter("created_ts", slot.Created.IsoString()));
            command.Parameters.Add(new SqliteParameter(UserIdColumnName, userId.Id.ToString()));
            await command.ExecuteNonQueryAsync(ct);
        }
    }

    private async Task<IList<ActivityTimeSlot>> ReadTimeSlots(SqliteConnection connection, IdOf<Timesheet> id, CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT projectid, activityid, qoffset, duration, created_ts FROM {SlotTableName} WHERE id=@id AND {UserIdColumnName}=@{UserIdColumnName};";
        command.Parameters.Add(new SqliteParameter("id", id.Id.ToString()));
        command.Parameters.Add(WithAccessCondition()!);

        var slots = new List<ActivityTimeSlot>();
        await using var reader = await command.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var projectId = IdOf<Project>.Of(Guid.Parse(reader.GetString(0)));
            var activityId = IdOf<Activity>.Of(Guid.Parse(reader.GetString(1)));
            var offset = Convert.ToInt32((long)reader[2]);
            var duration = Convert.ToInt32((long)reader[3]);
            var createdStr = reader.IsDBNull(4) ? null : reader.GetString(4);
            var created = createdStr is null
                ? UtcDateTime.MinValue
                : new UtcDateTime(DateTime.Parse(createdStr).ToUniversalTime());
            slots.Add(new ActivityTimeSlot(projectId, activityId, offset, duration, created));
        }

        return slots;
    }
}
