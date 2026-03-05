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

public class SqliteUserRepository(ISqliteConnectionProvider connectionProvider)
    : SqliteRepositoryBase<User>(connectionProvider, TableName, AggregateName), IUserRepository
{
    private const string TableName = "quser";
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
        var parameters = new List<SqliteParameter>
        {
            new("@email", email.ToLowerInvariant())
        };
        await using var connection = await _connectionProvider.GetConnectionAsync(ct);
        var result = await ExecuteQueryAsync(connection, query, ct, parameters).ToListAsync(ct);

        return result.FirstOrThrow(new NotFoundException($"User with email {email} was not found"));
    }
}
