using System.Threading.Tasks;
using Quarter.Core.Events;
using Quarter.Core.UnitTest.TestUtils;
using NUnit.Framework;

namespace Quarter.Core.UnitTest.Events
{
    public abstract class EventDispatcherTest
    {
        public class WhenThereAreNoSubscribers : EventDispatcherTest
        {
            [Test]
            public void ItShouldNotFailPublishing()
            {
                Assert.DoesNotThrowAsync(() => _eventDispatcher.Dispatch(new TestEvent("Nonsense")));
            }
        }

        public class WhenThereAreMultipleSubscribers : EventDispatcherTest
        {
            private TestSubscriber<TestEvent> _subscriberAlpha;
            private TestSubscriber<TestEvent> _subscriberBeta;

            [OneTimeSetUp]
            public async Task Dispatch()
            {
                _subscriberAlpha = new TestSubscriber<TestEvent>(_eventDispatcher);
                _subscriberBeta = new TestSubscriber<TestEvent>(_eventDispatcher);

                await _eventDispatcher.Dispatch(new TestEvent("Nonsense"));

                var done = Task.WhenAll(_subscriberAlpha.Subscription, _subscriberBeta.Subscription);

                _eventDispatcher.Dispose();

                await done;
            }

            [Test]
            public void ItShouldBeDispatchedToSubscriberAlpha()
                => Assert.That(_subscriberAlpha.CollectedEvents, Is.EqualTo(new[] { new TestEvent("Nonsense") }));

            [Test]
            public void ItShouldBeDispatchedToSubscriberBeta()
                => Assert.That(_subscriberBeta.CollectedEvents, Is.EqualTo(new[] { new TestEvent("Nonsense") }));

            [Test]
            public void ItShouldCompleteTheSubscribers()
            {
                Assert.Multiple(() =>
                {
                    Assert.True(_subscriberAlpha.Subscription.IsCompleted);
                    Assert.True(_subscriberBeta.Subscription.IsCompleted);
                });
            }
        }

        public class WhenASubscriptionIsCancelled : EventDispatcherTest
        {
            private TestSubscriber<TestEvent> _subscriberAlpha;
            private TestSubscriber<TestEvent> _subscriberBeta;

            [OneTimeSetUp]
            public async Task Dispatch()
            {
                _subscriberAlpha = new TestSubscriber<TestEvent>(_eventDispatcher);
                _subscriberBeta = new TestSubscriber<TestEvent>(_eventDispatcher);
                _subscriberBeta.Cancel();

                await _eventDispatcher.Dispatch(new TestEvent("Nonsense"));

                _eventDispatcher.Dispose();

                await _subscriberAlpha.Subscription;
            }

            [Test]
            public void ItShouldBeDispatchedToSubscriberAlpha()
                => Assert.That(_subscriberAlpha.CollectedEvents, Is.EqualTo(new[] { new TestEvent("Nonsense") }));

            [Test]
            public void ItShouldNotBeDispatchedToSubscriberBeta()
                => Assert.That(_subscriberBeta.CollectedEvents, Is.Empty);

            [Test]
            public void ItShouldCompleteSubscriberAlpha()
                => Assert.True(_subscriberAlpha.Subscription.IsCompleted);

            [Test]
            public void ItShouldHaveCancelledSubscriberBeta()
                => Assert.True(_subscriberBeta.Subscription.IsCanceled);
        }

        private record TestEvent(string Payload) : IEvent;

        private IEventDispatcher _eventDispatcher;

        [OneTimeSetUp]
        public void SetUp()
        {
            _eventDispatcher = new EventDispatcher();
        }
    }
}