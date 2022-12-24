using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using Quarter.HttpApi.Resources;

namespace Quarter.HttpApi.UnitTest.Services;

[TestFixture]
public class CreateActivityTest
{
    public class WhenInputIsValid : TestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();
        private ActivityResourceOutput? _output;
        private Project? _project;

        [OneTimeSetUp]
        public async Task Setup()
        {
            var input = new ActivityResourceInput
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
}