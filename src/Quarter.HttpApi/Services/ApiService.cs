using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;
using Quarter.HttpApi.Resources;

namespace Quarter.HttpApi.Services;

public interface IApiService
{
    IAsyncEnumerable<ProjectResourceOutput> ProjectsForUserAsync(OperationContext oc, CancellationToken ct);
    Task<ProjectResourceOutput> CreateProjectAsync(ProjectResourceInput input, OperationContext oc, CancellationToken ct);
    Task<ProjectResourceOutput> UpdateProjectAsync(IdOf<Project> projectId, ProjectResourceInput input, OperationContext oc, CancellationToken ct);
    Task DeleteProjectAsync(IdOf<Project> projectId, OperationContext oc, CancellationToken ct);
    IAsyncEnumerable<ActivityResourceOutput> ActivitiesForProject(IdOf<Project> projectId, OperationContext oc, CancellationToken ct);
    Task<ActivityResourceOutput> CreateActivityAsync(IdOf<Project> projectId, CreateActivityResourceInput input, OperationContext oc, CancellationToken ct);
    Task<ActivityResourceOutput> UpdateActivityAsync(IdOf<Project> projectId, IdOf<Activity> activityId, UpdateActivityResourceInput input, OperationContext oc, CancellationToken ct);
    Task DeleteActivityAsync(IdOf<Project> projectId, IdOf<Activity> activityId, OperationContext oc, CancellationToken ct);
}

public class ApiService : IApiService
{
    private readonly IRepositoryFactory _repositoryFactory;

    public ApiService(IRepositoryFactory repositoryFactory)
    {
        _repositoryFactory = repositoryFactory;
    }

    public IAsyncEnumerable<ProjectResourceOutput> ProjectsForUserAsync(OperationContext oc, CancellationToken ct)
    {
        var projectRepository = _repositoryFactory.ProjectRepository(oc.UserId);
        return projectRepository.GetAllAsync(ct).Select(ProjectResourceOutput.From);
    }

    public async Task<ProjectResourceOutput> CreateProjectAsync(ProjectResourceInput input, OperationContext oc, CancellationToken ct)
    {
        var project = new Project(input.name!, input.description!);
        project =  await _repositoryFactory.ProjectRepository(oc.UserId).CreateAsync(project, ct);
        return ProjectResourceOutput.From(project);
    }

    public async Task<ProjectResourceOutput> UpdateProjectAsync(IdOf<Project> projectId, ProjectResourceInput input, OperationContext oc, CancellationToken ct)
    {
        var project =  await _repositoryFactory.ProjectRepository(oc.UserId).UpdateByIdAsync(projectId, existing =>
        {
            if (input.name is not null) existing.Name = input.name;
            if (input.description is not null) existing.Description = input.description;
            return existing;
        }, ct);
        return ProjectResourceOutput.From(project);
    }

    public Task DeleteProjectAsync(IdOf<Project> projectId, OperationContext oc, CancellationToken ct)
    {
        return _repositoryFactory.ProjectRepository(oc.UserId).RemoveByIdAsync(projectId, ct);
    }

    public IAsyncEnumerable<ActivityResourceOutput> ActivitiesForProject(IdOf<Project> projectId, OperationContext oc, CancellationToken ct)
    {
        var activityRepository = _repositoryFactory.ActivityRepository(oc.UserId);
        return activityRepository.GetAllForProjectAsync(projectId, ct).Select(ActivityResourceOutput.From);
    }

    public async Task<ActivityResourceOutput> CreateActivityAsync(IdOf<Project> projectId, CreateActivityResourceInput input, OperationContext oc, CancellationToken ct)
    {
        var activityRepository = _repositoryFactory.ActivityRepository(oc.UserId);
        var projectRepository = _repositoryFactory.ProjectRepository(oc.UserId);

        // This will throw if the project does not exist (which is also the case if the user does not own the given project ID)
        _ = await projectRepository.GetByIdAsync(projectId, ct);

        var activity = await activityRepository.CreateAsync(input.ToActivity(projectId), ct);
        return ActivityResourceOutput.From(activity);
    }

    public async Task<ActivityResourceOutput> UpdateActivityAsync(IdOf<Project> projectId, IdOf<Activity> activityId, UpdateActivityResourceInput input, OperationContext oc, CancellationToken ct)
    {
        var activityRepository = _repositoryFactory.ActivityRepository(oc.UserId);
        var projectRepository = _repositoryFactory.ProjectRepository(oc.UserId);

        // This will throw if the project does not exist (which is also the case if the user does not own the given project ID)
        _ = await projectRepository.GetByIdAsync(projectId, ct);

        var activity = await activityRepository.UpdateByIdAsync(activityId, existing =>
        {
            if (input.name is not null) existing.Name = input.name;
            if (input.description is not null) existing.Description = input.description;
            if (input.color is not null) existing.Color = Color.FromHexString(input.color);
            return existing;
        }, ct);

        return ActivityResourceOutput.From(activity);
    }

    public async Task DeleteActivityAsync(IdOf<Project> projectId, IdOf<Activity> activityId, OperationContext oc, CancellationToken ct)
    {
        var activityRepository = _repositoryFactory.ActivityRepository(oc.UserId);
        var projectRepository = _repositoryFactory.ProjectRepository(oc.UserId);

        // This will throw if the project does not exist (which is also the case if the user does not own the given project ID)
        _ = await projectRepository.GetByIdAsync(projectId, ct);

        await activityRepository.RemoveByIdAsync(activityId, ct);
    }
}