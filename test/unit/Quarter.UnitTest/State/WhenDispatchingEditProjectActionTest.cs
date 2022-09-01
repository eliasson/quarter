using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Commands;
using Quarter.Core.Models;
using Quarter.State;
using Quarter.State.Forms;
using Quarter.State.ViewModels;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.State;

[TestFixture]
public class WhenDispatchingEditProjectActionTest : ActionHandlerTestCase
{
    private ApplicationState _state;
    private IdOf<Project> _projectId;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _projectId = IdOf<Project>.Random();
        _state = NewState();
        _state.Projects = new List<ProjectViewModel> { new ProjectViewModel { Id = _projectId, Name = "Old", Description = null } };
        _state.Modals.Push(ModalState.ParameterLess(typeof(FakeModal)));

        var formData = new ProjectFormData { Name = "Alpha", Description = "Project A" };
        await ActionHandler.HandleAsync(_state, new EditProjectAction(_projectId, formData), CancellationToken.None);
    }

    [Test]
    public void ItShouldIssueCommand()
    {
        var expectedCommand = new EditProjectCommand(_projectId, "Alpha", "Project A");
        AssertIssuedCommandByUserId(expectedCommand, ActingUserId);
    }

    [Test]
    public void ItShouldUpdateTheProjectInApplicationState()
    {
        Assert.Multiple(() =>
        {
            Assert.That(_state.Projects[0].Name, Is.EqualTo("Alpha"));
            Assert.That(_state.Projects[0].Description, Is.EqualTo("Project A"));
        });
    }

    [Test]
    public void ItShouldPopTopMostModal()
        => Assert.That(_state.Modals, Is.Empty);
}