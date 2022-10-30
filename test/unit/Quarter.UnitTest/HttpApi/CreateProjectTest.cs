using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.HttpApi;

[TestFixture]
public class CreateProjectTest
{
    public class WhenPayloadIsValid : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            await SetupAuthorizedUserAsync("john.doe@example.com");
            var payload = new
            {
                name = "Test name",
                description = "Test description",
            };
            _response = await PostAsync("/api/project/", payload);
        }

        [Test]
        public void ItShouldReturnCreatedStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    public class WhenInvalidPayload : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            await SetupAuthorizedUserAsync("john.doe@example.com");
            var payload = new
            {
                description = "Missing name"
            };
            _response = await PostAsync("/api/project/", payload);
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
            _response = await DeleteAsync($"/api/project/{project.Id.AsString()}");
        }

        [Test]
        public void ItShouldReturnUnauthorizedStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}