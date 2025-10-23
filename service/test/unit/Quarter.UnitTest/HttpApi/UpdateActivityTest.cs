using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.HttpApi.Resources;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.HttpApi;

[TestFixture]
public class UpdateActivityTest
{
    [TestFixture]
    public class WhenPayloadIsValid : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            var user = await SetupAuthorizedUserAsync("john.doe@example.com");
            var project = await AddProjectAsync(user.Id, "Project Alpha");
            var activity = await AddActivityAsync(user.Id, project.Id, "Activity Alpha");
            var payload = new
            {
                name = "Activity Alpha - updated"
            };
            _response = await PatchAsync($"/api/projects/{project.Id.AsString()}/activities/{activity.Id.AsString()}", payload);
        }

        [Test]
        public void ItShouldReturnOkStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        [Test]
        public async Task ItShouldReturnTheUpdatedPayload()
        {
            var payload = await _response.AsPayload<ActivityResourceOutput>();
            Assert.That(payload?.name, Is.EqualTo("Activity Alpha - updated"));
        }
    }

    [TestFixture]
    public class WhenActivityDoesNotExist : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            var user = await SetupAuthorizedUserAsync("john.doe@example.com");
            var project = await AddProjectAsync(user.Id, "Project Alpha");
            var payload = new
            {
                name = "Irrelevant"
            };

            _response = await PatchAsync($"/api/projects/{project.Id.AsString()}/activities/{IdOf<Activity>.Random()}", payload);
        }

        [Test]
        public void ItShouldReturnNotFoundStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [TestFixture]
    public class WhenUserIsNotAuthenticated : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            var user = await SetupUnauthenticatedUserAsync("john.doe@example.com");
            var project = await AddProjectAsync(user.Id, "Project Alpha");
            var activity = await AddActivityAsync(user.Id, project.Id, "Activity Alpha");
            var payload = new
            {
                name = "Irrelevant"
            };
            _response = await PatchAsync($"/api/projects/{project.Id.AsString()}/activities/{activity.Id.AsString()}", payload);
        }

        [Test]
        public void ItShouldReturnUnauthorizedStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
