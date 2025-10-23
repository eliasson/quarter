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
public class ShowEditProjectActionTest : ActionHandlerTestCase
{
    private Project _project;
    private ApplicationState _state;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _project = await AddProject(ActingUserId, "Project Alpha", "Notes");
        _state = await ActionHandler.HandleAsync(NewState(), new ShowEditProjectAction(_project.Id), CancellationToken.None);
    }

    [Test]
    public void ItShouldPushNewModal()
        => _state.AssertPushedNewModal(typeof(ProjectModal));


    [Test]
    public void ItShouldUseCorrectParameters()
    {
        var formData = _state.Modals.Select(m => m.Parameters[ApplicationState.FormData] as ProjectFormData).First();
        Assert.Multiple(() =>
        {
            Assert.That(formData?.Name, Is.EqualTo("Project Alpha"));
            Assert.That(formData?.Description, Is.EqualTo("Notes"));
        });
    }

    [Test]
    public void ItShouldUseCorrectTitle()
    {
        var parameters = _state.Modals.Select(m => m.Parameters).First();
        Assert.That(parameters[ApplicationState.ModalTitle], Is.EqualTo("Edit project"));
    }

    [Test]
    public void ItShouldUseProjectId()
    {
        var parameters = _state.Modals.Select(m => m.Parameters).First();
        Assert.That(parameters[nameof(ProjectModal.ProjectId)], Is.EqualTo(_project.Id));
    }
}
