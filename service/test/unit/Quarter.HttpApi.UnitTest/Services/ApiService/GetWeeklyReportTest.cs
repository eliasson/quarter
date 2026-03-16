using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using Quarter.HttpApi.Resources;

namespace Quarter.HttpApi.UnitTest.Services;

[TestFixture]
public class GetWeeklyReportTest
{
    [TestFixture]
    public class WhenNoTimeIsRegistered : TestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();
        private static readonly Date Today = Date.Today();
        private static readonly Date StartOfWeek = Today.StartOfWeek();
        private static readonly Date EndOfWeek = Today.EndOfWeek();

        private readonly IdOf<Project> _projectIdOne = IdOf<Project>.Random();
        private readonly IdOf<Project> _projectIdTwo = IdOf<Project>.Random();
        private readonly IdOf<Activity> _activityIdOne = IdOf<Activity>.Random();
        private readonly IdOf<Activity> _activityIdTwo = IdOf<Activity>.Random();
        private readonly IdOf<Activity> _activityIdThree = IdOf<Activity>.Random();

        private WeeklyReportResourceOutput _report = null!;

        [OneTimeSetUp]
        public async Task Setup()
        {
            var startOfWeekTimesheet = Timesheet.CreateForDate(StartOfWeek);
            startOfWeekTimesheet.Register(new ActivityTimeSlot(_projectIdOne, _activityIdOne, 0, 2));
            startOfWeekTimesheet.Register(new ActivityTimeSlot(_projectIdTwo, _activityIdTwo, 10, 4));
            startOfWeekTimesheet.Register(new ActivityTimeSlot(_projectIdTwo, _activityIdTwo, 20, 2));

            var endOfWeekTimesheet = Timesheet.CreateForDate(EndOfWeek);
            endOfWeekTimesheet.Register(new ActivityTimeSlot(_projectIdOne, _activityIdThree, 30, 8));

            await AddTimesheetAsync(_oc.UserId, startOfWeekTimesheet);
            await AddTimesheetAsync(_oc.UserId, endOfWeekTimesheet);

             _report = await ApiService.GetWeeklyReportAsync(Today, _oc, CancellationToken.None);
        }

        [Test]
        public void ItShouldIncludeStartOfWeekAsDateString()
            => Assert.That(_report.startOfWeek, Is.EqualTo(StartOfWeek.IsoString()));

        [Test]
        public void ItShouldIncludeEndOfWeekAsDateString()
            => Assert.That(_report.endOfWeek, Is.EqualTo(EndOfWeek.IsoString()));

        [Test]
        public void ItShouldIncludeTotalMinutes()
            => Assert.That(_report.totalMinutes, Is.EqualTo(16 * 15));

        [Test]
        public void ItShouldIncludeWeekdayTotals()
            => Assert.That(_report.weekdayTotals, Is.EqualTo(new[] { 8 * 15, 0, 0, 0, 0, 0, 8 * 15 }));
    }
}
