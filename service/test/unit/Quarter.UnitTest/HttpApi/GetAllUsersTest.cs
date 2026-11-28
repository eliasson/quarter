using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.HttpApi.Resources;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.HttpApi;

[TestFixture]
public class GetAllUsersTest
{
    [TestFixture]
    public class WhenUserIsAdmin : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            _ = await SetupAuthorizedUserAsync("john.doe@example.com", u => u.AssignRole(UserRole.Administrator));
            await AddUserAsync("lotta@example.com");
            await AddUserAsync("sven@example.com");
            _response = await GetAsync("/api/users");
        }

        [Test]
        public void ItShouldReturnSuccessfulStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        [Test]
        public void ItShouldReturnJsonContentType()
            => Assert.That(_response?.ContentType(), Is.EqualTo(MediaTypeNames.Application.Json));

        [Test]
        public async Task ItShouldReturnRolesForEachUser()
        {
            var payload = await _response.AsPayload<UserListItemResourceOutput[]>();
            var roles = payload?.ToDictionary(u => u.email, u => u.roles);

            Assert.That(roles, Is.EquivalentTo(new Dictionary<string, IReadOnlyList<string>>
            {
                ["john.doe@example.com"] = ["administrator"],
                ["lotta@example.com"] = [],
                ["sven@example.com"] = [],
            }));
        }

        [Test]
        public async Task ItShouldReturnLastLoginForActiveUserOnly()
        {
            // This also tests that the login tracking is set up in using the middleware.
            var payload = await _response.AsPayload<UserListItemResourceOutput[]>();
            var hasLastLogin = payload?.Select(u => (u.email, u.lastLogin is not null));

            Assert.That(hasLastLogin, Is.EquivalentTo([
                ("john.doe@example.com", true),
                ("lotta@example.com", false),
                ("sven@example.com", false),
            ]));
        }
    }

    [TestFixture]
    public class WhenUserNotAdmin : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            _ = await SetupAuthorizedUserAsync("john.doe@example.com");
            _response = await GetAsync("/api/users");
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
            _response = await GetAsync("/api/users");
        }

        [Test]
        public void ItShouldReturnUnauthorizedStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
