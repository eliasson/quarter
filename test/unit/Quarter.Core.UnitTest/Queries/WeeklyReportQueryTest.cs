using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Queries;
using Quarter.Core.Utils;

namespace Quarter.Core.UnitTest.Queries;

public abstract class WeeklyReportQueryTest
{
    public class WhenNoTimeIsRegistered : QueryTestBase
    {
        private static readonly Date Today = Date.Today();
        private WeeklyReportResult _result;

        [OneTimeSetUp]
        public async Task Setup()
            => _result =  await QueryHandler.ExecuteAsync(new WeeklyReportQuery(Today), OperationContext(), CancellationToken.None);

        [Test]
        public void ItShouldContainStartOfWeek()
            => Assert.That(_result.StartOfWeek, Is.EqualTo(Today.StartOfWeek()));

        [Test]
        public void ItShouldContainEndOfWeek()
            => Assert.That(_result.EndOfWeek, Is.EqualTo(Today.EndOfWeek()));

        [Test]
        public void ItShouldContainTotalMinutes()
            => Assert.That(_result.TotalMinutes, Is.EqualTo(0));

        [Test]
        public void ItShouldContainTotalHours()
            => Assert.That(_result.TotalAsHours(), Is.EqualTo("0.00"));
    }

    public class WhenTimeIsRegistered : QueryTestBase
    {

        private static readonly Date StartOfWeek = Date.Today().StartOfWeek();
        private static readonly Date EndOfWeek = StartOfWeek.EndOfWeek();
        private static readonly Date BeforeWeek = new Date(StartOfWeek.DateTime.AddDays(-1));
        private static readonly Date AfterWeek = new Date(EndOfWeek.DateTime.AddDays(1));

        private WeeklyReportResult _result;
        private readonly IdOf<Project> _projectIdOne = IdOf<Project>.Random();
        private readonly IdOf<Project> _projectIdTwo = IdOf<Project>.Random();
        private readonly IdOf<Activity> _activityIdOne = IdOf<Activity>.Random();
        private readonly IdOf<Activity> _activityIdTwo = IdOf<Activity>.Random();
        private readonly IdOf<Activity> _activityIdThree = IdOf<Activity>.Random();

        [OneTimeSetUp]
        public async Task Setup()
        {
            var beforeWeekTimesheet = Timesheet.CreateForDate(BeforeWeek);
            beforeWeekTimesheet.Register(new ActivityTimeSlot(_projectIdOne, _activityIdOne, 0, 20)); // Not included!

            var afterWeekTimesheet = Timesheet.CreateForDate(AfterWeek);
            afterWeekTimesheet.Register(new ActivityTimeSlot(_projectIdOne, _activityIdOne, 0, 20)); // Not included!

            var startOfWeekTimesheet = Timesheet.CreateForDate(StartOfWeek);
            startOfWeekTimesheet.Register(new ActivityTimeSlot(_projectIdOne, _activityIdOne, 0, 2));
            startOfWeekTimesheet.Register(new ActivityTimeSlot(_projectIdTwo, _activityIdTwo, 10, 4));
            startOfWeekTimesheet.Register(new ActivityTimeSlot(_projectIdTwo, _activityIdTwo, 20, 2));

            var endOfWeekTimesheet = Timesheet.CreateForDate(EndOfWeek);
            endOfWeekTimesheet.Register(new ActivityTimeSlot(_projectIdOne, _activityIdThree, 30, 8));

            await StoreTimesheet(ActingUser, beforeWeekTimesheet);
            await StoreTimesheet(ActingUser, startOfWeekTimesheet);
            await StoreTimesheet(ActingUser, endOfWeekTimesheet);
            await StoreTimesheet(ActingUser, afterWeekTimesheet);

            _result =  await QueryHandler.ExecuteAsync(new WeeklyReportQuery(Date.Today()), OperationContext(), CancellationToken.None);
        }

        [Test]
        public void ItShouldContainTotalMinutes()
            => Assert.That(_result?.TotalMinutes, Is.EqualTo(16 * 15));

        [Test]
        public void ItShouldContainTotalHours()
            => Assert.That(_result?.TotalAsHours(), Is.EqualTo("4.00"));

        [Test]
        public void ItShouldContainTotalUsageForTwoProjects()
        {
            var projectTotalUsage = _result.Usage.Values.Select(pu => (pu.ProjectId, pu.TotalMinutes));

            Assert.That(projectTotalUsage, Is.EquivalentTo(new []
            {
                ( _projectIdOne, 10 * 15 ),
                ( _projectIdTwo, 6 * 15 ),
            }));
        }

        [Test]
        public void ItShouldContainActivityTotalForEachProject()
        {
            var projectActivityTotalUsage = _result.Usage.Values.Select(pu =>
                (pu.ProjectId, pu.Usage.Values.Select(au => (au.ActivityId, au.TotalMinutes))));

            Assert.That(projectActivityTotalUsage, Is.EquivalentTo(new []
            {
                ( _projectIdOne, new []
                {
                    (_activityIdOne, 2 * 15),
                    (_activityIdThree, 8 * 15),
                }),
                ( _projectIdTwo, new []
                {
                    (_activityIdTwo, 6 * 15)
                }),
            }));
        }

        [Test]
        public void ItShouldContainUsagePerWeekday()
        {
            var result = _result.Usage.Values.SelectMany(pu => pu.Usage.Values.Select(au =>
                (pu.ProjectId, au.ActivityId, au.DurationPerWeekDay)));
            Assert.That(result, Is.EquivalentTo(new[]
            {
                ( _projectIdOne, _activityIdOne,   new [] { 2 * 15, 0, 0, 0, 0, 0,      0 } ),
                ( _projectIdOne, _activityIdThree, new [] {      0, 0, 0, 0, 0, 0, 8 * 15 } ),
                ( _projectIdTwo, _activityIdTwo,   new [] { 6 * 15, 0, 0, 0, 0, 0,      0 } ),
            }));
        }
    }
}