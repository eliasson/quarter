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

    protected async Task AddProject(IdOf<User> userId, string name)
    {
        var project = new Project(name, $"description:{name}");
        await _repositoryFactory.ProjectRepository(userId).CreateAsync(project, CancellationToken.None);
    }

    protected static OperationContext CreateOperationContext()
        => new (IdOf<User>.Random());
}