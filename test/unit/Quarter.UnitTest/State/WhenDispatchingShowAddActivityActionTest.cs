using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Pages.Application.Manage;
using Quarter.State;
using Quarter.State.Forms;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.State;

[TestFixture]
public class WhenDispatchingShowAddActivityActionTest : ActionHandlerTestCase
{
    private ApplicationState _state;
    private IdOf<Project> _projectId;

    [OneTimeSetUp]
    public async Task Setup()
    {
        var state = NewState();
        _projectId = IdOf<Project>.Random();
        _state = await ActionHandler.HandleAsync(state, new ShowAddActivityAction(_projectId), CancellationToken.None);
    }

    [Test]
    public void ItShouldPushNewModal()
        => _state.AssertPushedNewModal(typeof(ActivityModal));

    [Test]
    public void ItShouldUseNewFormData()
    {
        var formData = _state.Modals.Select(m => m.Parameters[ApplicationState.FormData] as ActivityFormData).First();
        Assert.Multiple(() =>
        {
            Assert.That(formData?.Name, Is.EqualTo(""));
            Assert.That(formData?.Description, Is.EqualTo(""));
        });
    }

    [Test]
    public void ItShouldUseCorrectTitle()
    {
        var parameters = _state.Modals.Select(m => m.Parameters).First();
        Assert.That(parameters[ApplicationState.ModalTitle], Is.EqualTo("Add activity"));
    }

    [Test]
    public void ItShouldSetProjectId()
    {
        var parameters = _state.Modals.Select(m => m.Parameters).First();
        Assert.That(parameters["ProjectId"], Is.EqualTo(_projectId));
    }
}