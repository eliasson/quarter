using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Quarter.Core.Events;

namespace Quarter.Core.UnitTest.TestUtils
{
    public class TestSubscriber<T> where T:IEvent
    {
        public readonly IList<T> CollectedEvents = new List<T>();
        public readonly Task Subscription;
        private readonly CancellationTokenSource _cts;

        public TestSubscriber(IEventDispatcher eventDispatcher)
        {
            _cts = new CancellationTokenSource();
            Subscription = eventDispatcher.Subscribe<T>((ev) =>
            {
                CollectedEvents.Add(ev);
                return Task.CompletedTask;
            }, _cts.Token);
        }

        public void Cancel()
            => _cts.Cancel();

        public async Task<T> EventuallyDispatchedOneEvent()
        {
            var cts = new CancellationTokenSource();
            var dispatchTask = DoEventually(cts.Token);
            var r = await Task.WhenAny(dispatchTask, Task.Delay(3000, cts.Token));
            if (r.IsCompleted && r is Task<T> bt)
                return bt.Result;
            throw new Exception($"Event of type {nameof(T)} never dispatched (although {CollectedEvents.Count} events was dispatched)");

            async Task<T> DoEventually(CancellationToken ct)
            {
                while (CollectedEvents.Count == 0)
                {
                    await Task.Delay(10, ct);
                }
                return CollectedEvents.First();
            }
        }
    }
}