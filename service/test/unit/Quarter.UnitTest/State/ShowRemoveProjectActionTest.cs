using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Components.Modals;
using Quarter.Core.Models;
using Quarter.State;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.State;

[TestFixture]
public class ShowRemoveProjectActionTest : ActionHandlerTestCase
{
    private ApplicationState _state;
    private IdOf<Project> _projectId;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _projectId = IdOf<Project>.Random();
        _state = await ActionHandler.HandleAsync(NewState(), new ShowRemoveProjectAction(_projectId), CancellationToken.None);
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
            Assert.That(parameters["Title"], Is.EqualTo("Remove project?"));
            Assert.That(parameters["Message"], Is.EqualTo("Are you sure you want to remove this project and all associated activities? This cannot be undone!"));
            Assert.That(parameters["ConfirmText"], Is.EqualTo("Remove"));
            Assert.That(parameters["IsDangerous"], Is.True);
        });
    }

    [Test]
    public void ItShouldIssueExpectedAction()
    {
        var parameters = _state.Modals.Select(m => m.Parameters).First();
        var action = parameters[nameof(ConfirmModal.OnConfirmAction)] as ConfirmRemoveProjectAction;

        Assert.That(action?.ProjectId, Is.EqualTo(_projectId));
    }
}
