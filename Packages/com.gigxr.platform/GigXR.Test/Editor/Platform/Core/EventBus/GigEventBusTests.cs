using System.Threading;
using GIGXR.Platform.Core.EventBus;
using NUnit.Framework;

namespace GIGXR.Test.Editor.Platform.Core.EventBus
{
    public class GigEventBusTests
    {
        [Test]
        public void GigEventBus_ReturnsFalseWhenPublishingToZeroSubscribers()
        {
            // Arrange
            IGigEventBus<GigEventBusTests> eventBus = new GigEventBus<GigEventBusTests>();

            // Act
            var subscribersNotified = eventBus.Publish(new TestEvent("test"));

            // Assert
            Assert.IsFalse(subscribersNotified);
        }

        [Test]
        public void GigEventBus_CanPublishToOneSubscriber()
        {
            // Arrange
            IGigEventBus<GigEventBusTests> eventBus = new GigEventBus<GigEventBusTests>();
            var subscriptionCalled = false;
            var message = "";
            eventBus.Subscribe<TestEvent>(@event =>
            {
                subscriptionCalled = true;
                message = @event.TestProperty;
            });

            // Act
            var subscribersNotified = eventBus.Publish(new TestEvent("test message"));

            // Assert
            Assert.IsTrue(subscribersNotified);
            Assert.IsTrue(subscriptionCalled);
            Assert.AreEqual("test message", message);
        }

        [Test]
        public void GigEventBus_CanPublishToManySubscribers()
        {
            // Arrange
            IGigEventBus<GigEventBusTests> eventBus = new GigEventBus<GigEventBusTests>();
            var subscriptionCalledCount = 0;
            eventBus.Subscribe<TestEvent>(@event => { Interlocked.Increment(ref subscriptionCalledCount); });
            eventBus.Subscribe<TestEvent>(@event => { Interlocked.Increment(ref subscriptionCalledCount); });
            eventBus.Subscribe<TestEvent>(@event => { Interlocked.Increment(ref subscriptionCalledCount); });

            // Act
            var subscribersNotified = eventBus.Publish(new TestEvent("test"));

            // Assert
            Assert.IsTrue(subscribersNotified);
            Assert.AreEqual(3, subscriptionCalledCount);
        }

        [Test]
        public void GigEventBus_SendsSameObjectReferenceToSubscribers()
        {
            // Arrange
            IGigEventBus<GigEventBusTests> eventBus = new GigEventBus<GigEventBusTests>();
            var sentEvent = new TestEvent("test");
            object receivedEvent = null;
            eventBus.Subscribe<TestEvent>(@event => { receivedEvent = @event; });

            // Act
            var subscribersNotified = eventBus.Publish(sentEvent);

            // Assert
            Assert.IsTrue(subscribersNotified);
            Assert.AreSame(sentEvent, receivedEvent);
            Assert.AreNotSame(new TestEvent("test"), receivedEvent);
        }

        [Test]
        public void GigEventBus_AllowsForUnsubscribing()
        {
            // Arrange
            IGigEventBus<GigEventBusTests> eventBus = new GigEventBus<GigEventBusTests>();
            var subscriptionCalledCount = 0;
            void EventHandler(TestEvent @event) => Interlocked.Increment(ref subscriptionCalledCount);
            eventBus.Subscribe<TestEvent>(EventHandler);
            eventBus.Publish(new TestEvent("Test One")); // subscriptionCalledCount = 1 now
            eventBus.Unsubscribe<TestEvent>(EventHandler);

            // Act
            var subscribersNotified = eventBus.Publish(new TestEvent("Test Two")); // This should do nothing.

            // Assert
            Assert.IsFalse(subscribersNotified);
            Assert.AreEqual(1, subscriptionCalledCount);
        }

        [Test]
        public void GigEventBus_CanPublishToASpecificType()
        {
            // Arrange
            IGigEventBus<GigEventBusTests> eventBus = new GigEventBus<GigEventBusTests>();
            var testEventResult = "";
            var testEventTwoResult = "";
            eventBus.Subscribe<TestEvent>(@event => { testEventResult = @event.TestProperty; });
            eventBus.Subscribe<TestEventTwo>(@event => { testEventTwoResult = @event.TestProperty; });

            // Act
            eventBus.Publish(new TestEvent("result from one"));
            eventBus.Publish(new TestEventTwo("result from two"));

            // Assert
            Assert.AreEqual("result from one", testEventResult);
            Assert.AreEqual("result from two", testEventTwoResult);
        }

        [Test]
        public void GigEventBus_DoesntAllowMultipleSubscriptionsFromTheSameHandler()
        {
            // Arrange
            IGigEventBus<GigEventBusTests> eventBus = new GigEventBus<GigEventBusTests>();
            var subscriptionCalledCount = 0;
            void EventHandler(TestEvent @event) => Interlocked.Increment(ref subscriptionCalledCount);

            // Act
            var firstSubscriptionResult = eventBus.Subscribe<TestEvent>(EventHandler);
            var secondSubscriptionResult = eventBus.Subscribe<TestEvent>(EventHandler);
            eventBus.Publish(new TestEvent("test"));

            // Assert
            Assert.IsTrue(firstSubscriptionResult);
            Assert.IsFalse(secondSubscriptionResult);
            Assert.AreEqual(1, subscriptionCalledCount);
        }

        [Test]
        public void GigEventBus_ReturnsBoolRepresentingUnsubscribeResult()
        {
            // Arrange
            IGigEventBus<GigEventBusTests> eventBus = new GigEventBus<GigEventBusTests>();

            void EventHandler(TestEvent @event)
            {
            }

            eventBus.Subscribe<TestEvent>(EventHandler);

            // Act
            var firstUnsubscribeResult = eventBus.Unsubscribe<TestEvent>(EventHandler);
            var secondUnsubscribeResult = eventBus.Unsubscribe<TestEvent>(EventHandler);

            // Assert
            Assert.IsTrue(firstUnsubscribeResult);
            Assert.IsFalse(secondUnsubscribeResult);
        }

        private class TestEvent : IGigEvent<GigEventBusTests>
        {
            public string TestProperty { get; }
            public TestEvent(string testValue) => TestProperty = testValue;
        }

        private class TestEventTwo : IGigEvent<GigEventBusTests>
        {
            public string TestProperty { get; }
            public TestEventTwo(string testValue) => TestProperty = testValue;
        }
    }
}