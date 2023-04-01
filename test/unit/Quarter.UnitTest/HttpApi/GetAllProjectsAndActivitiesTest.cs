using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.HttpApi.Resources;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.HttpApi;

[TestFixture]
public class GetAllProjectsAndActivitiesTest
{
    public class WhenValid : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            var user = await SetupAuthorizedUserAsync("john.doe@example.com");
            var projectAlpha = await AddProjectAsync(user.Id, "Project Alpha");
            var projectBravo = await AddProjectAsync(user.Id, "Project Bravo");
            await AddActivityAsync(user.Id, projectAlpha.Id, "Activity Alpha");
            await AddActivityAsync(user.Id, projectBravo.Id, "Activity Bravo");

            _response = await GetAsync($"/api/projects/activities");
        }

        [Test]
        public void ItShouldReturnSuccessfulStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        [Test]
        public void ItShouldReturnJsonContentType()
            => Assert.That(_response?.ContentType(), Is.EqualTo(MediaTypeNames.Application.Json));

        [Test]
        public async Task ItShouldReturnAllProjects()
        {
            var resource = await _response.AsPayload<ProjectAndActivitiesResourceOutput>();
            var projects = resource?.projects.Select(p => p.name);
            Assert.That(projects, Is.EquivalentTo(new [] { "Project Alpha", "Project Bravo" }));
        }

        [Test]
        public async Task ItShouldReturnAllActivities()
        {
            var resource = await _response.AsPayload<ProjectAndActivitiesResourceOutput>();
            var activities = resource?.activities.Select(a => a.name);
            Assert.That(activities, Is.EquivalentTo(new [] { "Activity Alpha", "Activity Bravo" }));
        }
    }

    public class WhenUserIsNotAuthenticated : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            var user = await SetupUnauthenticatedUserAsync("john.doe@example.com");
            var project = await AddProjectAsync(user.Id, "Project Alpha");
            await AddActivityAsync(user.Id, project.Id, "Activity Alpha");

            _response = await GetAsync($"/api/projects/activities");
        }

        [Test]
        public void ItShouldReturnUnauthorizedStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}