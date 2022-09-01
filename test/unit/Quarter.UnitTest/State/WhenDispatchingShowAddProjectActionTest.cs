using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Pages.Application.Manage;
using Quarter.State;
using Quarter.State.Forms;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.State;

[TestFixture]
public class WhenDispatchingShowAddProjectActionTest : ActionHandlerTestCase
{
    private ApplicationState _state;

    [OneTimeSetUp]
    public async Task Setup()
    {
        var state = NewState();
        _state = await ActionHandler.HandleAsync(state, new ShowAddProjectAction(), CancellationToken.None);
    }

    [Test]
    public void ItShouldPushNewModal()
        => _state.AssertPushedNewModal(typeof(ProjectModal));

    [Test]
    public void ItShouldUseNewFormData()
    {
        var formData = _state.Modals.Select(m => m.Parameters[ApplicationState.FormData] as ProjectFormData).First();
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
        Assert.That(parameters[ApplicationState.ModalTitle], Is.EqualTo("Add project"));
    }
}