using System.Threading;
using NUnit.Framework;
using Quarter.Core.Commands;
using Quarter.Core.Models;
using Quarter.State;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.State;

[TestFixture]
public class ConfirmRemoveUserActionTest : ActionHandlerTestCase
{
    private IdOf<User> _userId;

    [OneTimeSetUp]
    public void Setup()
    {
        var state = NewState();
        _userId = IdOf<User>.Random();
        ActionHandler.HandleAsync(state, new ConfirmRemoveUserAction(_userId.AsString()), CancellationToken.None);
    }

    [Test]
    public void ItShouldIssueCommand()
    {
        var expectedCommand = new RemoveUserCommand(_userId);
        AssertIssuedCommandByUserId(expectedCommand, ActingUserId);
    }

    [Test]
    public void ItShouldPopTopMostModal()
    {
        var state = NewState();
        state.Modals.Push(ModalState.ParameterLess(typeof(FakeModal)));
        ActionHandler.HandleAsync(state, new CloseModalAction(), CancellationToken.None);

        Assert.That(state.Modals, Is.Empty);
    }
}
