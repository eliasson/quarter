using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Commands;
using Quarter.State;
using Quarter.State.Forms;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.State;

[TestFixture]
public class WhenDispatchingAddProjectActionTest : ActionHandlerTestCase
{
    [OneTimeSetUp]
    public async Task Setup()
    {
        var state = NewState();
        var formData = new ProjectFormData { Name = "Alpha", Description = "Project A" };
        await ActionHandler.HandleAsync(state, new AddProjectAction(formData), CancellationToken.None);
    }

    [Test]
    public void ItShouldIssueCommand()
    {
        var expectedCommand = new AddProjectCommand("Alpha", "Project A");
        AssertIssuedCommandByUserId(expectedCommand, ActingUserId);
    }

    [Test]
    public void ItShouldPopTopMostModal()
    {
        var state = NewState();
        state.Modals.Push(ModalState.ParameterLess(typeof(FakeModal)));

        var formData = new ProjectFormData { Name = "Alpha", Description = "Project A" };
        ActionHandler.HandleAsync(state, new AddProjectAction(formData), CancellationToken.None);

        Assert.That(state.Modals, Is.Empty);
    }
}