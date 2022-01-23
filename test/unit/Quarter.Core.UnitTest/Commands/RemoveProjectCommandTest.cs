using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Commands;
using Quarter.Core.Events;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Repositories;

namespace Quarter.Core.UnitTest.Commands;

public abstract class RemoveProjectCommandTest : CommandTestBase<ProjectRemovedEvent>
{
    public class WhenProjectDoesNotExist : RemoveProjectCommandTest
    {
        [Test]
        public void ItShouldNotFail()
        {
            var command = new RemoveProjectCommand(IdOf<Project>.Random());
            Assert.DoesNotThrowAsync(() => Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None));
        }

        [Test]
        public void ItShouldNotDispatchAnyEvent()
            => Assert.That(EventSubscriber.CollectedEvents, Is.Empty);
    }

    public class WhenProjectExist : RemoveProjectCommandTest
    {
        private Project _initialProject;
        private IProjectRepository _projectRepository;

        [OneTimeSetUp]
        public async Task AddingInitialProject()
        {
            _projectRepository = RepositoryFactory.ProjectRepository(ActingUser);
            _initialProject = await _projectRepository.CreateAsync(new Project("a", "a"), CancellationToken.None);
            var command = new RemoveProjectCommand(_initialProject.Id);

            await Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None);
        }

        [Test]
        public void ItShouldHaveRemovedProject()
            => Assert.ThrowsAsync<NotFoundException>(() => _projectRepository.GetByIdAsync(_initialProject.Id, CancellationToken.None));

        [Test]
        public async Task ItShouldDispatchProjectRemovedEvent()
        {
            var ev = await EventSubscriber.EventuallyDispatchedOneEvent();

            Assert.That(ev.ProjectId, Is.EqualTo(_initialProject.Id));
        }
    }
}