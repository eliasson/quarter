using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using Quarter.State;

namespace Quarter.UnitTest.State;

[TestFixture]
public class EraseTimeActionTest
{
    public class WhenNoTimeIsRegistered : TestCase
    {
        private Date _dateInTest;

        [Test]
        public void ItShouldNotThrow()
        {
            _dateInTest = Date.Random();
            Assert.DoesNotThrowAsync(() => ActionHandler.HandleAsync(State, new EraseTimeAction(_dateInTest, new EraseTimeSlot(0, 2)), CancellationToken.None));
        }
    }

    public class WhenTimeIsRegistered : TestCase
    {
        private Date _dateInTest;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _dateInTest = Date.Random();
            await AddTimesheet(ActingUserId, _dateInTest, 0, 4);
            State = await ActionHandler.HandleAsync(State, new EraseTimeAction(_dateInTest, new EraseTimeSlot(0, 2)), CancellationToken.None);
        }

        [Test]
        public void ItShouldUpdateTimesheet()
            => Assert.That(State.SelectedTimesheet?.TotalMinutes(), Is.EqualTo(2 * 15));
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