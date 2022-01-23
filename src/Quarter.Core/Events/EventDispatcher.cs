using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Quarter.Core.Events
{
    public class EventDispatcher : IEventDispatcher
    {
        private readonly Channel<IEvent> _mainChannel;

        private readonly IList<Channel<IEvent>> _subscriptions = new List<Channel<IEvent>>();

        public EventDispatcher()
        {
            _mainChannel = Channel.CreateUnbounded<IEvent>();

            // Start the main subscription that will "fork" out into separate subscriber channels
            _ = Task.Run(() => MainSubscription(CancellationToken.None));
        }

        public Task Dispatch(IEvent ev)
            => _mainChannel.Writer.WriteAsync(ev).AsTask();

        public Task Subscribe<T>(Func<T, Task> subscriber, CancellationToken ct) where T : IEvent
        {
            var subscription = Channel.CreateUnbounded<IEvent>();

            lock (_mainChannel)
            {
                // TODO: Add a unit-test for when starting N subscribers in parallel.
                _subscriptions.Add(subscription);
            }

            // Cleanup if this subscription gets cancelled before it is started
            ct.Register(() => _subscriptions.Remove(subscription)); // Idempotent, no need to lock

            // Fork a new channel dedicated for this subscription.
            return Task.Run(async () =>
            {
                await foreach (var e in subscription.Reader.ReadAllAsync(ct))
                {
                    if (e is T ee)
                        await subscriber.Invoke(ee);
                }

                _subscriptions.Remove(subscription); // Idempotent, no need to lock
            }, ct);
        }

        private async Task MainSubscription(CancellationToken ct)
        {
            await foreach (var e in _mainChannel.Reader.ReadAllAsync(ct))
            {
                foreach (var subscription in _subscriptions) // Single-threaded access, no need to lock
                    await subscription.Writer.WriteAsync(e, ct);
            }

            // Shutdown any forked subscription as the show is over
            foreach (var subscription in _subscriptions) // Single-threaded access, no need to lock
                subscription.Writer.Complete();
        }

        public void Dispose()
            => _mainChannel.Writer.Complete();
    }
}