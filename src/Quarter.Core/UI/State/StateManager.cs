using System;
using System.Threading;
using System.Threading.Tasks;

namespace Quarter.Core.UI.State
{
    /// <summary>
    /// Type signature / marked for any action.
    /// </summary>
    public interface IAction
    {
    }

    public interface IStateManager<out TState>
    {
        /// <summary>
        /// The global application state which can be used by any QuarterComponent.
        /// </summary>
        public TState State { get; }

        /// <summary>
        /// Triggered whenever something in the application state has changed.
        /// </summary>
        public event EventHandler OnChange;

        Task DispatchAsync(IAction action, CancellationToken ct);
    }

    public interface IActionHandler<TState>
    {
        Task<TState> HandleAsync(TState currentState, IAction action, CancellationToken ct);
    }

    public class StateManager<TState> : IStateManager<TState>
    {
        public TState State { get; private set; }
        public event EventHandler? OnChange;

        private readonly IActionHandler<TState> _actionHandler;

        public StateManager(TState initialState, IActionHandler<TState> actionHandler)
        {
            _actionHandler = actionHandler;
            State = initialState;
        }

        public async Task DispatchAsync(IAction action, CancellationToken ct)
        {
            State = await _actionHandler.HandleAsync(State, action, ct);

            // This is pretty aggressive. Assume state is changed, which will cause all QuarterComponents to be
            // re-rendered. Make some tests and see how big of performance issue this is (if any). Maybe split the state
            // into sub-state / segments orh paths that can be subscribed to instead
            OnChange?.Invoke(this, EventArgs.Empty);
        }
    }
}