using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.HttpApi;

[TestFixture]
public class DeleteProjectTest
{
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