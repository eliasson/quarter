#nullable enable

using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.HttpApi;

[TestFixture]
public class GetSelfTest
{
    [TestFixture]
    public class WhenUserIsAuthenticated : HttpTestCase
    {
        private HttpResponseMessage _response = null!;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            _ = await SetupAuthorizedUserAsync("john.doe@example.com");
            _response = await GetAsync("/api/users/self");
        }

        [Test]
        public void ItShouldReturnSuccessfulStatus()
            => Assert.That(_response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        [Test]
        public void ItShouldReturnJsonContentType()
            => Assert.That(_response.ContentType(), Is.EqualTo(MediaTypeNames.Application.Json));
    }

    [TestFixture]
    public class WhenUserIsNotAuthenticated : HttpTestCase
    {
        private HttpResponseMessage _response = null!;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            _ = await SetupUnauthenticatedUserAsync("john.doe@example.com");
            _response = await GetAsync("/api/users/self");
        }

        [Test]
        public void ItShouldReturnUnauthorizedStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
