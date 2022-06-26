using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Components.Modals;
using Quarter.Core.Models;
using Quarter.State;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.State;

public class ShowRestoreProjectActionTest : ActionHandlerTestCase
{
    private ApplicationState _state;
    private IdOf<Project> _projectId;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _state = NewState();
        _projectId = IdOf<Project>.Random();
        _state.Modals.Push(ModalState.ParameterLess(typeof(FakeModal)));
        _state = await ActionHandler.HandleAsync(NewState(), new ShowRestoreProjectAction(_projectId), CancellationToken.None);
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
            Assert.That(parameters["Title"], Is.EqualTo("Restore project?"));
            Assert.That(parameters["Message"], Is.EqualTo("If you restore this project you will be able to use it to register time again. All previously registered time will still be available. The project can later be archived again."));
            Assert.That(parameters["ConfirmText"], Is.EqualTo("Restore"));
        });
    }

    [Test]
    public void ItShouldIssueExpectedAction()
    {
        var parameters = _state.Modals.Select(m => m.Parameters).First();
        var action = parameters[nameof(ConfirmModal.OnConfirmAction)] as ConfirmRestoreProjectAction;

        Assert.That(action?.ProjectId, Is.EqualTo(_projectId));
    }
}