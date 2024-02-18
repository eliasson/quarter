using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Queries;
using Quarter.Core.Utils;

namespace Quarter.Core.UnitTest.Queries;

[TestFixture]
public class TimesheetSummaryQueryTest
{
    public class WhenConstructed : QueryTestBase
    {
        [Test]
        public void ItShouldSetupStartAndEndDatesForWeekOfDate()
        {
            var someDate = new Date(DateTime.Parse("2021-11-02T00:00:00Z")); // Tuesday
            var expectedStartDate = new Date(DateTime.Parse("2021-11-01T00:00:00Z"));
            var expectedEndDate = new Date(DateTime.Parse("2021-11-07T00:00:00Z"));
            var query = TimesheetSummaryQuery.ForWeek(someDate);

            Assert.That(query, Is.EqualTo(new TimesheetSummaryQuery(expectedStartDate, expectedEndDate)));
        }
    }

    public class WhenNoTimeIsRegistered : QueryTestBase
    {
        private TimesheetSummaryQueryResult _vm;

        private static readonly Date EndDate = Date.Today();
        private static readonly Date StartDate = new Date(EndDate.DateTime.AddDays(-2));

        [OneTimeSetUp]
        public async Task Setup()
        {
            _vm = await QueryHandler.ExecuteAsync(new TimesheetSummaryQuery(StartDate, EndDate), OperationContext(), CancellationToken.None);
        }

        [Test]
        public void ItShouldContainTotalMinutes()
            => Assert.That(_vm?.TotalMinutes, Is.EqualTo(0));

        [Test]
        public void ItShouldContainEmptyTimesheetsForAllDays()
        {
            var dayOne = new Date(StartDate.DateTime).IsoString();
            var dayTwo = new Date(StartDate.DateTime.AddDays(1)).IsoString();
            var dayThree = new Date(StartDate.DateTime.AddDays(2)).IsoString();

            var result = _vm?.Timesheets.Select(t => (t.Date.IsoString(), t.TotalMinutes()));

            Assert.That(result, Is.EqualTo(new[]
            {
                (dayOne, 0),
                (dayTwo, 0),
                (dayThree, 0),
            }));
        }
    }

    public class WhenTimeIsRegistered : QueryTestBase
    {
        private static readonly Date Yesterday = new Date(Date.Today().DateTime.AddDays(-1));
        private static readonly Date Today = Date.Today();
        private static readonly Date Tomorrow = new Date(Date.Today().DateTime.AddDays(1));
        private TimesheetSummaryQueryResult _vm;

        [OneTimeSetUp]
        public async Task Setup()
        {
            var yesterdayTimesheet = Timesheet.CreateForDate(Yesterday);
            yesterdayTimesheet.Register(new ActivityTimeSlot(IdOf<Project>.Random(), IdOf<Activity>.Random(), 0, 2));

            var todayTimesheet = Timesheet.CreateForDate(Today);
            todayTimesheet.Register(new ActivityTimeSlot(IdOf<Project>.Random(), IdOf<Activity>.Random(), 0, 4));

            var tomorrowTimesheet = Timesheet.CreateForDate(Tomorrow);
            tomorrowTimesheet.Register(new ActivityTimeSlot(IdOf<Project>.Random(), IdOf<Activity>.Random(), 0, 8));

            await StoreTimesheet(ActingUser, yesterdayTimesheet);
            await StoreTimesheet(ActingUser, todayTimesheet);
            await StoreTimesheet(ActingUser, tomorrowTimesheet);

            _vm = await QueryHandler.ExecuteAsync(new TimesheetSummaryQuery(Today, Tomorrow), OperationContext(),
                CancellationToken.None);
        }

        [Test]
        public void ItShouldContainTotalMinutes()
            => Assert.That(_vm?.TotalMinutes, Is.EqualTo(12 * 15));

        [Test]
        public void ItShouldContainEmptyTimesheetsForAllDays()
        {
            var dayOne = Today.IsoString();
            var dayTwo = Tomorrow.IsoString();
            var result = _vm?.Timesheets.Select(t => (t.Date.IsoString(), t.TotalMinutes()));

            Assert.That(result, Is.EqualTo(new[]
            {
                (dayOne, 4 * 15),
                (dayTwo, 8 * 15),
            }));
        }
    }
}
