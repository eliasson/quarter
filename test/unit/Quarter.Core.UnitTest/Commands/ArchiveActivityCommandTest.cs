using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Commands;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;

namespace Quarter.Core.UnitTest.Commands;

public abstract class ArchiveActivityCommandTest : CommandTestBase
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
}