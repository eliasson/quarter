using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Quarter.Core.Commands;
using Quarter.Core.Models;
using Quarter.State;
using Quarter.State.ViewModels;

namespace Quarter.UnitTest.State;

[TestFixture]
public class ConfirmRemoveActivityActionTest : ActionHandlerTestCase
{
    private IdOf<Project> _projectId;
    private IdOf<Activity> _activityId;
    private ApplicationState _state;

    [OneTimeSetUp]
    public void Setup()
    {
        _state = NewState();
        _projectId = IdOf<Project>.Random();
        _activityId = IdOf<Activity>.Random();
        _state.Projects.Add(new ProjectViewModel
        {
            Id = _projectId,
            Name = "Alpha",
            Activities = new List<ActivityViewModel>
            {
                new ActivityViewModel { Id = _activityId, Name = "One" },
                new ActivityViewModel { Id = IdOf<Activity>.Random(), Name = "Two" },
            }
        });

        ActionHandler.HandleAsync(_state, new ConfirmRemoveActivityAction(_activityId), CancellationToken.None);
    }

    [Test]
    public void ItShouldIssueCommand()
    {
        var expectedCommand = new RemoveActivityCommand(_activityId);
        AssertIssuedCommandByUserId(expectedCommand, ActingUserId);
    }

    [Test]
    public void ItShouldNoLongerIncludeActivityInState()
    {
        var activityNames = _state.Projects.First().Activities?.Select(a => a.Name);
        Assert.That(activityNames, Is.EqualTo(new[] { "Two" }));
    }

    [Test]
    public void ItShouldPopTopMostModal()
        => Assert.That(_state.Modals, Is.Empty);
}
