using System;
using System.Threading;
using NUnit.Framework;
using Quarter.State;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.State;

[TestFixture]
public class WhenDispatchingCloseModalActionTest : ActionHandlerTestCase
{
    [Test]
    public void ItShouldPopTopMostModal()
    {
        var state = NewState();
        state.Modals.Push(ModalState.ParameterLess(typeof(FakeModal)));
        ActionHandler.HandleAsync(state, new CloseModalAction(), CancellationToken.None);

        Assert.That(state.Modals, Is.Empty);
    }

    [Test]
    public void ItShouldThrowIfNoModalsExists()
        => Assert.ThrowsAsync<InvalidOperationException>(() => ActionHandler.HandleAsync(NewState(), new CloseModalAction(), CancellationToken.None));
}