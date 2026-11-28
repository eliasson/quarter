using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.Core.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User> GetUserByEmailAsync(string email, CancellationToken ct);

    /// <summary>
    /// Record that the user logged in (or was active) at the given timestamp.
    ///
    /// A timestamp older than the one already stored is ignored.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="timestamp">When the user was active</param>
    /// <param name="ct">Token used to cancel the operation.</param>
    Task RecordUserLoginAsync(IdOf<User> userId, UtcDateTime timestamp, CancellationToken ct);

    /// <summary>
    /// Get the last login for all users that have logged in at least once.
    /// </summary>
    /// <param name="ct">Token used to cancel the operation.</param>
    Task<ReadOnlyDictionary<IdOf<User>, UtcDateTime>> GetLastLoginsAsync(CancellationToken ct);
}

public class InMemoryUserRepository : InMemoryRepositoryBase<User>, IUserRepository
{
    private readonly ConcurrentDictionary<IdOf<User>, UtcDateTime> _lastLogins = new();

    public Task<User> GetUserByEmailAsync(string email, CancellationToken ct)
    {
        var maybeUser = Storage.Values.FirstOrDefault(u => u.Email.Value == email.ToLowerInvariant());

        return maybeUser is null
            ? throw new NotFoundException($"Could not find user with email {email}")
            : Task.FromResult(maybeUser);
    }

    public Task RecordUserLoginAsync(IdOf<User> userId, UtcDateTime timestamp, CancellationToken ct)
    {
        if (Storage.ContainsKey(userId))
            _lastLogins.AddOrUpdate(userId, timestamp,
                (_, existing) => timestamp.DateTime > existing.DateTime ? timestamp : existing);

        return Task.CompletedTask;
    }

    public Task<ReadOnlyDictionary<IdOf<User>, UtcDateTime>> GetLastLoginsAsync(CancellationToken ct)
    {
        var result = _lastLogins
            .Where(kv => Storage.ContainsKey(kv.Key))
            .ToDictionary(kv => kv.Key, kv => kv.Value)
            .AsReadOnly();

        return Task.FromResult(result);
    }

    public override Task Truncate(CancellationToken ct)
    {
        _lastLogins.Clear();
        return base.Truncate(ct);
    }

    protected override void CheckConstraints(User aggregate)
    {
        if (Storage.Values.Any(u => Equals(u.Email, aggregate.Email) && !Equals(u.Id, aggregate.Id)))
            throw new ArgumentException("Could not store as Email already in use");
    }
}

public class PostgresUserRepository(IPostgresConnectionProvider connectionProvider)
    : PostgresRepositoryBase<User>(connectionProvider, TableName, AggregateName), IUserRepository
{
    private const string TableName = "quser"; // "users" is reserved word in Postgres
    private const string AggregateName = "User";
    private const string EmailColumnName = "email";
    private const string LastLoginColumnName = "last_login_ts";

    protected override IEnumerable<string> AdditionalColumns()
        => [EmailColumnName];

    protected override object AdditionalColumnValue(string columnName, User aggregate)
        => columnName switch
        {
            EmailColumnName => aggregate.Email.Value.ToLowerInvariant(),
            _ => throw new NotImplementedException($"Additional column named [{columnName}] is not implemented"),
        };

    public async Task<User> GetUserByEmailAsync(string email, CancellationToken ct)
    {
        var query = $"SELECT data FROM {TableName} WHERE email=@email";
        var parameters = new List<NpgsqlParameter>
        {
            new ("@email", email.ToLowerInvariant())
        };
        await using var connection = await _connectionProvider.GetConnectionAsync(ct);
        var result = await ExecuteQueryAsync(connection, query, ct, parameters).ToListAsync(ct);

        return result.FirstOrThrow(new NotFoundException($"User with email {email} was not found"));
    }

    public async Task RecordUserLoginAsync(IdOf<User> userId, UtcDateTime timestamp, CancellationToken ct)
    {
        await using var connection = await _connectionProvider.GetConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        command.CommandText =
            $"UPDATE {TableName} SET {LastLoginColumnName}=@timestamp " +
            $"WHERE id=@id AND ({LastLoginColumnName} IS NULL OR {LastLoginColumnName} < @timestamp)";

        command.Parameters.AddRange(new NpgsqlParameter[]
        {
            new("id", userId.Id),
            new("timestamp", timestamp.DateTime),
        });

        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task<ReadOnlyDictionary<IdOf<User>, UtcDateTime>> GetLastLoginsAsync(CancellationToken ct)
    {
        await using var connection = await _connectionProvider.GetConnectionAsync(ct);
        await using var command = connection.CreateCommand();

        command.CommandText =
            $"SELECT id, {LastLoginColumnName} FROM {TableName} WHERE {LastLoginColumnName} IS NOT NULL";

        var result = new Dictionary<IdOf<User>, UtcDateTime>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var userId = IdOf<User>.Of(reader.GetGuid(0));
            result[userId] = new UtcDateTime(reader.GetFieldValue<DateTime>(1));
        }

        return new ReadOnlyDictionary<IdOf<User>, UtcDateTime>(result);
    }
}
