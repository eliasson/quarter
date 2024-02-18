using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.Core.Repositories;

public interface IActivityRepository : IRepository<Activity>
{
    Task<Activity> CreateSandboxActivityAsync(IdOf<Project> projectId, CancellationToken ct);

    /// <summary>
    /// Retrieve all activities for a specific project
    /// </summary>
    /// <param name="projectId">The ID of the project to retrieve activities for</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>An enumerable of aggregates</returns>
    IAsyncEnumerable<Activity> GetAllForProjectAsync(IdOf<Project> projectId, CancellationToken ct);
}

public static class ActivityRepository
{
    public static Task<Activity> CreateSandboxActivityAsync(IActivityRepository self, IdOf<Project> projectId, CancellationToken ct)
    {
        var activity = new Activity(
            projectId,
            "Your first activity",
            "An activity is used to register time. Each activity has a color to make it easier to associate to.",
            Color.FromHexString("#4e4694"));
        return self.CreateAsync(activity, ct);
    }
}

public class InMemoryActivityRepository : InMemoryRepositoryBase<Activity>, IActivityRepository
{
    public Task<Activity> CreateSandboxActivityAsync(IdOf<Project> projectId, CancellationToken ct)
        => ActivityRepository.CreateSandboxActivityAsync(this, projectId, ct);

    public IAsyncEnumerable<Activity> GetAllForProjectAsync(IdOf<Project> projectId, CancellationToken ct)
        => Storage.Values.Where(a => a.ProjectId == projectId).ToAsyncEnumerable();
}

public class PostgresActivityRepository(IPostgresConnectionProvider connectionProvider, IdOf<User> userId)
    : PostgresRepositoryBase<Activity>(connectionProvider, TableName, AggregateName), IActivityRepository
{
    private const string TableName = "activity";
    private const string AggregateName = "Activity";
    private const string UserIdColumnName = "userid";

    protected override IEnumerable<string> AdditionalColumns()
        => new[] { UserIdColumnName };

    protected override object AdditionalColumnValue(string columnName, Activity aggregate)
        => columnName switch
        {
            UserIdColumnName => userId.Id,
            _ => throw new NotImplementedException($"Additional column named [{columnName}] is not implemented"),
        };

    protected override NpgsqlParameter? WithAccessCondition()
        => new(UserIdColumnName, userId.Id);

    public Task<Activity> CreateSandboxActivityAsync(IdOf<Project> projectId, CancellationToken ct)
        => ActivityRepository.CreateSandboxActivityAsync(this, projectId, ct);

    public IAsyncEnumerable<Activity> GetAllForProjectAsync(IdOf<Project> projectId, CancellationToken ct)
        => GetAllAsync(ct).Where(a => a.ProjectId == projectId);
}
