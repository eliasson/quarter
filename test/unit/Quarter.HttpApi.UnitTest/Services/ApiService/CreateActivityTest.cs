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
public class CreateActivityTest
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
            var input = new CreateActivityResourceInput
            {
                name = "Activity alpha",
                description = "Description alpha",
                color = "#aabbcc",
            };
            _project = await AddProject(_oc.UserId, "Project alpha");
            _output = await ApiService.CreateActivityAsync(_project.Id, input, _oc, CancellationToken.None);
        }

        [Test]
        public void ItShouldReturnOutputResourceForActivity()
            => Assert.That(_output?.name, Is.EqualTo("Activity alpha"));

        [Test]
        public async Task ItShouldHaveCreatedActivity()
        {
            var activities = await ReadActivitiesAsync(_oc.UserId, _project!.Id);
            var activityNames = activities.Select(p => p.Name);
            Assert.That(activityNames, Is.EqualTo(new [] { "Activity alpha" }));
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
            var input = new CreateActivityResourceInput
            {
                name = "Activity alpha",
                description = "Description alpha",
                color = "#aabbcc",
            };
            var project = await AddProject(userId, "Project alpha");

            Assert.CatchAsync<NotFoundException>(() => ApiService.CreateActivityAsync(project.Id, input, oc, CancellationToken.None));
        }
    }

    [TestFixture]
    public class WhenProjectIsMissing : TestCase
    {
        [Test]
        public void ItShouldThrowNotFoundException()
        {
            var oc = CreateOperationContext();
            var input = new CreateActivityResourceInput
            {
                name = "Activity alpha",
                description = "Description alpha",
                color = "#aabbcc",
            };
            Assert.CatchAsync<NotFoundException>(() =>
                ApiService.CreateActivityAsync(IdOf<Project>.Random(), input, oc, CancellationToken.None));
        }
    }
}