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
public class GetTimesheetsForMonthTest
{
    [TestFixture]
    public class WhenTimeIsRegistered : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            var user = await SetupAuthorizedUserAsync("john.doe@example.com");
            var date = new Date(new DateTime(2026, 02, 07));
            _ = await AddTimesheetAsync(user.Id, date,
                new ActivityTimeSlot(IdOf<Project>.Random(), IdOf<Activity>.Random(), 10, 4));

            _response = await GetAsync($"/api/timesheets/{date.DateTime.Year}/{date.DateTime.Month}");
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
            var payload = await _response.AsPayload<TimesheetsResourceOutput>();
            Assert.That(payload?.timesheets.Count, Is.EqualTo(28));
        }
    }

    [TestFixture(0)]
    [TestFixture(13)]
    public class WithInvalidDateParameter(int month) : HttpTestCase
    {
        private HttpResponseMessage _response;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            _ = await SetupAuthorizedUserAsync("john.doe@example.com");
            _response = await GetAsync($"/api/timesheets/2026/{month}");
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
            _response = await GetAsync("/api/timesheets/2026/02");
        }

        [Test]
        public void ItShouldReturnUnauthorizedStatus()
            => Assert.That(_response?.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
