using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.HttpApi;

[TestFixture]
public class DeleteProjectTest
{
    public class WhenProjectExist : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            var user = await SetupAuthorizedUserAsync("john.doe@example.com");
            var project = await AddProjectAsync(user.Id, "Project Alpha");
            _response = await DeleteAsync($"/api/projects/{project.Id.AsString()}");
        }

        [Test]
        public void ItShouldReturnNoContentStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    public class WhenProjectDoesNotExist : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            await SetupAuthorizedUserAsync("john.doe@example.com");
            _response = await DeleteAsync($"/api/projects/{IdOf<Project>.Random().AsString()}");
        }

        [Test]
        public void ItShouldReturnNoContentStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    public class WhenUserIsNotAuthenticated : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            var user = await SetupUnauthenticatedUserAsync("john.doe@example.com");
            var project = await AddProjectAsync(user.Id, "Project Alpha");
            _response = await DeleteAsync($"/api/projects/{project.Id.AsString()}");
        }

        [Test]
        public void ItShouldReturnUnauthorizedStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}