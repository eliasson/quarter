using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Queries;
using Quarter.Core.Utils;

namespace Quarter.Core.UnitTest.Queries;

[TestFixture]
public class MonthlyReportQueryTest
{
    public class WhenNoTimeIsRegistered : QueryTestBase
    {
        private static readonly Date Today = Date.Today();
        private MonthlyReportResult _result;

        [OneTimeSetUp]
        public async Task Setup()
            => _result = await QueryHandler.ExecuteAsync(new MonthlyReportQuery(Today), OperationContext(),
                CancellationToken.None);

        [Test]
        public void ItShouldContainStartOfMonth()
            => Assert.That(_result.StartOfMonth, Is.EqualTo(Today.StartOfMonth()));

        [Test]
        public void ItShouldContainEndOfMonth()
            => Assert.That(_result.EndOfMonth, Is.EqualTo(Today.EndOfMonth()));

        [Test]
        public void ItShouldContainTotalMinutes()
            => Assert.That(_result.TotalMinutes, Is.EqualTo(0));

        [Test]
        public void ItShouldContainTotalHours()
            => Assert.That(_result.TotalMinutes.MinutesAsHours(), Is.EqualTo("0.00"));
    }

    public class WhenTimeIsRegistered : QueryTestBase
    {
        private static readonly Date StartOfMonth = Date.Today().StartOfMonth();
        private static readonly Date EndOfMonth = StartOfMonth.EndOfMonth();
        private static readonly Date BeforeMonth = new Date(StartOfMonth.DateTime.AddDays(-1));
        private static readonly Date AfterMonth = new Date(EndOfMonth.DateTime.AddDays(1));
        private readonly IdOf<Project> _projectIdOne = IdOf<Project>.Random();
        private readonly IdOf<Project> _projectIdTwo = IdOf<Project>.Random();
        private readonly IdOf<Activity> _activityIdOne = IdOf<Activity>.Random();
        private readonly IdOf<Activity> _activityIdTwo = IdOf<Activity>.Random();
        private readonly IdOf<Activity> _activityIdThree = IdOf<Activity>.Random();

        private MonthlyReportResult _result;

        [OneTimeSetUp]
        public async Task Setup()
        {
            var beforeMonthTimesheet = Timesheet.CreateForDate(BeforeMonth);
            beforeMonthTimesheet.Register(new ActivityTimeSlot(_projectIdOne, _activityIdOne, 0, 20)); // Not included!

            var afterMonthTimesheet = Timesheet.CreateForDate(AfterMonth);
            afterMonthTimesheet.Register(new ActivityTimeSlot(_projectIdOne, _activityIdOne, 0, 20)); // Not included!

            var startOfMonthTimesheet = Timesheet.CreateForDate(StartOfMonth);
            startOfMonthTimesheet.Register(new ActivityTimeSlot(_projectIdOne, _activityIdOne, 0, 2));
            startOfMonthTimesheet.Register(new ActivityTimeSlot(_projectIdTwo, _activityIdTwo, 10, 4));
            startOfMonthTimesheet.Register(new ActivityTimeSlot(_projectIdTwo, _activityIdTwo, 20, 2));

            var endOfMonthTimesheet = Timesheet.CreateForDate(EndOfMonth);
            endOfMonthTimesheet.Register(new ActivityTimeSlot(_projectIdOne, _activityIdThree, 30, 8));

            await StoreTimesheet(ActingUser, beforeMonthTimesheet);
            await StoreTimesheet(ActingUser, startOfMonthTimesheet);
            await StoreTimesheet(ActingUser, endOfMonthTimesheet);
            await StoreTimesheet(ActingUser, afterMonthTimesheet);

            _result = await QueryHandler.ExecuteAsync(new MonthlyReportQuery(Date.Today()), OperationContext(),
                CancellationToken.None);
        }

        [Ignore("WIP"), Test]
        public void ItShouldContainTotalMinutes()
            => Assert.That(_result?.TotalMinutes, Is.EqualTo(16 * 15));
    }
}