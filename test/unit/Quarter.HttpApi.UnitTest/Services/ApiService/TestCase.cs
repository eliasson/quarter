using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;
using Quarter.HttpApi.Services;

namespace Quarter.HttpApi.UnitTest.Services;

public class TestCase
{
    protected readonly IApiService ApiService;
    private readonly InMemoryRepositoryFactory _repositoryFactory;

    protected TestCase()
    {
        _repositoryFactory = new InMemoryRepositoryFactory();
        ApiService = new ApiService(_repositoryFactory);
    }

    protected Task<Project> AddProject(IdOf<User> userId, string name)
    {
        var project = new Project(name, $"description:{name}");
        return _repositoryFactory.ProjectRepository(userId).CreateAsync(project, CancellationToken.None);
    }

    protected Task<Activity> AddActivity(IdOf<User> userId, IdOf<Project> projectId, string name)
    {
        var activity = new Activity(projectId, name, $"description:{name}", Color.FromHexString("#FFFFFF"));
        return _repositoryFactory.ActivityRepository(userId).CreateAsync(activity, CancellationToken.None);
    }

    protected Task<Project> ReadProjectAsync(IdOf<User> userId, IdOf<Project> projectId)
        => _repositoryFactory.ProjectRepository(userId).GetByIdAsync(projectId, CancellationToken.None);

    protected ValueTask<List<Project>> ReadProjectsAsync(IdOf<User> userId)
        => _repositoryFactory.ProjectRepository(userId)
            .GetAllAsync(CancellationToken.None)
            .ToListAsync(CancellationToken.None);

    protected Task<Activity> ReadActivityAsync(IdOf<User> userId, IdOf<Project> projectId, IdOf<Activity> activityId)
        => _repositoryFactory.ActivityRepository(userId)
            .GetByIdAsync(activityId, CancellationToken.None);

    protected ValueTask<List<Activity>> ReadActivitiesAsync(IdOf<User> userId, IdOf<Project> projectId)
        => _repositoryFactory.ActivityRepository(userId)
            .GetAllForProjectAsync(projectId, CancellationToken.None)
            .ToListAsync(CancellationToken.None);

    protected Task AddTimesheetAsync(IdOf<User> userId, Timesheet timesheet)
        => _repositoryFactory.TimesheetRepository(userId).CreateAsync(timesheet, CancellationToken.None);

    protected Task<Timesheet> ReadTimesheetAsync(IdOf<User> userId, Date date)
        => _repositoryFactory.TimesheetRepository(userId).GetByDateAsync(date, CancellationToken.None);

    protected static OperationContext CreateOperationContext()
        => new (IdOf<User>.Random());
}