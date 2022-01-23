using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using Quarter.State;

namespace Quarter.UnitTest.State;

public abstract class WhenDispatchingRegisterTimeActionTest
{
    public class WhenNoTimeIsRegistered : TestCase
    {
        private Date _dateInTest;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _dateInTest = Date.Random();
            var timeSlot = new ActivityTimeSlot(IdOf<Project>.Random(), IdOf<Activity>.Random(), 0, 2);
            State = await ActionHandler.HandleAsync(State, new RegisterTimeAction(_dateInTest, timeSlot), CancellationToken.None);
        }

        [Test]
        public void ItShouldUpdateTimesheet()
            => Assert.That(State.SelectedTimesheet?.TotalMinutes(), Is.EqualTo(2 * 15));
    }

    public class WhenTimeIsRegistered : TestCase
    {
        private Date _dateInTest;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _dateInTest = Date.Random();
            await AddTimesheet(ActingUserId, _dateInTest, 0, 4);
            var timeSlot = new ActivityTimeSlot(IdOf<Project>.Random(), IdOf<Activity>.Random(), 10, 2);
            State = await ActionHandler.HandleAsync(State, new RegisterTimeAction(_dateInTest, timeSlot), CancellationToken.None);
        }

        [Test]
        public void ItShouldUpdateTimesheet()
            => Assert.That(State.SelectedTimesheet?.TotalMinutes(), Is.EqualTo(6 * 15));
    }

    public abstract class TestCase : ActionHandlerTestCase
    {
        protected ApplicationState State;

        [OneTimeSetUp]
        public void SetupTestCase()
        {
            State = NewState();
        }
    }
}