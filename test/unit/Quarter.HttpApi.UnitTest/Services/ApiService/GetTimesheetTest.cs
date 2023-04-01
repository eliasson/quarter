using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using Quarter.HttpApi.Resources;

namespace Quarter.HttpApi.UnitTest.Services;

[TestFixture]
public class GetTimesheetTest
{
    [TestFixture]
    public class WhenNoTimeIsRegistered : TestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();
        private readonly Date _today = Date.Today();
        private TimesheetResourceOutput _timesheet = null!;

        [OneTimeSetUp]
        public async Task Setup()
            => _timesheet = await ApiService.GetTimesheetAsync(_today, _oc, CancellationToken.None);

        [Test]
        public void ItShouldReturnResultForTheGivenDate()
            => Assert.That(_timesheet.date, Is.EqualTo(_today.IsoString()));

        [Test]
        public void ItShouldReturnAnEmptyListOfSlots()
            => Assert.That(_timesheet.timeSlots, Is.Empty);
    }

    [TestFixture]
    public class WhenTimeIsRegistered : TestCase
    {
        private readonly IdOf<Project> _projectIdA = IdOf<Project>.Random();
        private readonly IdOf<Activity> _activityIdA = IdOf<Activity>.Random();
        private readonly OperationContext _oc = CreateOperationContext();
        private readonly Date _today = Date.Today();
        private TimesheetResourceOutput _timesheet = null!;

        [OneTimeSetUp]
        public async Task Setup()
        {
            var timeSheet = Timesheet.CreateForDate(_today);
            timeSheet.Register(new ActivityTimeSlot(_projectIdA, _activityIdA, 4, 44));
            await AddTimesheetAsync(_oc.UserId, timeSheet);

            _timesheet = await ApiService.GetTimesheetAsync(_today, _oc, CancellationToken.None);
        }

        [Test]
        public void ItShouldReturnTheTimesheetWithRegisteredTime()
            => Assert.That(_timesheet.totalMinutes, Is.EqualTo(44 * 15));
    }
}