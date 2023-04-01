using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.HttpApi.Resources;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.HttpApi;

[TestFixture]
public class CreateProjectTest
{
    [TestFixture]
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
            _response = await PostAsync("/api/projects/", payload);
        }

        [Test]
        public void ItShouldReturnCreatedStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        [Test]
        public async Task ItShouldReturnTheCreatedPayload()
        {
            var payload = await _response.AsPayload<ProjectResourceOutput>();
            Assert.That(payload?.name, Is.EqualTo("Test name"));
        }

        [Test]
        public async Task ItShouldReturnLocationToEntity()
        {
            var payload = await _response.AsPayload<ProjectResourceOutput>();
            Assert.That(_response?.Headers.Location?.ToString(), Is.EqualTo($"/api/projects/{payload?.id}"));
        }
    }

    [TestFixture]
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
            _response = await PostAsync("/api/projects/", payload);
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
            await SetupUnauthenticatedUserAsync("john.doe@example.com");
            var payload = new
            {
                name = "Test name",
                description = "Test description",
            };
            _response = await PostAsync("/api/projects/", payload);
        }

        [Test]
        public void ItShouldReturnUnauthorizedStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}