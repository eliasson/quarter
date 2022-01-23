using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Commands;
using Quarter.Core.Events;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;

namespace Quarter.Core.UnitTest.Commands;

public class EditProjectCommandTest : CommandTestBase<ProjectEditedEvent>
{
    public class WhenProjectDoesNotExist : EditProjectCommandTest
    {
        [Test]
        public void ItShouldFail()
        {
            var command = new EditProjectCommand(IdOf<Project>.Random(), null, null);
            Assert.ThrowsAsync<NotFoundException>(() => Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None));
        }

        [Test]
        public void ItShouldNotDispatchAnyEvent()
            => Assert.That(EventSubscriber.CollectedEvents, Is.Empty);
    }

    public class WhenEditingProjectName : EditProjectCommandTest
    {
        public static IEnumerable<object[]> EditTests()
        {
            yield return new object[]
            {
                null, null,
                "Initial name", "Initial description"
            };

            yield return new object[]
            {
                "Updated name", "Updated description",
                "Updated name", "Updated description"
            };
        }

        [TestCaseSource(nameof(EditTests))]
        public async Task ItShouldHaveFieldValue(string name, string description, string expectedName, string expectedDescription)
        {
            var projectRepository = RepositoryFactory.ProjectRepository(ActingUser);
            var initialProject = await projectRepository.CreateAsync(new Project("Initial name", "Initial description"),
                CancellationToken.None);
            var command = new EditProjectCommand(initialProject.Id, name, description);
            await Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None);

            var readProject = await projectRepository.GetByIdAsync(initialProject.Id, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(readProject.Name, Is.EqualTo(expectedName));
                Assert.That(readProject.Description, Is.EqualTo(expectedDescription));
            });
        }
    }

    public class SuccessfulEditing : EditProjectCommandTest
    {
        private Project _initialProject;

        [OneTimeSetUp]
        public async Task AddingInitialProject()
        {
            var projectRepository = RepositoryFactory.ProjectRepository(ActingUser);
            _initialProject = await projectRepository.CreateAsync(new Project("Initial name", "Initial description"), CancellationToken.None);
            var command = new EditProjectCommand(_initialProject.Id, "Updated name", null);
            await Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None);
        }

        [Test]
        public async Task ItShouldDispatchEvent()
        {
            var ev = await EventSubscriber.EventuallyDispatchedOneEvent();

            Assert.That(ev.ProjectId, Is.EqualTo(_initialProject.Id));
        }
    }
}