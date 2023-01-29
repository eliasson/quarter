using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using Quarter.HttpApi.Resources;

namespace Quarter.HttpApi.UnitTest.Resources;

[TestFixture]
public class TimesheetResourceOutputTest
{
    [TestFixture]
    public class WhenTimesheetIsEmpty : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            Timesheet = Timesheet.CreateForDate(DateInTest);
            Resource = TimesheetResourceOutput.From(Timesheet);
        }

        [Test]
        public void ItShouldMapDateAsIsoString()
            => Assert.That(Resource.date, Is.EqualTo(Timesheet.Date.IsoString()));

        [Test]
        public void ItShouldMapTotal()
            => Assert.That(Resource.totalMinutes, Is.EqualTo(0));
    }

    [TestFixture]
    public class WhenTimesheetContainsSlots : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            Timesheet = Timesheet.CreateForDate(DateInTest);
            Timesheet.Register(new ActivityTimeSlot(ProjectIdA, ActivityIdA, 4, 10));
            Timesheet.Register(new ActivityTimeSlot(ProjectIdB, ActivityIdB, 50, 20));
            Resource = TimesheetResourceOutput.From(Timesheet);
        }

        [Test]
        public void ItShouldMapTotal()
            => Assert.That(Resource.totalMinutes, Is.EqualTo((10 + 20) * 15));

        [Test]
        public void ItShouldMapTimeSlots()
        {
            Assert.That(Resource.timeSlots, Is.EqualTo(new []
            {
                new Quarter.HttpApi.Resources.TimeSlotOutput(ProjectIdA.AsString(), ActivityIdA.AsString(), 4, 10),
                new Quarter.HttpApi.Resources.TimeSlotOutput(ProjectIdB.AsString(), ActivityIdB.AsString(), 50, 20),
            }));
        }
    }

    public abstract class TestCase
    {
        protected Date DateInTest = Date.Random();
        protected Timesheet Timesheet = null!;
        protected TimesheetResourceOutput Resource = null!;
        protected readonly IdOf<Project> ProjectIdA = IdOf<Project>.Random();
        protected readonly IdOf<Project> ProjectIdB = IdOf<Project>.Random();
        protected readonly IdOf<Activity> ActivityIdA = IdOf<Activity>.Random();
        protected readonly IdOf<Activity> ActivityIdB = IdOf<Activity>.Random();
    }
}