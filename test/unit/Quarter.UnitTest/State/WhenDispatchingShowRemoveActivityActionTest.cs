using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Components.Modals;
using Quarter.Core.Models;
using Quarter.State;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.State;

public class WhenDispatchingShowRemoveActivityActionTest : ActionHandlerTestCase
{
    private ApplicationState _state;
    private IdOf<Activity> _activityId;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _activityId = IdOf<Activity>.Random();
        _state = await ActionHandler.HandleAsync(NewState(), new ShowRemoveActivityAction(_activityId), CancellationToken.None);
    }

    [Test]
    public void ItShouldPushNewModal()
        => _state.AssertPushedNewModal(typeof(ConfirmRemoveModal));

    [Test]
    public void ItShouldUseCorrectParameters()
    {
        var parameters = _state.Modals.Select(m => m.Parameters).First();
        Assert.Multiple(() =>
        {
            Assert.That(parameters["Title"], Is.EqualTo("Remove activity?"));
            Assert.That(parameters["Message"], Is.EqualTo("Are you sure you want to remove this activity and all registered time? This cannot be undone!"));
        });
    }

    [Test]
    public void ItShouldIssueExpectedAction()
    {
        var parameters = _state.Modals.Select(m => m.Parameters).First();
        var action = parameters[nameof(ConfirmRemoveModal.OnConfirmAction)] as ConfirmRemoveActivityAction;

        Assert.That(action?.ActivityId, Is.EqualTo(_activityId));
    }
}