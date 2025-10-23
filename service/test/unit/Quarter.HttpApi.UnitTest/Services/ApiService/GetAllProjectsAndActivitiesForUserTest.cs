using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Utils;
using Quarter.HttpApi.Resources;

namespace Quarter.HttpApi.UnitTest.Services;

[TestFixture]
public class GetAllProjectsAndActivitiesForUserTest
{
    [TestFixture]
    public class WhenNoProjectExists : TestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();
        private ProjectAndActivitiesResourceOutput _result = null!;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _result = await ApiService.GetAllProjectsAndActivitiesForUserAsync(_oc, CancellationToken.None);
        }

        [Test]
        public void ItShouldReturnAnEmptyResultForProjects()
            => Assert.That(_result.projects, Is.Empty);

        [Test]
        public void ItShouldReturnAnEmptyResultForActivities()
            => Assert.That(_result.activities, Is.Empty);
    }

    [TestFixture]
    public class WhenProjectsAndActivitiesExist : TestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();
        private ProjectAndActivitiesResourceOutput _result = null!;

        [OneTimeSetUp]
        public async Task Setup()
        {
            var projectAlpha = await AddProject(_oc.UserId, "Project Alpha");
            var projectBravo = await AddProject(_oc.UserId, "Project Bravo");
            _ = await AddActivity(_oc.UserId, projectAlpha.Id, "Activity Alpha");
            _ = await AddActivity(_oc.UserId, projectBravo.Id, "Activity Bravo");

            _result = await ApiService.GetAllProjectsAndActivitiesForUserAsync(_oc, CancellationToken.None);
        }

        [Test]
        public void ItShouldReturnAllProjects()
            => Assert.That(_result.projects.Select(p => p.name), Is.EquivalentTo(new[] { "Project Alpha", "Project Bravo" }));

        [Test]
        public void ItShouldReturnAllActivities()
            => Assert.That(_result.activities.Select(a => a.name), Is.EquivalentTo(new[] { "Activity Alpha", "Activity Bravo" }));
    }
}
