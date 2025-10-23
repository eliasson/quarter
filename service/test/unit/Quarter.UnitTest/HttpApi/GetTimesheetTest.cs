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
public class GetTimesheetTest
{
    [TestFixture]
    public class WhenTimeIsRegistered : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            var user = await SetupAuthorizedUserAsync("john.doe@example.com");
            var date = Date.Random();
            _ = await AddTimesheetAsync(user.Id, date,
                new ActivityTimeSlot(IdOf<Project>.Random(), IdOf<Activity>.Random(), 10, 4));

            _response = await GetAsync($"/api/timesheets/{date.IsoString()}");
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
            Assert.That(payload?.totalMinutes, Is.EqualTo(4 * 15));
        }
    }

    [TestFixture]
    public class WhenNoTimeIsRegistered : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            _ = await SetupAuthorizedUserAsync("john.doe@example.com");
            var date = Date.Today().IsoString();

            _response = await GetAsync($"/api/timesheets/{date}");
        }

        [Test]
        public void ItShouldReturnSuccessfulStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        [Test]
        public void ItShouldReturnJsonContentType()
            => Assert.That(_response?.ContentType(), Is.EqualTo(MediaTypeNames.Application.Json));
    }

    [TestFixture]
    public class WithInvalidDateParameter : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            _ = await SetupAuthorizedUserAsync("john.doe@example.com");
            var date = Date.Today().IsoString();

            _response = await GetAsync($"/api/timesheets/01-27-23");
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

            _response = await GetAsync($"/api/timesheets/{date}");
        }

        [Test]
        public void ItShouldReturnUnauthorizedStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
