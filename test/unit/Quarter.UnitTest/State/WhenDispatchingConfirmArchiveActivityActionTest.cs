using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Quarter.Core.Commands;
using Quarter.Core.Models;
using Quarter.State;
using Quarter.State.ViewModels;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.State;

public class WhenDispatchingConfirmArchiveActivityActionTest : ActionHandlerTestCase
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
        _state.Modals.Push(ModalState.ParameterLess(typeof(FakeModal)));

        ActionHandler.HandleAsync(_state, new ConfirmArchiveActivityAction(_activityId), CancellationToken.None);
    }

    [Test]
    public void ItShouldIssueCommand()
    {
        var expectedCommand = new ArchiveActivityCommand(_activityId);
        AssertIssuedCommandByUserId(expectedCommand, ActingUserId);
    }

    [Test]
    public void ItShouldBeMarkedAsArchivedInState()
    {
        var activity = _state.Projects.First().Activities?.First(a => a.Id == _activityId);
        Assert.That(activity.IsArchived, Is.True);
    }

    [Test]
    public void ItShouldPopTopMostModal()
        => Assert.That(_state.Modals, Is.Empty);
}