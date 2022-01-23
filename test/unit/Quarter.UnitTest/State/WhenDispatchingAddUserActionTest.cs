using System;
using System.Threading;
using NUnit.Framework;
using Quarter.Core.Commands;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using Quarter.State;
using Quarter.State.Forms;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.State;

public class WhenDispatchingAddUserActionTest : ActionHandlerTestCase
{
    [OneTimeSetUp]
    public void Setup()
    {
        var state = NewState();
        var formData = new UserFormData { Email = "jane.doe@example.com", IsAdmin = false };
        ActionHandler.HandleAsync(state, new AddUserAction(formData), CancellationToken.None);
    }

    [Test]
    public void ItShouldIssueCommand()
    {
        var expectedCommand = new AddUserCommand(new Email("jane.doe@example.com"), Array.Empty<UserRole>());
        AssertIssuedCommandByUserId(expectedCommand, ActingUserId);
    }

    [Test]
    public void ItShouldPopTopMostModal()
    {
        var state = NewState();
        state.Modals.Push(ModalState.ParameterLess(typeof(FakeModal)));

        var formData = new UserFormData { Email = "jane.doe@example.com", IsAdmin = false };
        ActionHandler.HandleAsync(state, new AddUserAction(formData), CancellationToken.None);

        Assert.That(state.Modals, Is.Empty);
    }
}