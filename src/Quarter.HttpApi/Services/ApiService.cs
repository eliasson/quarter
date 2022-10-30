using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;
using Quarter.HttpApi.Resources;

namespace Quarter.HttpApi.Services;

public interface IApiService
{
    IAsyncEnumerable<ProjectResourceOutput> ProjectsForUserAsync(OperationContext oc, CancellationToken ct);
    Task CreateProjectAsync(ProjectResourceInput input, OperationContext oc, CancellationToken ct);
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

    public async Task CreateProjectAsync(ProjectResourceInput input, OperationContext oc, CancellationToken ct)
    {
        var project = new Project(input.name!, input.description!);
        await _repositoryFactory.ProjectRepository(oc.UserId).CreateAsync(project, ct);
    }

    public Task DeleteProjectAsync(IdOf<Project> projectId, OperationContext oc, CancellationToken ct)
    {
        return _repositoryFactory.ProjectRepository(oc.UserId).RemoveByIdAsync(projectId, ct);
    }
}