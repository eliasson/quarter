using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.HttpApi;

[TestFixture]
public class CreateUserTest
{
    [TestFixture]
    public class WhenUserIsAdmin : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            _ = await SetupAuthorizedUserAsync("john.doe@example.com", u => u.AssignRole(UserRole.Administrator));

            var payload = new { email = "jane@example.com" };
            _response = await PostAsync("/api/users", payload);
        }

        [Test]
        public void ItShouldReturnSuccessfulStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        [Test]
        public void ItShouldReturnJsonContentType()
            => Assert.That(_response?.ContentType(), Is.EqualTo(MediaTypeNames.Application.Json));
    }

    [TestFixture]
    public class WhenUserNotAdmin : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            _ = await SetupAuthorizedUserAsync("john.doe@example.com");
            var payload = new { email = "jane@example.com" };
            _response = await PostAsync("/api/users", payload);
        }

        [Test]
        public void ItShouldReturnForbiddenStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [TestFixture]
    public class WhenUserIsNotAuthenticated : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            _ = await SetupUnauthenticatedUserAsync("john.doe@example.com");
            var payload = new { email = "jane@example.com" };
            _response = await PostAsync("/api/users", payload);
        }

        [Test]
        public void ItShouldReturnUnauthorizedStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
