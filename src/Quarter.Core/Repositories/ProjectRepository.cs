using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Quarter.Core.Models;

namespace Quarter.Core.Repositories;

public interface IProjectRepository : IRepository<Project>
{
    Task<Project> CreateSandboxProjectAsync(CancellationToken ct);
}

public static class ProjectRepository
{
    public static Task<Project> CreateSandboxProjectAsync(IProjectRepository self, CancellationToken ct)
    {
        var project = new Project("Your first project", "A project is used to group activities.");
        return self.CreateAsync(project, ct);
    }
}

public class InMemoryProjectRepository : InMemoryRepositoryBase<Project>, IProjectRepository
{
    public Task<Project> CreateSandboxProjectAsync(CancellationToken ct)
        => ProjectRepository.CreateSandboxProjectAsync(this, ct);
}

public class PostgresProjectRepository : PostgresRepositoryBase<Project>, IProjectRepository
{
    private readonly IdOf<User> _userId;

    private const string TableName = "project";
    private const string AggregateName = "Project";
    private const string UserIdColumnName = "userid";

    public PostgresProjectRepository(IPostgresConnectionProvider connectionProvider, IdOf<User> userId)
        : base(connectionProvider, TableName, AggregateName)
    {
        _userId = userId;
    }

    protected override IEnumerable<string> AdditionalColumns()
        => new[] { UserIdColumnName };

    protected override object AdditionalColumnValue(string columnName, Project aggregate)
        => columnName switch
        {
            UserIdColumnName => _userId.Id,
            _ => throw new NotImplementedException($"Additional column named [{columnName}] is not implemented"),
        };

    protected override NpgsqlParameter? WithAccessCondition()
        => new NpgsqlParameter(UserIdColumnName, _userId.Id);

    public Task<Project> CreateSandboxProjectAsync(CancellationToken ct)
        => ProjectRepository.CreateSandboxProjectAsync(this, ct);
}
