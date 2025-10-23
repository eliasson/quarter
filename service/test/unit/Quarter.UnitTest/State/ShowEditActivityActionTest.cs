using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using Quarter.Pages.Application.Manage;
using Quarter.State;
using Quarter.State.Forms;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.State;

[TestFixture]
public class ShowEditActivityActionTest : ActionHandlerTestCase
{
    private Activity _activity;
    private ApplicationState _state;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _activity = await AddActivity(ActingUserId, IdOf<Project>.Random(), "Activity One", "Notes", Color.FromHexString("#222"));
        _state = await ActionHandler.HandleAsync(NewState(), new ShowEditActivityAction(_activity.ProjectId, _activity.Id), CancellationToken.None);
    }

    [Test]
    public void ItShouldPushNewModal()
        => _state.AssertPushedNewModal(typeof(ActivityModal));


    [Test]
    public void ItShouldUseCorrectParameters()
    {
        var formData = _state.Modals.Select(m => m.Parameters[ApplicationState.FormData] as ActivityFormData).First();
        Assert.Multiple(() =>
        {
            Assert.That(formData?.Name, Is.EqualTo("Activity One"));
            Assert.That(formData?.Description, Is.EqualTo("Notes"));
            Assert.That(formData?.Color, Is.EqualTo("#222222"));
        });
    }

    [Test]
    public void ItShouldUseCorrectTitle()
    {
        var parameters = _state.Modals.Select(m => m.Parameters).First();
        Assert.That(parameters[ApplicationState.ModalTitle], Is.EqualTo("Edit activity"));
    }

    [Test]
    public void ItShouldUseProjectId()
    {
        var parameters = _state.Modals.Select(m => m.Parameters).First();
        Assert.That(parameters[nameof(ProjectModal.ProjectId)], Is.EqualTo(_activity.ProjectId));
    }

    [Test]
    public void ItShouldUseActivityId()
    {
        var parameters = _state.Modals.Select(m => m.Parameters).First();
        Assert.That(parameters[nameof(ActivityModal.ActivityId)], Is.EqualTo(_activity.Id));
    }
}
