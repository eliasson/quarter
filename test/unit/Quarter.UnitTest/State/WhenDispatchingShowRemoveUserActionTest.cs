using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Components.Modals;
using Quarter.State;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.State;

public class WhenDispatchingShowRemoveUserActionTest : ActionHandlerTestCase
{
    private ApplicationState _state;

    [OneTimeSetUp]
    public async Task Setup()
        => _state = await ActionHandler.HandleAsync(NewState(), new ShowRemoveUserAction("jane.doe@example.com"), CancellationToken.None);

    [Test]
    public void ItShouldPushNewModal()
        => _state.AssertPushedNewModal(typeof(ConfirmModal));

    [Test]
    public void ItShouldUseCorrectParameters()
    {
        var parameters = _state.Modals.Select(m => m.Parameters).First();
        Assert.Multiple(() =>
        {
            Assert.That(parameters["Title"], Is.EqualTo("Remove user?"));
            Assert.That(parameters["Message"], Is.EqualTo("Are you sure you want to remove this user and all associated projects? This cannot be undone!"));
            Assert.That(parameters["ConfirmText"], Is.EqualTo("Remove"));
            Assert.That(parameters["IsDangerous"], Is.True);
        });
    }

    [Test]
    public void ItShouldIssueExpectedAction()
    {
        var parameters = _state.Modals.Select(m => m.Parameters).First();
        var action = parameters[nameof(ConfirmModal.OnConfirmAction)] as ConfirmRemoveUserAction;

        Assert.That(action?.UserId, Is.EqualTo("jane.doe@example.com"));
    }
}