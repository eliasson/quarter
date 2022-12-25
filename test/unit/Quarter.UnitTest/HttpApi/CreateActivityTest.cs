using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.HttpApi.Resources;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.HttpApi;

[TestFixture]
public class CreateActivityTest
{
    public class WhenPayloadIsValid : HttpTestCase
    {
        private HttpResponseMessage _response;
        private Project _project;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            var user = await SetupAuthorizedUserAsync("john.doe@example.com");
            _project = await AddProjectAsync(user.Id, "Project Alpha");

            var payload = new
            {
                projectId = _project.Id.AsString(),
                name = "Test name",
                description = "Test description",
                color = "#CCAABB"
            };

            _response = await PostAsync($"/api/projects/{_project.Id.AsString()}/activities", payload);
        }

        [Test]
        public void ItShouldReturnCreatedStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        [Test]
        public async Task ItShouldReturnTheCreatedPayload()
        {
            var payload = await _response.AsPayload<ActivityResourceOutput>();
            Assert.That(payload?.name, Is.EqualTo("Test name"));
        }

        [Test]
        public async Task ItShouldReturnLocationToEntity()
        {
            var payload = await _response.AsPayload<ActivityResourceOutput>();
            Assert.That(_response?.Headers.Location?.ToString(), Is.EqualTo($"/api/projects/{_project.Id.AsString()}/activities/{payload?.id}"));
        }
    }

    public class WhenInvalidPayload : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            var user = await SetupAuthorizedUserAsync("john.doe@example.com");
            var project = await AddProjectAsync(user.Id, "Project Alpha");
            var payload = new
            {
                projectId = project.Id.AsString(),
                name = "Test name",
                // Missing description
                color = "#CCAABB"
            };

            _response = await PostAsync($"/api/projects/{project.Id.AsString()}/activities", payload);
        }

        [Test]
        public void ItShouldReturnBadRequestStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    public class WhenUserIsNotAuthenticated : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            var user = await SetupUnauthenticatedUserAsync("john.doe@example.com");
            var project = await AddProjectAsync(user.Id, "Project Alpha");
            var payload = new
            {
                projectId = project.Id.AsString(),
                name = "Test name",
                description = "Test description",
                color = "#CCAABB"
            };

            _response = await PostAsync($"/api/projects/{project.Id.AsString()}/activities", payload);
        }

        [Test]
        public void ItShouldReturnUnauthorizedStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}