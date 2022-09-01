using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Commands;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;

namespace Quarter.Core.UnitTest.Commands;

[TestFixture]
public class RemoveProjectCommandTest : CommandTestBase
{
    public class WhenProjectDoesNotExist : RemoveProjectCommandTest
    {
        [Test]
        public void ItShouldNotFail()
        {
            var command = new RemoveProjectCommand(IdOf<Project>.Random());
            Assert.DoesNotThrowAsync(() => Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None));
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
            _initialProject = await _projectRepository.CreateAsync(new Project("a", "a"), CancellationToken.None);
            var command = new RemoveProjectCommand(_initialProject.Id);

            await Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None);
        }

        [Test]
        public void ItShouldHaveRemovedProject()
            => Assert.ThrowsAsync<NotFoundException>(() => _projectRepository.GetByIdAsync(_initialProject.Id, CancellationToken.None));
    }

    public class WhenTimeIsRegistered : RemoveProjectCommandTest
    {
        private Project _projectOne;
        private Project _projectTwo;
        private Activity _activityOne;
        private Activity _activityTwo;
        private Date _dateInTest;

        [OneTimeSetUp]
        public async Task RegisterTimeForProject()
        {
            _dateInTest = Date.Today();
            _projectOne = await CreateProjectAsync("One");
            _projectTwo = await CreateProjectAsync("Two");
            _activityOne = await CreateActivityAsync(_projectOne.Id, "One");
            _activityTwo = await CreateActivityAsync(_projectTwo.Id, "Two");

            await RegisterTimeAsync(_dateInTest, _activityOne, 0, 4);
            await RegisterTimeAsync(_dateInTest, _activityTwo, 10, 4);

            var command = new RemoveProjectCommand(_projectOne.Id);

            await Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None);
        }

        [Test]
        public async Task ItShouldNotIncludeTimeSlotForDeletedProjectInTimesheet()
        {
            var ts = await GetTimesheetAsync(_dateInTest);
            var slotProjects = ts.Slots().Select(s => s.ProjectId);
            Assert.That(slotProjects, Is.EqualTo(new [] { _projectTwo.Id }));
        }
    }
}