using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Commands;
using Quarter.Core.Models;
using Quarter.State;
using Quarter.State.ViewModels;

namespace Quarter.UnitTest.State;

[TestFixture]
public class ConfirmRestoreProjectActionTest : ActionHandlerTestCase
{
    private ApplicationState _state;
    private IdOf<Project> _projectId;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _state = NewState();
        _projectId = IdOf<Project>.Random();
        _state.Projects = new List<ProjectViewModel> { new() { Id = _projectId, Name = "Alpha", IsArchived = true } };
        _state = await ActionHandler.HandleAsync(_state, new ConfirmRestoreProjectAction(_projectId), CancellationToken.None);
    }

    [Test]
    public void ItShouldIssueCommand()
    {
        var expectedCommand = new RestoreProjectCommand(_projectId);
        AssertIssuedCommandByUserId(expectedCommand, ActingUserId);
    }

    [Test]
    public void ItShouldBeMarkedAsArchivedInState()
    {
        var project = _state.Projects.First(p => p.Id == _projectId);
        Assert.That(project.IsArchived, Is.False);
    }

    [Test]
    public void ItShouldPopTopMostModal()
        => Assert.That(_state.Modals, Is.Empty);
}
