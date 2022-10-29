using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;
using Quarter.HttpApi.Services;

namespace Quarter.HttpApi.UnitTest.Services;

[TestFixture]
public class ApiServiceTest
{
    public class WhenNoProjectsExistForUser : TestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();

        [Test]
        public async Task ItShouldReturnAnEmptyResultForProjects()
        {
            var projects = await ApiService.AllForUserAsync(_oc, CancellationToken.None).ToListAsync();
            Assert.That(projects, Is.Empty);
        }
    }

    public class WhenUserHasProjects : TestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();

        [OneTimeSetUp]
        public async Task Setup()
        {
            await AddProject(_oc.UserId, "Project alpha");
        }

        [Test]
        public async Task ItShouldContainProject()
        {
            var projectNames = await ApiService.AllForUserAsync(_oc, CancellationToken.None)
                .Select(p => p.name)
                .ToListAsync();
            Assert.That(projectNames, Does.Contain("Project alpha"));
        }
    }

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
}