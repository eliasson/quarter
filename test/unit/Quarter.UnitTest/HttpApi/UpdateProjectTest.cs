using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.HttpApi.Resources;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.HttpApi;

[TestFixture]
public class UpdateProjectTest
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
            var payload = new
            {
                name = "Project Alpha Updated",
                description = "Updated description",
            };
            _response = await PatchAsync($"/api/projects/{project.Id.AsString()}", payload);
        }

        [Test]
        public void ItShouldReturnOkStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        [Test]
        public async Task ItShouldReturnTheUpdatedPayload()
        {
            var payload = await _response.AsPayload<ProjectResourceOutput>();
            Assert.That(payload?.name, Is.EqualTo("Project Alpha Updated"));
        }
    }

    [TestFixture]
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
                description = "Missing name"
            };
            _response = await PatchAsync($"/api/projects/{project.Id.AsString()}", payload);
        }

        [Test]
        public void ItShouldReturnBadRequestStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
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
            _response = await PatchAsync($"/api/projects/{project.Id.AsString()}", project);
        }

        [Test]
        public void ItShouldReturnUnauthorizedStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}