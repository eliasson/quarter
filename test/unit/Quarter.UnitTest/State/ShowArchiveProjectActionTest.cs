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
public class ShowArchiveProjectActionTest : ActionHandlerTestCase
{
    private ApplicationState _state;
    private IdOf<Project> _projectId;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _state = NewState();
        _projectId = IdOf<Project>.Random();
        _state.Modals.Push(ModalState.ParameterLess(typeof(FakeModal)));
        _state = await ActionHandler.HandleAsync(NewState(), new ShowArchiveProjectAction(_projectId), CancellationToken.None);
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
            Assert.That(parameters["Title"], Is.EqualTo("Archive project?"));
            Assert.That(parameters["Message"], Is.EqualTo("If you archive this project it can no longer be used to register time. All registered time will still be available though. This project can be restored at a later time."));
            Assert.That(parameters["ConfirmText"], Is.EqualTo("Archive"));
        });
    }

    [Test]
    public void ItShouldIssueExpectedAction()
    {
        var parameters = _state.Modals.Select(m => m.Parameters).First();
        var action = parameters[nameof(ConfirmModal.OnConfirmAction)] as ConfirmArchiveProjectAction;

        Assert.That(action?.ProjectId, Is.EqualTo(_projectId));
    }
}