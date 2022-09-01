using System.Linq;
using System.Threading;
using NUnit.Framework;
using Quarter.Core.Commands;
using Quarter.Core.Models;
using Quarter.State;
using Quarter.State.ViewModels;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.State;

[TestFixture]
public class ConfirmRemoveProjectActionTest : ActionHandlerTestCase
{
    private IdOf<Project> _projectId;
    private ApplicationState _state;

    [OneTimeSetUp]
    public void Setup()
    {
        _state = NewState();
        _projectId = IdOf<Project>.Random();
        _state.Projects.Add(new ProjectViewModel
        {
            Id = _projectId,
            Name = "Alpha"
        });

        ActionHandler.HandleAsync(_state, new ConfirmRemoveProjectAction(_projectId), CancellationToken.None);
    }

    [Test]
    public void ItShouldIssueCommand()
    {
        var expectedCommand = new RemoveProjectCommand(_projectId);
        AssertIssuedCommandByUserId(expectedCommand, ActingUserId);
    }

    [Test]
    public void ItShouldNoLongerIncludeProjectInState()
    {
        var projectNames = _state.Projects.Select(p => p.Name);
        Assert.That(projectNames, Is.Empty);
    }

    [Test]
    public void ItShouldPopTopMostModal()
    {
        var state = NewState();
        state.Modals.Push(ModalState.ParameterLess(typeof(FakeModal)));
        ActionHandler.HandleAsync(state, new CloseModalAction(), CancellationToken.None);

        Assert.That(state.Modals, Is.Empty);
    }
}