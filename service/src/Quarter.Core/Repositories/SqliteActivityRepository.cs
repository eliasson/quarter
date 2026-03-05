using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Quarter.Core.Models;

namespace Quarter.Core.Repositories;

public class SqliteActivityRepository(ISqliteConnectionProvider connectionProvider, IdOf<User> userId)
    : SqliteRepositoryBase<Activity>(connectionProvider, TableName, AggregateName), IActivityRepository
{
    private const string TableName = "activity";
    private const string AggregateName = "Activity";
    private const string UserIdColumnName = "userid";

    protected override IEnumerable<string> AdditionalColumns()
        => new[] { UserIdColumnName };

    protected override object AdditionalColumnValue(string columnName, Activity aggregate)
        => columnName switch
        {
            UserIdColumnName => userId.Id.ToString(),
            _ => throw new NotImplementedException($"Additional column named [{columnName}] is not implemented"),
        };

    protected override SqliteParameter? WithAccessCondition()
        => new(UserIdColumnName, userId.Id.ToString());

    public Task<Activity> CreateSandboxActivityAsync(IdOf<Project> projectId, CancellationToken ct)
        => ActivityRepository.CreateSandboxActivityAsync(this, projectId, ct);

    public IAsyncEnumerable<Activity> GetAllForProjectAsync(IdOf<Project> projectId, CancellationToken ct)
        => GetAllAsync(ct).Where(a => a.ProjectId == projectId);
}
