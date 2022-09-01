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
public class WhenDispatchingEditActivityActionTest : ActionHandlerTestCase
{
    private ApplicationState _state;
    private IdOf<Project> _projectId;
    private IdOf<Activity> _activityId;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _projectId = IdOf<Project>.Random();
        _activityId = IdOf<Activity>.Random();
        var activities = new List<ActivityViewModel> { new() { Id = _activityId, Name = "Old" } };

        _state = NewState();
        _state.Projects = new List<ProjectViewModel> { new () { Id = _projectId, Activities = activities }};
        _state.Modals.Push(ModalState.ParameterLess(typeof(FakeModal)));

        var formData = new ActivityFormData { Name = "Alpha", Description = "Activity A", Color = "#112233" };
        await ActionHandler.HandleAsync(_state, new EditActivityAction(_projectId, _activityId, formData), CancellationToken.None);
    }

    [Test]
    public void ItShouldIssueCommand()
    {
        var expectedCommand = new EditActivityCommand(_activityId, "Alpha", "Activity A", Quarter.Core.Utils.Color.FromHexString("#112233"));
        AssertIssuedCommandByUserId(expectedCommand, ActingUserId);
    }

    [Test]
    public void ItShouldUpdateTheActivityInApplicationState()
    {
        var activity = _state.Projects[0].Activities[0];
        Assert.Multiple(() =>
        {
            Assert.That(activity.Name, Is.EqualTo("Alpha"));
            Assert.That(activity.Description, Is.EqualTo("Activity A"));
            Assert.That(activity.Color, Is.EqualTo("#112233"));
        });
    }

    [Test]
    public void ItShouldPopTopMostModal()
        => Assert.That(_state.Modals, Is.Empty);
}