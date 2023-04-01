using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.HttpApi;

[TestFixture]
public class DeleteActivityTest
{
    [TestFixture]
    public class WhenActivityExist : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            var user = await SetupAuthorizedUserAsync("john.doe@example.com");
            var project = await AddProjectAsync(user.Id, "Project Alpha");
            var activity = await AddActivityAsync(user.Id, project.Id, "Activity Alpha");
            _response = await DeleteAsync($"/api/projects/{project.Id.AsString()}/activities/{activity.Id.AsString()}");
        }

        [Test]
        public void ItShouldReturnNoContentStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [TestFixture]
    public class WhenActivityDoesNotExist : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            var user = await SetupAuthorizedUserAsync("john.doe@example.com");
            var project = await AddProjectAsync(user.Id, "Project Alpha");

            _response = await DeleteAsync($"/api/projects/{project.Id.AsString()}/activities/{IdOf<Activity>.Random()}");
        }

        [Test]
        public void ItShouldReturnNoContentStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
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
            var activity = await AddActivityAsync(user.Id, project.Id, "Activity Alpha");
            _response = await DeleteAsync($"/api/projects/{project.Id.AsString()}/activities/{activity.Id.AsString()}");
        }

        [Test]
        public void ItShouldReturnUnauthorizedStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}