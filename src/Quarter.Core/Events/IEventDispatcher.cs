using System;
using System.Threading;
using System.Threading.Tasks;

namespace Quarter.Core.Events
{
    /// <summary>
    /// Internal event dispatcher used to notify subscribers with events such as:
    /// - Aggregate changes
    /// - Ephemeral system events
    /// - Etc.
    ///
    /// The event dispatcher is single threaded, each subscriber is notified in sequence!
    /// </summary>
    public interface IEventDispatcher : IDisposable
    {
        /// <summary>
        /// Dispatch an event to any active subscriber.
        /// The method might, or might not, complete after the subscribers are notified. Assume that the task returned
        /// by this method is completed before any subscribers are notified.
        /// </summary>
        /// <param name="ev"></param>
        /// <returns></returns>
        Task Dispatch(IEvent ev);

        /// <summary>
        /// Subscribe to events of type T
        /// </summary>
        /// <param name="subscriber">The function that will be called upon receiving an event</param>
        /// <param name="ct">Cancellation token used to cancel this subscription</param>
        /// <typeparam name="T">The type of event to subscribe to</typeparam>
        /// <returns>A task that will be completed if this subscription is cancelled or when the dispatcher ends</returns>
        Task Subscribe<T>(Func<T, Task> subscriber, CancellationToken ct) where T : IEvent;
    }
}