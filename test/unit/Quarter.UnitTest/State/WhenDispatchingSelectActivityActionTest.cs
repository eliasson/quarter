using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.State;

namespace Quarter.UnitTest.State;

[TestFixture]
public class WhenDispatchingSelectActivityActionTest : ActionHandlerTestCase
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
}