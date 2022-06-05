using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Components.Modals;
using Quarter.Core.Models;
using Quarter.State;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.State;

public class WhenDispatchingShowRestoreActivityActionTest : ActionHandlerTestCase
{
    private ApplicationState _state;
    private IdOf<Activity> _activityId;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _activityId = IdOf<Activity>.Random();
        _state = await ActionHandler.HandleAsync(NewState(), new ShowRestoreActivityAction(_activityId), CancellationToken.None);
    }

    [Test]
    public void ItShouldPushNewModal()
        => _state.AssertPushedNewModal(typeof(ConfirmModal));

    [Test]
    public void ItShouldUseCorrectParameters()
    {
        var parameters = _state.Modals.Select(m => m.Parameters).First();
        Assert.Multiple(() =>
        {
            Assert.That(parameters["Title"], Is.EqualTo("Restore activity?"));
            Assert.That(parameters["Message"], Is.EqualTo("If you archive this activity you will be able to use it to register time again. All previously registered time will still be available. The activity can later be archived again."));
            Assert.That(parameters["ConfirmText"], Is.EqualTo("Restore"));
        });
    }

    [Test]
    public void ItShouldIssueExpectedAction()
    {
        var parameters = _state.Modals.Select(m => m.Parameters).First();
        var action = parameters[nameof(ConfirmModal.OnConfirmAction)] as ConfirmRestoreActivityAction;

        Assert.That(action?.ActivityId, Is.EqualTo(_activityId));
    }
}