using System;
using System.Threading;
using System.Threading.Tasks;
using Quarter.Core.UI.State;
using Microsoft.AspNetCore.Components;

namespace Quarter.Core.UI.Components;

public abstract class QuarterComponent<TState> : ComponentBase, IDisposable
{
    protected TState? State { get; private set; }

    [Inject]
    protected IStateManager<TState>? StateManager { get; set; }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        if (StateManager is null) throw new ArgumentException("No StateManager injected");

        State = StateManager.State;
        StateManager.OnChange += OnStateChange;

        await base.SetParametersAsync(parameters);
    }

    private void OnStateChange(object? sender, EventArgs e)
        => OnApplicationStateChange();

    protected virtual void OnApplicationStateChange()
    {
        // This is part of the poor mans state-manager. Whenever the application state changes, pretend that the
        // component was just recreated.
        // This is obviously costly, when it start to hurt make it opt-in (override) by those children that want it.
        // When that is starting to hurt, find a better solution.

        OnParametersSet();
        _ = InvokeAsync(StateHasChanged);
    }

    protected Task DispatchAsync(IAction action)
        => DispatchAsync(action, CancellationToken.None);

    protected async Task DispatchAsync(IAction action, CancellationToken ct)
    {
        if (StateManager is null) throw new ArgumentException("No StateManager injected");

        await StateManager.DispatchAsync(action, ct);
    }

    public virtual void Dispose()
    {
        if (StateManager is not null)
            StateManager.OnChange -= OnStateChange;
    }
}
