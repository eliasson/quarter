using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Commands;
using Quarter.Core.Events;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;

namespace Quarter.Core.UnitTest.Commands;

public class RemoveActivityCommandTest : CommandTestBase<ActivityRemovedEvent>
{
    public class WhenActivityDoesNotExist : RemoveActivityCommandTest
    {
        [Test]
        public void ItShouldNotFail()
        {
            var command = new RemoveActivityCommand(IdOf<Activity>.Random());
            Assert.DoesNotThrowAsync(() => Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None));
        }

        [Test]
        public void ItShouldNotDispatchAnyEvent()
            => Assert.That(EventSubscriber.CollectedEvents, Is.Empty);
    }

    public class WhenActivityExist : RemoveActivityCommandTest
    {
        private Activity _initialActivity;
        private IActivityRepository _activityRepository;

        [OneTimeSetUp]
        public async Task AddingInitialProject()
        {
            _activityRepository = RepositoryFactory.ActivityRepository(ActingUser);
            var activity = new Activity(IdOf<Project>.Random(), "a", "a", Color.FromHexString("#123"));
            _initialActivity = await _activityRepository.CreateAsync(activity, CancellationToken.None);
            var command = new RemoveActivityCommand(_initialActivity.Id);

            await Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None);
        }

        [Test]
        public void ItShouldHaveRemovedActivity()
            => Assert.ThrowsAsync<NotFoundException>(() => _activityRepository.GetByIdAsync(_initialActivity.Id, CancellationToken.None));

        [Test]
        public async Task ItShouldDispatchActivityRemovedEvent()
        {
            var ev = await EventSubscriber.EventuallyDispatchedOneEvent();

            Assert.That(ev.ActivityId, Is.EqualTo(_initialActivity.Id));
        }
    }
}