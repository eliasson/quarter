using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Commands;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using Quarter.State;
using Quarter.State.Forms;
using Quarter.State.ViewModels;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.State;

public class WhenDispatchingAddActivityActionTest : ActionHandlerTestCase
{
    private IdOf<Project> _projectId;
    private ApplicationState _state;

    [OneTimeSetUp]
    public async Task Setup()
    {
        var state = NewState();
        var formData = new ActivityFormData { Name = "Alpha", Description = "Activity A", Color = "#123" };
        _projectId = IdOf<Project>.Random();
        state.Modals.Push(ModalState.ParameterLess(typeof(FakeModal)));
        state.Projects.Add(new ProjectViewModel { Id = _projectId, Name =" Project A"});

        _state = await ActionHandler.HandleAsync(state, new AddActivityAction(_projectId, formData), CancellationToken.None);
    }

    [Test]
    public void ItShouldIssueCommand()
    {
        var expectedCommand = new AddActivityCommand(_projectId, "Alpha", "Activity A", Color.FromHexString("#123"));
        AssertIssuedCommandByUserId(expectedCommand, ActingUserId);
    }

    [Test]
    public void ItShouldPopTopMostModal()
        => Assert.That(_state.Modals, Is.Empty);
}