using System.Linq;
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
    public class WhenTimeIsRegistered : ReportTestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();

        private WeeklyReportResourceOutput _report = null!;

        [OneTimeSetUp]
        public async Task Setup()
        {
            var startOfWeekTimesheet = Timesheet.CreateForDate(StartOfWeek);
            startOfWeekTimesheet.Register(new ActivityTimeSlot(ProjectIdOne, ActivityIdOne, 0, 2));
            startOfWeekTimesheet.Register(new ActivityTimeSlot(ProjectIdTwo, ActivityIdTwo, 10, 4));
            startOfWeekTimesheet.Register(new ActivityTimeSlot(ProjectIdTwo, ActivityIdTwo, 20, 2));

            var endOfWeekTimesheet = Timesheet.CreateForDate(EndOfWeek);
            endOfWeekTimesheet.Register(new ActivityTimeSlot(ProjectIdOne, ActivityIdThree, 30, 8));

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

        [Test]
        public void ItShouldIncludeProjectUsage()
        {
            var projectsTotal = _report.Usage.Select(pu => (pu.projectId, pu.totalMinutes)).ToList();

            Assert.That(projectsTotal, Is.EquivalentTo(new[]
            {
                (ProjectIdOne.AsString(), 10 * 15),
                (ProjectIdTwo.AsString(), 6 * 15),
            }));
        }


        [Test]
        public void ItShouldIncludeActivityUsage()
        {
            var result = _report.Usage.FirstOrDefault()?
                .activityUsage
                .Select(pu => (pu.activityId, pu.totalMinutes)).ToList();

            Assert.That(result, Is.EquivalentTo(new[]
            {
                (ActivityIdOne.AsString(), 2 * 15),
                (ActivityIdThree.AsString(), 8 * 15),
            }));
        }

        [Test]
        public void ItShouldIncludeActivityWeekdayTotals()
        {
            var activityUsage = _report.Usage.FirstOrDefault()?
                .activityUsage
                .Select(pu => (pu.activityId, pu.weekdayTotals)).FirstOrDefault();

            Assert.That(activityUsage, Is.EqualTo(
                (ActivityIdOne.AsString(), new[] { 2 * 15, 0, 0, 0, 0, 0, 0 })
            ));
        }
    }

    [TestFixture]
    public class WhenNoTimeIsRegistered : ReportTestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();

        private WeeklyReportResourceOutput _report = null!;

        [OneTimeSetUp]
        public async Task Setup()
        {
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
            => Assert.That(_report.totalMinutes, Is.Zero);

        [Test]
        public void ItShouldIncludeWeekdayTotals()
            => Assert.That(_report.weekdayTotals, Is.EqualTo(new[] { 0, 0, 0, 0, 0, 0, 0 }));

        [Test]
        public void ItShouldIncludeEmptyProjectUsage()
        {
            Assert.That(_report.Usage, Is.Empty);
        }
    }

    public abstract class ReportTestCase : TestCase
    {
        protected static readonly Date Today = Date.Today();
        protected static readonly Date StartOfWeek = Today.StartOfWeek();
        protected static readonly Date EndOfWeek = Today.EndOfWeek();

        protected readonly IdOf<Project> ProjectIdOne = IdOf<Project>.Random();
        protected readonly IdOf<Project> ProjectIdTwo = IdOf<Project>.Random();
        protected readonly IdOf<Activity> ActivityIdOne = IdOf<Activity>.Random();
        protected readonly IdOf<Activity> ActivityIdTwo = IdOf<Activity>.Random();
        protected readonly IdOf<Activity> ActivityIdThree = IdOf<Activity>.Random();
    }
}
