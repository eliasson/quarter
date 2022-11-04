using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.HttpApi.UnitTest.Services;

[TestFixture]
public class ActivitiesForProjectTest
{
    public class WhenNoActivitiesExistsForProject : TestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();
        private Project? _project;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _project = await AddProject(_oc.UserId, "Project alpha");
        }

        [Test]
        public async Task ItShouldReturnAnEmptyResultForActivities()
        {
            var activities = await ApiService.ActivitiesForProject(_project!.Id, _oc, CancellationToken.None).ToListAsync();
            Assert.That(activities, Is.Empty);
        }
    }

    public class WhenActivitiesExist : TestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();
        private Project? _project;
        private Activity? _activity;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _project = await AddProject(_oc.UserId, "Project alpha");
            _activity = await AddActivity(_oc.UserId, _project.Id, "Activity alpha");
        }

        [Test]
        public async Task ItShouldContainActivity()
        {
            var activityNames = await ApiService.ActivitiesForProject(_project!.Id, _oc, CancellationToken.None)
                .Select(p => p.name)
                .ToListAsync();
            Assert.That(activityNames, Does.Contain("Activity alpha"));
        }
    }
}