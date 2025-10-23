using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using Quarter.HttpApi.Resources;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.HttpApi;

[TestFixture]
public class UpdateTimesheetTest
{
    [TestFixture]
    public class WhenTimesheetDoesNotExist : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            var date = Date.Random();
            var user = await SetupAuthorizedUserAsync("john.doe@example.com");
            var project = await AddProjectAsync(user.Id, "Project Alpha");
            var activity = await AddActivityAsync(user.Id, project.Id, "Activity Alpha");

            var payload = new
            {
                date = date.IsoString(),
                timeSlots = new[]
                {
                    new
                    {
                        projectId = project.Id.AsString(),
                        activityId = activity.Id.AsString(),
                        offset = 4,
                        duration = 8,
                    }
                }
            };
            _response = await PutAsync($"/api/timesheets/{date.IsoString()}", payload);
        }

        [Test]
        public void ItShouldReturnSuccessfulStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        [Test]
        public void ItShouldReturnJsonContentType()
            => Assert.That(_response?.ContentType(), Is.EqualTo(MediaTypeNames.Application.Json));

        [Test]
        public async Task ItShouldReturnTimesheetPayload()
        {
            var payload = await _response.AsPayload<TimesheetResourceOutput>();
            Assert.That(payload?.totalMinutes, Is.EqualTo(8 * 15));
        }
    }

    [TestFixture]
    public class WhenTimesheetIsRegistered : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            var date = Date.Random();
            var user = await SetupAuthorizedUserAsync("john.doe@example.com");
            var project = await AddProjectAsync(user.Id, "Project Alpha");
            var activity = await AddActivityAsync(user.Id, project.Id, "Activity Alpha");
            _ = await AddTimesheetAsync(user.Id, date, new ActivityTimeSlot(project.Id, activity.Id, 0, 4));

            var payload = new
            {
                date = date.IsoString(),
                timeSlots = new[]
                {
                    new
                    {
                        projectId = project.Id.AsString(),
                        activityId = activity.Id.AsString(),
                        offset = 10,
                        duration = 8,
                    }
                }
            };
            _response = await PutAsync($"/api/timesheets/{date.IsoString()}", payload);
        }

        [Test]
        public void ItShouldReturnSuccessfulStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        [Test]
        public void ItShouldReturnJsonContentType()
            => Assert.That(_response?.ContentType(), Is.EqualTo(MediaTypeNames.Application.Json));

        [Test]
        public async Task ItShouldReturnTimesheetPayload()
        {
            var payload = await _response.AsPayload<TimesheetResourceOutput>();
            Assert.That(payload?.totalMinutes, Is.EqualTo(12 * 15));
        }
    }

    [TestFixture]
    public class WithMalformedDateParameter : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            _ = await SetupAuthorizedUserAsync("john.doe@example.com");
            var date = Date.Today().IsoString();

            _response = await PutAsync($"/api/timesheets/01-27-23", new object());
        }

        [Test]
        public void ItShouldReturnNotFound()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [TestFixture]
    public class WhenUserIsNotAuthenticated : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            _ = await SetupUnauthenticatedUserAsync("john.doe@example.com");
            var date = Date.Today().IsoString();
            var payload = new
            {
                date,
                timeSlots = new[]
                {
                    new
                    {
                        projectId = IdOf<Project>.Random().AsString(),
                        activityId = IdOf<Activity>.Random().AsString(),
                        offset = 10,
                        duration = 8,
                    }
                }
            };

            _response = await PutAsync($"/api/timesheets/{date}", payload);
        }

        [Test]
        public void ItShouldReturnUnauthorizedStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
