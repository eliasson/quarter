using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Commands;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Repositories;

namespace Quarter.Core.UnitTest.Commands;

[TestFixture]
public class RestoreProjectCommandTest : CommandTestBase
{
    public class WhenProjectDoesNotExist : RestoreProjectCommandTest
    {
        [Test]
        public void ItShouldFail()
        {
            var command = new RestoreProjectCommand(IdOf<Project>.Random());
            Assert.ThrowsAsync<NotFoundException>(() => Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None));
        }
    }

    public class WhenProjectExist : RemoveProjectCommandTest
    {
        private Project _initialProject;
        private IProjectRepository _projectRepository;

        [OneTimeSetUp]
        public async Task AddingInitialProject()
        {
            _projectRepository = RepositoryFactory.ProjectRepository(ActingUser);
            var project = new Project("a", "a");
            project.Archive();
            _initialProject = await _projectRepository.CreateAsync(project, CancellationToken.None);
            var command = new RestoreProjectCommand(_initialProject.Id);

            await Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None);
        }

        [Test]
        public async Task ItShouldHaveMarkedProjectAsArchived()
        {
            var activity = await _projectRepository.GetByIdAsync(_initialProject.Id, CancellationToken.None);
            Assert.That(activity.IsArchived, Is.False);
        }
    }
}
