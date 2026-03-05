using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using Microsoft.Data.Sqlite;
using Quarter.Core.Repositories;

namespace Quarter.StartupTasks;

/// <summary>
/// On first boot with the new release, migrates all data from PostgreSQL to SQLite if SQLite is
/// empty and PostgreSQL is reachable. On success, SQLite will be used on next restart.
/// On failure, PostgreSQL continues to be used — the app stays up.
/// </summary>
public class DataMigrationStartupTask(
    IPostgresConnectionProvider postgresProvider,
    ISqliteConnectionProvider sqliteProvider,
    ILogger<DataMigrationStartupTask> logger)
    : IStartupTask
{
    public async Task ExecuteAsync()
    {
        if (!await IsSqliteEmptyAsync())
        {
            logger.LogDebug("SQLite already has data, skipping PostgreSQL → SQLite migration");
            return;
        }

        if (!await IsPostgresReachableAsync())
        {
            logger.LogInformation("PostgreSQL is not reachable, skipping data migration");
            return;
        }

        logger.LogInformation("Starting data migration from PostgreSQL to SQLite...");

        await using var sqliteConn = await sqliteProvider.GetConnectionAsync(CancellationToken.None);
        await using var sqliteTx = await sqliteConn.BeginTransactionAsync();

        try
        {
            await using var pgConn = await postgresProvider.GetConnectionAsync(CancellationToken.None);
            await CopyQUserTable(pgConn, sqliteConn, CancellationToken.None);
            await CopyProjectTable(pgConn, sqliteConn, CancellationToken.None);
            await CopyActivityTable(pgConn, sqliteConn, CancellationToken.None);
            await CopyTimesheetTable(pgConn, sqliteConn, CancellationToken.None);
            await CopyTimeslotTable(pgConn, sqliteConn, CancellationToken.None);

            await sqliteTx.CommitAsync();
            logger.LogInformation("Data migration completed successfully. SQLite will be used on next restart.");
        }
        catch (Exception ex)
        {
            await sqliteTx.RollbackAsync();
            logger.LogError(ex, "Data migration from PostgreSQL to SQLite failed. Continuing with PostgreSQL.");
        }
    }

    private async Task<bool> IsSqliteEmptyAsync()
    {
        try
        {
            await using var conn = await sqliteProvider.GetConnectionAsync(CancellationToken.None);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM quser";
            return Convert.ToInt64(await cmd.ExecuteScalarAsync()) == 0;
        }
        catch
        {
            return true;
        }
    }

    private async Task<bool> IsPostgresReachableAsync()
    {
        try
        {
            await using var conn = await postgresProvider.GetConnectionAsync(CancellationToken.None);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT 1";
            await cmd.ExecuteScalarAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static async Task CopyQUserTable(NpgsqlConnection pgConn, SqliteConnection sqliteConn, CancellationToken ct)
    {
        await using var selectCmd = pgConn.CreateCommand();
        selectCmd.CommandText = "SELECT id, data, email FROM quser";
        await using var reader = await selectCmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var id = ((Guid)reader[0]).ToString();
            var data = reader.GetString(1);
            var email = reader.GetString(2);

            await using var insertCmd = sqliteConn.CreateCommand();
            insertCmd.CommandText = "INSERT OR IGNORE INTO quser (id, data, email) VALUES (@id, @data, @email)";
            insertCmd.Parameters.AddWithValue("@id", id);
            insertCmd.Parameters.AddWithValue("@data", data);
            insertCmd.Parameters.AddWithValue("@email", email);
            await insertCmd.ExecuteNonQueryAsync(ct);
        }
    }

    private static async Task CopyProjectTable(NpgsqlConnection pgConn, SqliteConnection sqliteConn, CancellationToken ct)
    {
        await using var selectCmd = pgConn.CreateCommand();
        selectCmd.CommandText = "SELECT id, data, userid FROM project";
        await using var reader = await selectCmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var id = ((Guid)reader[0]).ToString();
            var data = reader.GetString(1);
            var userid = ((Guid)reader[2]).ToString();

            await using var insertCmd = sqliteConn.CreateCommand();
            insertCmd.CommandText = "INSERT OR IGNORE INTO project (id, data, userid) VALUES (@id, @data, @userid)";
            insertCmd.Parameters.AddWithValue("@id", id);
            insertCmd.Parameters.AddWithValue("@data", data);
            insertCmd.Parameters.AddWithValue("@userid", userid);
            await insertCmd.ExecuteNonQueryAsync(ct);
        }
    }

    private static async Task CopyActivityTable(NpgsqlConnection pgConn, SqliteConnection sqliteConn, CancellationToken ct)
    {
        await using var selectCmd = pgConn.CreateCommand();
        selectCmd.CommandText = "SELECT id, data, userid FROM activity";
        await using var reader = await selectCmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var id = ((Guid)reader[0]).ToString();
            var data = reader.GetString(1);
            var userid = ((Guid)reader[2]).ToString();

            await using var insertCmd = sqliteConn.CreateCommand();
            insertCmd.CommandText = "INSERT OR IGNORE INTO activity (id, data, userid) VALUES (@id, @data, @userid)";
            insertCmd.Parameters.AddWithValue("@id", id);
            insertCmd.Parameters.AddWithValue("@data", data);
            insertCmd.Parameters.AddWithValue("@userid", userid);
            await insertCmd.ExecuteNonQueryAsync(ct);
        }
    }

    private static async Task CopyTimesheetTable(NpgsqlConnection pgConn, SqliteConnection sqliteConn, CancellationToken ct)
    {
        await using var selectCmd = pgConn.CreateCommand();
        selectCmd.CommandText = "SELECT id, data, date, date_ts, userid FROM timesheet";
        await using var reader = await selectCmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var id = ((Guid)reader[0]).ToString();
            var data = reader.GetString(1);
            var date = reader.GetString(2);
            var dateTs = reader.IsDBNull(3) ? (object)DBNull.Value : ((DateTime)reader[3]).ToString("yyyy-MM-dd");
            var userid = ((Guid)reader[4]).ToString();

            await using var insertCmd = sqliteConn.CreateCommand();
            insertCmd.CommandText = "INSERT OR IGNORE INTO timesheet (id, data, date, date_ts, userid) VALUES (@id, @data, @date, @date_ts, @userid)";
            insertCmd.Parameters.AddWithValue("@id", id);
            insertCmd.Parameters.AddWithValue("@data", data);
            insertCmd.Parameters.AddWithValue("@date", date);
            insertCmd.Parameters.AddWithValue("@date_ts", dateTs);
            insertCmd.Parameters.AddWithValue("@userid", userid);
            await insertCmd.ExecuteNonQueryAsync(ct);
        }
    }

    private static async Task CopyTimeslotTable(NpgsqlConnection pgConn, SqliteConnection sqliteConn, CancellationToken ct)
    {
        await using var selectCmd = pgConn.CreateCommand();
        selectCmd.CommandText = "SELECT id, projectid, activityid, date, date_ts, qoffset, duration, created, created_ts, userid FROM timeslot";
        await using var reader = await selectCmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var id = ((Guid)reader[0]).ToString();
            var projectid = ((Guid)reader[1]).ToString();
            var activityid = ((Guid)reader[2]).ToString();
            var date = reader.GetString(3);
            var dateTs = reader.IsDBNull(4) ? (object)DBNull.Value : ((DateTime)reader[4]).ToString("yyyy-MM-dd");
            var qoffset = Convert.ToInt32((short)reader[5]);
            var duration = Convert.ToInt32((short)reader[6]);
            var created = reader.GetString(7);
            var createdTs = reader.IsDBNull(8) ? (object)DBNull.Value : ((DateTime)reader[8]).ToString("o");
            var userid = ((Guid)reader[9]).ToString();

            await using var insertCmd = sqliteConn.CreateCommand();
            insertCmd.CommandText = "INSERT OR IGNORE INTO timeslot (id, projectid, activityid, date, date_ts, qoffset, duration, created, created_ts, userid) VALUES (@id, @projectid, @activityid, @date, @date_ts, @qoffset, @duration, @created, @created_ts, @userid)";
            insertCmd.Parameters.AddWithValue("@id", id);
            insertCmd.Parameters.AddWithValue("@projectid", projectid);
            insertCmd.Parameters.AddWithValue("@activityid", activityid);
            insertCmd.Parameters.AddWithValue("@date", date);
            insertCmd.Parameters.AddWithValue("@date_ts", dateTs);
            insertCmd.Parameters.AddWithValue("@qoffset", qoffset);
            insertCmd.Parameters.AddWithValue("@duration", duration);
            insertCmd.Parameters.AddWithValue("@created", created);
            insertCmd.Parameters.AddWithValue("@created_ts", createdTs);
            insertCmd.Parameters.AddWithValue("@userid", userid);
            await insertCmd.ExecuteNonQueryAsync(ct);
        }
    }
}
