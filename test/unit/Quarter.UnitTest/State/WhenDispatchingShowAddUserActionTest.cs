using System.Linq;
using System.Threading;
using NUnit.Framework;
using Quarter.Pages.Admin.Users;
using Quarter.State;
using Quarter.State.Forms;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.State;

[TestFixture]
public class WhenDispatchingShowAddUserActionTest : ActionHandlerTestCase
{
    [Test]
    public void ItShouldPushNewModal()
    {
        var state = NewState();
        ActionHandler.HandleAsync(state, new ShowAddUserAction(), CancellationToken.None);

        state.AssertPushedNewModal(typeof(UserModal));
    }

    [Test]
    public void ItShouldUseNewFormData()
    {
        var state = NewState();
        ActionHandler.HandleAsync(state, new ShowAddUserAction(), CancellationToken.None);

        var formData = state.Modals.Select(m => m.Parameters[ApplicationState.FormData] as UserFormData).First();
        Assert.Multiple(() =>
        {
            Assert.That(formData?.Email, Is.Empty);
            Assert.That(formData?.IsAdmin, Is.False);
        });
    }
}