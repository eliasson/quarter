using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Quarter.Core.Commands;
using Quarter.Core.Models;
using Quarter.State;
using Quarter.State.ViewModels;

namespace Quarter.UnitTest.State;

public class WhenDispatchingConfirmRestoreActivityActionTest : ActionHandlerTestCase
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
                new ActivityViewModel { Id = _activityId, Name = "One", IsArchived = true },
                new ActivityViewModel { Id = IdOf<Activity>.Random(), Name = "Two" },
            }
        });

        ActionHandler.HandleAsync(_state, new ConfirmRestoreActivityAction(_activityId), CancellationToken.None);
    }

    [Test]
    public void ItShouldIssueCommand()
    {
        var expectedCommand = new RestoreActivityCommand(_activityId);
        AssertIssuedCommandByUserId(expectedCommand, ActingUserId);
    }

    [Test]
    public void ItShouldBeMarkedAsNotArchivedInState()
    {
        var activity = _state.Projects.First().Activities?.First(a => a.Id == _activityId);
        Assert.That(activity.IsArchived, Is.False);
    }

    [Test]
    public void ItShouldPopTopMostModal()
        => Assert.That(_state.Modals, Is.Empty);
}