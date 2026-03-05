using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Quarter.Core.Models;

namespace Quarter.Core.Repositories;

public class SqliteProjectRepository(ISqliteConnectionProvider connectionProvider, IdOf<User> userId)
    : SqliteRepositoryBase<Project>(connectionProvider, TableName, AggregateName), IProjectRepository
{
    private const string TableName = "project";
    private const string AggregateName = "Project";
    private const string UserIdColumnName = "userid";

    protected override IEnumerable<string> AdditionalColumns()
        => new[] { UserIdColumnName };

    protected override object AdditionalColumnValue(string columnName, Project aggregate)
        => columnName switch
        {
            UserIdColumnName => userId.Id.ToString(),
            _ => throw new NotImplementedException($"Additional column named [{columnName}] is not implemented"),
        };

    protected override SqliteParameter? WithAccessCondition()
        => new(UserIdColumnName, userId.Id.ToString());

    public Task<Project> CreateSandboxProjectAsync(CancellationToken ct)
        => ProjectRepository.CreateSandboxProjectAsync(this, ct);
}
