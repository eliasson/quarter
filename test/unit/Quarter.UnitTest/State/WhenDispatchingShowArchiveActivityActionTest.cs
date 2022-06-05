using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Components.Modals;
using Quarter.Core.Models;
using Quarter.State;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.State;

public class WhenDispatchingShowArchiveActivityActionTest : ActionHandlerTestCase
{
    private ApplicationState _state;
    private IdOf<Activity> _activityId;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _activityId = IdOf<Activity>.Random();
        _state = await ActionHandler.HandleAsync(NewState(), new ShowArchiveActivityAction(_activityId), CancellationToken.None);
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
            Assert.That(parameters["Title"], Is.EqualTo("Archive activity?"));
            Assert.That(parameters["Message"], Is.EqualTo("If you archive this activity it can no longer be used to register time. All registered time will still be available though. This activity can be restored at a later time."));
        });
    }

    [Test]
    public void ItShouldIssueExpectedAction()
    {
        var parameters = _state.Modals.Select(m => m.Parameters).First();
        var action = parameters[nameof(ConfirmModal.OnConfirmAction)] as ConfirmArchiveActivityAction;

        Assert.That(action?.ActivityId, Is.EqualTo(_activityId));
    }
}