using System;
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
public class GetWeeklyReportTest
{
    [TestFixture]
    public class WhenTimeIsRegistered : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            var user = await SetupAuthorizedUserAsync("john.doe@example.com");
            var date = new Date(new DateTime(2026, 03, 18));
            _ = await AddTimesheetAsync(user.Id, date,
                new ActivityTimeSlot(IdOf<Project>.Random(), IdOf<Activity>.Random(), 10, 4));

            _response = await GetAsync($"/api/reports/week/{date.IsoString()}");
        }

        [Test]
        public void ItShouldReturnSuccessfulStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        [Test]
        public void ItShouldReturnJsonContentType()
            => Assert.That(_response?.ContentType(), Is.EqualTo(MediaTypeNames.Application.Json));

        [Test]
        public async Task ItShouldReturnReportPayload()
        {
            var payload = await _response.AsPayload<WeeklyReportResourceOutput>();
            var date = new Date(new DateTime(2026, 03, 16));

            Assert.That(payload?.startOfWeek, Is.EqualTo(date.IsoString()));
        }
    }

    [TestFixture(0, 1)]
    [TestFixture(13, 1)]
    [TestFixture(1, 0)]
    [TestFixture(1, 31)]
    [TestFixture(2, 30)]
    public class WithInvalidDateParameter(int month, int day) : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            _ = await SetupAuthorizedUserAsync("john.doe@example.com");
            _response = await GetAsync($"/api/reports/week/2026-{month}-{day}");
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
            _response = await GetAsync("/api/reports/week/2026-03-18");
        }

        [Test]
        public void ItShouldReturnUnauthorizedStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
