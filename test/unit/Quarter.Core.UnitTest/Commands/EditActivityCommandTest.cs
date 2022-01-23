using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Commands;
using Quarter.Core.Events;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.Core.UnitTest.Commands;

public abstract class EditActivityCommandTest : CommandTestBase<ActivityEditedEvent>
{
    public class WhenActivityDoesNotExist : EditActivityCommandTest
    {
        [Test]
        public void ItShouldFail()
        {
            var command = new EditActivityCommand(IdOf<Activity>.Random(), null, null, null);
            Assert.ThrowsAsync<NotFoundException>(() => Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None));
        }

        [Test]
        public void ItShouldNotDispatchAnyEvent()
            => Assert.That(EventSubscriber.CollectedEvents, Is.Empty);
    }

    public class WhenEditingActivityName : EditActivityCommandTest
    {
        public static IEnumerable<object[]> EditTests()
        {
            yield return new object[]
            {
                null, null, null,
                "Initial name", "Initial description", Color.FromHexString("#FFFFFF"),
            };

            yield return new object[]
            {
                "Updated name", "Updated description", Color.FromHexString("#000000"),
                "Updated name", "Updated description", Color.FromHexString("#000000"),
            };
        }

        [TestCaseSource(nameof(EditTests))]
        public async Task ItShouldHaveFieldValue(
            string name, string description, Color color,
            string expectedName, string expectedDescription, Color expectedColor)
        {
            var activityRepository = RepositoryFactory.ActivityRepository(ActingUser);
            var initialActivity = await activityRepository.CreateAsync(
                new Activity(IdOf<Project>.Random(), "Initial name", "Initial description", Color.FromHexString("#ffffff"))
                , CancellationToken.None);

            var command = new EditActivityCommand(initialActivity.Id, name, description, color);
            await Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None);

            var readProject = await activityRepository.GetByIdAsync(initialActivity.Id, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(readProject.Name, Is.EqualTo(expectedName));
                Assert.That(readProject.Description, Is.EqualTo(expectedDescription));
                Assert.That(readProject.Color, Is.EqualTo(expectedColor));
            });
        }
    }

    public class SuccessfulEditing : EditActivityCommandTest
    {
        private Activity _initialActivity;

        [OneTimeSetUp]
        public async Task AddingInitialProject()
        {
            var activityRepository = RepositoryFactory.ActivityRepository(ActingUser);
            _initialActivity = await activityRepository.CreateAsync(
                new Activity(IdOf<Project>.Random(), "Initial name", "Initial description", Color.FromHexString("#ffffff"))
                , CancellationToken.None);
            var command = new EditActivityCommand(_initialActivity.Id, "Updated name", null, null);
            await Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None);
        }

        [Test]
        public async Task ItShouldDispatchEvent()
        {
            var ev = await EventSubscriber.EventuallyDispatchedOneEvent();

            Assert.That(ev.ActivityId, Is.EqualTo(_initialActivity.Id));
        }
    }
}