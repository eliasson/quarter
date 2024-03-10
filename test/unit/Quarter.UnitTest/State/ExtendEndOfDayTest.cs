#nullable enable

using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.State;

namespace Quarter.UnitTest.State;

[TestFixture]
public class ExtendEndOfDayTest
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
            _initialHour = state.EndHourOfDay;
            _state = await ActionHandler.HandleAsync(state, new ExtendEndOfDay(), CancellationToken.None);
        }

        [Test]
        public void ItShouldHaveMovedTheHourForwardOneHour()
            => Assert.That(_state.EndHourOfDay, Is.EqualTo(_initialHour + 1));
    }

    [TestFixture]
    public class WhenAtMidnight : ActionHandlerTestCase
    {
        private ApplicationState _state = null!;

        [OneTimeSetUp]
        public async Task Setup()
        {
            var state = NewState();
            state.EndHourOfDay = 23;
            _state = await ActionHandler.HandleAsync(state, new ExtendEndOfDay(), CancellationToken.None);
        }

        [Test]
        public void ItShouldNotHaveChangedEndOfDay()
            => Assert.That(_state.EndHourOfDay, Is.EqualTo(23));
    }
}
