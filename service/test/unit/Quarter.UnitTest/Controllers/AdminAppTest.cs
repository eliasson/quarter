using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Controllers;

[TestFixture]
public class AdminAppTest
{
    [TestFixture]
    public class WhenUserNotAdmin : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            _ = await SetupAuthorizedUserAsync("john.doe@example.com");
            _response = await GetAsync("/admin/users");
        }

        [Test]
        public void ItShouldReturnForbiddenStatus()
            => Assert.That(_response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [TestFixture]
    public class WhenUserIsNotAuthenticated : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            _ = await SetupUnauthenticatedUserAsync("john.doe@example.com");
            _response = await GetAsync("/admin/users");
        }

        [Test]
        public void ItShouldRedirectToLogin()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
                Assert.That(_response.Headers.Location?.ToString(), Is.EqualTo("/ui/login.html"));
            });
        }
    }

    [TestFixture]
    public class WhenUserIsAuthenticated : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            _ = await SetupAuthorizedUserAsync("john.doe@example.com", configure: u => u.AssignRole(UserRole.Administrator));
            _response = await GetAsync("/admin/users");
        }

        [Test]
        public void ItShouldReturnOkStatus()
        {
            Assert.That(_response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
    }
}
