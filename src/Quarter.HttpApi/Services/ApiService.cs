using Quarter.Core.Repositories;
using Quarter.Core.Utils;
using Quarter.HttpApi.Resources;

namespace Quarter.HttpApi.Services;

public interface IApiService
{
    IAsyncEnumerable<ProjectResourceOutput> AllForUserAsync(OperationContext oc, CancellationToken ct);
}

public class ApiService : IApiService
{
    private readonly IRepositoryFactory _repositoryFactory;

    public ApiService(IRepositoryFactory repositoryFactory)
    {
        _repositoryFactory = repositoryFactory;
    }
    public IAsyncEnumerable<ProjectResourceOutput> AllForUserAsync(OperationContext oc, CancellationToken ct)
    {
        var projectRepository = _repositoryFactory.ProjectRepository(oc.UserId);
        return projectRepository.GetAllAsync(ct).Select(ProjectResourceOutput.From);
    }
}