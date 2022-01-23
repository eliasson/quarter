using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Quarter.Core.UI.State;
using Quarter.State;

namespace Quarter.UnitTest.TestUtils;

public class TestStateManager : IStateManager<ApplicationState>
{
    public ApplicationState State { get; } = new();
    public event EventHandler OnChange;
    public readonly IList<IAction> DispatchedActions = new List<IAction>();

    public Task DispatchAsync(IAction action, CancellationToken ct)
    {
        DispatchedActions.Add(action);
        OnChange?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }
}