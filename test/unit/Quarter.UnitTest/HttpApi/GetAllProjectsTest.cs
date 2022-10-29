using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.HttpApi;

[TestFixture]
public class GetAllProjectsTest
{
    [Ignore("Fake auth does not work")]
    public class WhenValid : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            var user = await SetupAuthorizedUserAsync("john.doe@example.com");
            await AddProjectAsync(user.Id, "Project Alpha");
            _response = await GetAsync("/api/project");
        }

        [Test]
        public void ItShouldReturnSuccessfulStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        [Test]
        public void ItShouldReturnJsonContentType()
            => Assert.That(_response?.ContentType(), Is.EqualTo(MediaTypeNames.Application.Json));
    }

    public class WhenUserIsNotAuthenticated : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            var user = await SetupUnauthenticatedUserAsync("john.doe@example.com");
            await AddProjectAsync(user.Id, "Project Alpha");
            _response = await GetAsync("/api/project");
        }

        [Test]
        public void ItShouldReturnUnauthorizedStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}