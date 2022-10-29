using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.HttpApi.UnitTest.Services;

[TestFixture]
public class DeleteProjectTest
{
    public class WhenUserHasProjects : TestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();
        private Project? _project;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _project = await AddProject(_oc.UserId, "Project alpha");
            await ApiService.DeleteProjectAsync(_project.Id, _oc, CancellationToken.None);
        }

        [Test]
        public void ItShouldHaveDeletedProject()
            => Assert.ThrowsAsync<NotFoundException>(() => ReadProjectAsync(_oc.UserId, _project!.Id));
    }

    public class WhenNoProjectsExistForUser : TestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();

        [Test]
        public void ItShouldThrowNotFoundError()
        {
            var projectId = IdOf<Project>.Random();
            var ex = Assert.CatchAsync<NotFoundException>(() => ApiService.DeleteProjectAsync(projectId, _oc, CancellationToken.None));
            Assert.That(ex?.Message, Does.Contain($"Found no project with ID \"{projectId.AsString()}\""));
        }
    }
}