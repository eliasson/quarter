using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.State;

namespace Quarter.UnitTest.State;

[TestFixture]
public class SelectEraseActivityActionTest : ActionHandlerTestCase
{
    [Test]
    public async Task ItShouldRemoveSelectedActivityFromState()
    {
        var state = NewState();
        state.SelectedActivity = new SelectedActivity(IdOf<Project>.Random(), IdOf<Activity>.Random());
        state = await ActionHandler.HandleAsync(state, new SelectEraseActivityAction(), CancellationToken.None);

        Assert.That(state.SelectedActivity, Is.Null);
    }

    [Test]
    public void ItShouldBeNoOpIfNoActivityIsSelected()
    {
        var state = NewState();

        Assert.DoesNotThrowAsync(async () => await ActionHandler.HandleAsync(state, new SelectEraseActivityAction(), CancellationToken.None));
    }
}