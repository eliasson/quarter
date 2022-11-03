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
}