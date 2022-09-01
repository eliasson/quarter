using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Utils;
using Quarter.State;

namespace Quarter.UnitTest.State;

[TestFixture]
public class LoadTimesheetActionTest
{
    public class WhenNoTimeIsRegistered : TestCase
    {
        private Date _dateInTest;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _dateInTest = Date.Random();
            State = await ActionHandler.HandleAsync(State, new LoadTimesheetAction(_dateInTest), CancellationToken.None);
        }

        [Test]
        public void ItShouldLoadNewTimesheet()
            => Assert.That(State.SelectedTimesheet?.Slots(), Is.Empty);

        [Test]
        public void ItShouldSetCurrentDate()
            => Assert.That(State.SelectedDate, Is.EqualTo(_dateInTest));
    }

    public class WhenTimeIsRegistered : TestCase
    {
        private Date _dateInTest;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _dateInTest = Date.Random();
            await AddTimesheet(ActingUserId, _dateInTest, 0, 4);
            State = await ActionHandler.HandleAsync(State, new LoadTimesheetAction(_dateInTest), CancellationToken.None);
        }

        [Test]
        public void ItShouldLoadNewTimesheet()
            => Assert.That(State.SelectedTimesheet?.TotalMinutes(), Is.EqualTo(4 * 15));

        [Test]
        public void ItShouldSetCurrentDate()
            => Assert.That(State.SelectedDate, Is.EqualTo(_dateInTest));

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