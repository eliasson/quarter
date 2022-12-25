using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.HttpApi.UnitTest.Services;

[TestFixture]
public class DeleteActivityTest
{
    public class WhenProjectIsOwnedUser : TestCase
    {
        private OperationContext? _oc;
        private Project? _project;
        private Activity? _activity;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _oc = CreateOperationContext();
            _project = await AddProject(_oc.UserId, "Project alpha");
            _activity = await AddActivity(_oc.UserId, _project.Id, "Activity alpha");

            await ApiService.DeleteActivityAsync(_project.Id, _activity.Id, _oc, CancellationToken.None);
        }

        [Test]
        public void ItShouldHaveDeletedTheActivity()
            => Assert.ThrowsAsync<NotFoundException>(() => ReadActivityAsync(_oc!.UserId, _project!.Id, _activity!.Id));
    }

    public class WhenProjectIsOwnedByOtherUser : TestCase
    {
        [Test]
        public async Task ItShouldThrowNotFoundException()
        {
            var oc = CreateOperationContext();
            var project = await AddProject(IdOf<User>.Random(), "Project alpha");
            var activity = await AddActivity(oc.UserId, project.Id, "Activity alpha");

            Assert.CatchAsync<NotFoundException>(() => ApiService.DeleteActivityAsync(project.Id, activity.Id, oc, CancellationToken.None));
        }
    }

    public class WhenProjectIsMissing : TestCase
    {
        [Test]
        public void ItShouldThrowNotFoundException()
        {
            var oc = CreateOperationContext();

            Assert.CatchAsync<NotFoundException>(() =>
                ApiService.DeleteActivityAsync(IdOf<Project>.Random(), IdOf<Activity>.Random(), oc, CancellationToken.None));
        }
    }
}