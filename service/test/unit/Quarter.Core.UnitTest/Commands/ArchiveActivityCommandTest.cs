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
public class ArchiveActivityCommandTest : CommandTestBase
{
    public class WhenActivityDoesNotExist : EditActivityCommandTest
    {
        [Test]
        public void ItShouldFail()
        {
            var command = new ArchiveActivityCommand(IdOf<Activity>.Random());
            Assert.ThrowsAsync<NotFoundException>(() => Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None));
        }
    }

    public class WhenActivityExist : ArchiveActivityCommandTest
    {
        private Activity _initialActivity;
        private IActivityRepository _activityRepository;

        [OneTimeSetUp]
        public async Task AddingInitialProject()
        {
            _activityRepository = RepositoryFactory.ActivityRepository(ActingUser);
            var activity = new Activity(IdOf<Project>.Random(), "a", "a", Color.FromHexString("#123"));
            _initialActivity = await _activityRepository.CreateAsync(activity, CancellationToken.None);
            var command = new ArchiveActivityCommand(_initialActivity.Id);

            await Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None);
        }

        [Test]
        public async Task ItShouldHaveMarkedActivityAsArchived()
        {
            var activity = await _activityRepository.GetByIdAsync(_initialActivity.Id, CancellationToken.None);
            Assert.That(activity.IsArchived, Is.True);
        }
    }

    public class WhenTimeIsRegistered : ArchiveActivityCommandTest
    {
        private Activity _activityOne;
        private Activity _activityTwo;
        private Date _dateInTest;

        [OneTimeSetUp]
        public async Task RegisterTimeForActivity()
        {
            _dateInTest = Date.Today();
            _activityOne = await CreateActivityAsync(IdOf<Project>.Random(), "One");
            _activityTwo = await CreateActivityAsync(IdOf<Project>.Random(), "Two");
            await RegisterTimeAsync(_dateInTest, _activityOne, 0, 4);
            await RegisterTimeAsync(_dateInTest, _activityTwo, 10, 4);

            var command = new ArchiveActivityCommand(_activityOne.Id);

            await Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None);
        }

        [Test]
        public async Task ItShouldStillIncludeTimeslotsInTimesheet()
        {
            var ts = await GetTimesheetAsync(_dateInTest);
            var slotActivities = ts.Slots().Select(s => s.ActivityId);

            Assert.That(slotActivities, Is.EqualTo(new[] { _activityOne.Id, _activityTwo.Id }));
        }
    }
}
