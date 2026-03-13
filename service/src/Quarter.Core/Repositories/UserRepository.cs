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

public interface IUserRepository : IRepository<User>
{
    Task<User> GetUserByEmailAsync(string email, CancellationToken ct);
}

public class InMemoryUserRepository : InMemoryRepositoryBase<User>, IUserRepository
{
    public Task<User> GetUserByEmailAsync(string email, CancellationToken ct)
    {
        var maybeUser = Storage.Values.FirstOrDefault(u => u.Email.Value == email.ToLowerInvariant());

        return maybeUser is null
            ? throw new NotFoundException($"Could not find user with email {email}")
            : Task.FromResult(maybeUser);
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

    protected override IEnumerable<string> AdditionalColumns()
        => new[] { EmailColumnName };

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
}
