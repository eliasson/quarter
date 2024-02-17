using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.State;
using Quarter.State.ViewModels;

namespace Quarter.UnitTest.State;

[TestFixture]
public class SelectActivityActionTest : ActionHandlerTestCase
{
    [Test]
    public async Task ItShouldUpdateSelectedActivity()
    {
        var oldSelectedActivity = new SelectedActivity(IdOf<Project>.Random(), IdOf<Activity>.Random());
        var newSelectedActivity = new SelectedActivity(IdOf<Project>.Random(), IdOf<Activity>.Random());

        var state = NewState();
        state.SelectedActivity = oldSelectedActivity;
        state = await ActionHandler.HandleAsync(state, new SelectActivityAction(newSelectedActivity), CancellationToken.None);

        Assert.That(state.SelectedActivity, Is.EqualTo(newSelectedActivity));
    }

    [Test]
    public async Task ItShouldReturnNullForSelectedProjectAndActivity()
    {
        var state = NewState();
        var projectId = IdOf<Project>.Random();
        var activityId = IdOf<Activity>.Random();
        state.Projects.Add(new ProjectViewModel
        {
            Id = projectId,
            Name = "Alpha",
            Activities = new List<ActivityViewModel>
            {
                new () { Id = activityId, Name = "One" },
                new () { Id = IdOf<Activity>.Random(), Name = "Two" },
            }
        });

        state = await ActionHandler.HandleAsync(state,
            new SelectActivityAction(new SelectedActivity(projectId, activityId)),
            CancellationToken.None);

        Assert.That(state.GetSelectedActivity()?.Name, Is.EqualTo("One"));
    }
}
