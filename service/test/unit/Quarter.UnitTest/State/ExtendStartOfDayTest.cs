#nullable enable

using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.State;

namespace Quarter.UnitTest.State;

[TestFixture]
public class ExtendStartOfDayTest
{
    [TestFixture]
    public class WhenNotAtMidnight : ActionHandlerTestCase
    {
        private int _initialHour;
        private ApplicationState _state = null!;

        [OneTimeSetUp]
        public async Task Setup()
        {
            var state = NewState();
            _initialHour = state.StartHourOfDay;
            _state = await ActionHandler.HandleAsync(state, new ExtendStartOfDay(), CancellationToken.None);
        }

        [Test]
        public void ItShouldHaveMovedTheHourBackOneHour()
            => Assert.That(_state.StartHourOfDay, Is.EqualTo(_initialHour - 1));
    }

    [TestFixture]
    public class WhenAtMidnight : ActionHandlerTestCase
    {
        private ApplicationState _state = null!;

        [OneTimeSetUp]
        public async Task Setup()
        {
            var state = NewState();
            state.StartHourOfDay = 0;
            _state = await ActionHandler.HandleAsync(state, new ExtendStartOfDay(), CancellationToken.None);
        }

        [Test]
        public void ItShouldNotHaveChangedStartOfDay()
            => Assert.That(_state.StartHourOfDay, Is.EqualTo(0));
    }
}
