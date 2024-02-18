using System.Threading;
using NUnit.Framework;
using Quarter.Core.Commands;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using Quarter.State;
using Quarter.State.Forms;

namespace Quarter.UnitTest.State;

[TestFixture]
public class AddUserActionWithAdminRoleTest : ActionHandlerTestCase
{
    [OneTimeSetUp]
    public void Setup()
    {
        var state = NewState();
        var formData = new UserFormData { Email = "janet.doe@example.com", IsAdmin = true };
        ActionHandler.HandleAsync(state, new AddUserAction(formData), CancellationToken.None);
    }

    [Test]
    public void ItShouldIssueCommand()
    {
        var expectedCommand = new AddUserCommand(new Email("janet.doe@example.com"), new[] { UserRole.Administrator });
        AssertIssuedCommand(expectedCommand);
    }
}
