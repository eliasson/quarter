using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using Quarter.HttpApi.Resources;

namespace Quarter.HttpApi.UnitTest.Services;

[TestFixture]
public class UpdateTimesheetTest
{
    [TestFixture]
    public class WhenNoTimeIsRegistered : TestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();
        private readonly Date _today = Date.Today();
        private TimesheetResourceOutput _output = null!;
        private Project _project = null!;
        private Activity _activity = null!;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _project = await AddProject(_oc.UserId, "Project Alpha");
            _activity = await AddActivity(_oc.UserId, _project.Id, "Activity Alpha");
            var input = new TimesheetResourceInput
            {
                date = _today.ToString(),
                timeSlots = new TimeSlotInput[]
                {
                    new TimeSlotInput
                    {
                        projectId = _project.Id.AsString(),
                        activityId = _activity.Id.AsString(),
                        offset = 10,
                        duration = 2,
                    }
                }
            };
            _output = await ApiService.UpdateTimesheetAsync(input, _oc, CancellationToken.None);
        }

        [Test]
        public void ItShouldReturnOutputResourceForTimesheet()
            => Assert.That(_output?.date, Is.EqualTo(_today.IsoString()));

        [Test]
        public async Task ItShouldHaveUpdatedTheTimesheet()
        {
            var timesheet = await ReadTimesheetAsync(_oc.UserId, _today);
            var slots = timesheet.Slots().Select(s => (s.ProjectId, s.ActivityId, s.Offset, s.Duration));

            Assert.That(slots, Is.EqualTo(new[]
            {
                (_project.Id, _activity.Id, 10, 2),
            }));
        }
    }

    [TestFixture]
    public class WhenTimeIsRegistered : TestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();
        private readonly Date _today = Date.Today();
        private TimesheetResourceOutput _output = null!;
        private Project _project = null!;
        private Activity _activity = null!;
        private Timesheet _timesheet = null!;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _project = await AddProject(_oc.UserId, "Project Alpha");
            _activity = await AddActivity(_oc.UserId, _project.Id, "Activity Alpha");

            _timesheet = Timesheet.CreateForDate(_today);
            _timesheet.Register(new ActivityTimeSlot(_project.Id, _activity.Id, 0, 12));
            await AddTimesheetAsync(_oc.UserId, _timesheet);

            var input = new TimesheetResourceInput
            {
                date = _today.ToString(),
                timeSlots = new TimeSlotInput[]
                {
                    new TimeSlotInput
                    {
                        projectId = null,
                        activityId = null,
                        offset = 4,
                        duration = 4,
                    }
                }
            };
            _output = await ApiService.UpdateTimesheetAsync(input, _oc, CancellationToken.None);
        }

        [Test]
        public void ItShouldReturnOutputResourceForTimesheet()
            => Assert.That(_output?.date, Is.EqualTo(_today.IsoString()));

        [Test]
        public async Task ItShouldHaveUpdatedTheTimesheet()
        {
            var timesheet = await ReadTimesheetAsync(_oc.UserId, _today);
            var slots = timesheet.Slots().Select(s => (s.ProjectId, s.ActivityId, s.Offset, s.Duration));

            Assert.That(slots, Is.EqualTo(new[]
            {
                (_project.Id, _activity.Id, 0, 4),
                (_project.Id, _activity.Id, 8, 4),
            }));
        }
    }
}
