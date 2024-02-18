using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using Quarter.HttpApi.Resources;

namespace Quarter.HttpApi.UnitTest.Services;

[TestFixture]
public class UpdateActivityTest
{
    [TestFixture]
    public class WhenInputIsValid : TestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();
        private ActivityResourceOutput? _output;
        private Project? _project;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _project = await AddProject(_oc.UserId, "Project alpha");
            var activity = await AddActivity(_oc.UserId, _project.Id, "Activity alpha");

            var input = new UpdateActivityResourceInput
            {
                name = "Activity alpha - updated",
                description = "Description alpha - updated",
                color = "#ABCABC",
            };

            _output = await ApiService.UpdateActivityAsync(_project.Id, activity.Id, input, _oc, CancellationToken.None);
        }

        [Test]
        public void ItShouldReturnOutputResourceForActivity()
            => Assert.That(_output?.name, Is.EqualTo("Activity alpha - updated"));

        [Test]
        public async Task ItShouldHaveUpdatedTheActivity()
        {
            var activities = await ReadActivitiesAsync(_oc.UserId, _project!.Id);
            var activity = activities.Single();

            Assert.Multiple(() =>
            {
                Assert.That(activity.Name, Is.EqualTo("Activity alpha - updated"));
                Assert.That(activity.Description, Is.EqualTo("Description alpha - updated"));
                Assert.That(activity.Color.ToHex(), Is.EqualTo("#ABCABC"));
            });
        }
    }

    [TestFixture]
    public class WhenInputIsPartial : TestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();
        private ActivityResourceOutput? _output;
        private Project? _project;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _project = await AddProject(_oc.UserId, "Project alpha");
            var activity = await AddActivity(_oc.UserId, _project.Id, "Activity alpha");

            var input = new UpdateActivityResourceInput
            {
                description = "Description alpha - updated",
            };

            _output = await ApiService.UpdateActivityAsync(_project.Id, activity.Id, input, _oc, CancellationToken.None);
        }

        [Test]
        public async Task ItShouldOnlyHaveUpdatedDescription()
        {
            var activities = await ReadActivitiesAsync(_oc.UserId, _project!.Id);
            var activity = activities.Single();

            Assert.Multiple(() =>
            {
                Assert.That(activity.Name, Is.EqualTo("Activity alpha"));
                Assert.That(activity.Description, Is.EqualTo("Description alpha - updated"));
                Assert.That(activity.Color.ToHex(), Is.EqualTo("#FFFFFF"));
            });
        }
    }

    [TestFixture]
    public class WhenActivityIsMissing : TestCase
    {
        [Test]
        public async Task ItShouldThrowNotFoundException()
        {
            var oc = CreateOperationContext();
            var project = await AddProject(oc.UserId, "Project alpha");
            _ = await AddActivity(oc.UserId, project.Id, "Activity alpha");

            var input = new UpdateActivityResourceInput
            {
                name = "Activity alpha - updated",
                description = "Description alpha",
                color = "#aabbcc",
            };

            Assert.CatchAsync<NotFoundException>(() => ApiService.UpdateActivityAsync(project.Id, IdOf<Activity>.Random(), input, oc, CancellationToken.None));
        }
    }

    [TestFixture]
    public class WhenProjectIsOwnedByOtherUser : TestCase
    {
        [Test]
        public async Task ItShouldThrowNotFoundException()
        {
            var oc = CreateOperationContext();
            var userId = IdOf<User>.Random();
            var project = await AddProject(userId, "Project alpha");
            var activity = await AddActivity(userId, project.Id, "Activity alpha");

            var input = new UpdateActivityResourceInput
            {
                name = "Activity alpha - updated",
                description = "Description alpha",
                color = "#aabbcc",
            };

            Assert.CatchAsync<NotFoundException>(() => ApiService.UpdateActivityAsync(project.Id, activity.Id, input, oc, CancellationToken.None));
        }
    }

    [TestFixture]
    public class WhenProjectIsMissing : TestCase
    {
        [Test]
        public void ItShouldThrowNotFoundException()
        {
            var oc = CreateOperationContext();
            var input = new UpdateActivityResourceInput
            {
                name = "Activity alpha",
                description = "Description alpha",
                color = "#aabbcc",
            };
            Assert.CatchAsync<NotFoundException>(() =>
                ApiService.UpdateActivityAsync(IdOf<Project>.Random(), IdOf<Activity>.Random(), input, oc, CancellationToken.None));
        }
    }
}
