using System;
using ExitGames.Client.Photon;
using GIGXR.Platform.Core.EventBus;
using GIGXR.Platform.Networking;
using GIGXR.Platform.Networking.EventBus;
using NUnit.Framework;
using Photon.Realtime;

namespace GIGXR.Test.Editor.Platform.Networking.EventBus
{
    public class CustomNetworkEventHandlerTests
    {
        [Test]
        public void CustomNetworkEventHandler_OnEvent_PublishesConcreteTypeAndNotInterface()
        {
            // Arrange
            var eventBus = new TestEventBus();
            var handler = new CustomNetworkEventHandler(eventBus);
            const byte eventCode = 1;
            handler.RegisterNetworkEvent<TestCustomNetworkEvent, TestCustomNetworkEventSerializer>(eventCode);

            var customEvent = new TestCustomNetworkEvent("test-value");
            var serializer = new TestCustomNetworkEventSerializer();

            // Act
            handler.OnEventInternal(eventCode, serializer.Serialize(customEvent));

            // Assert
            Assert.AreEqual(typeof(TestCustomNetworkEvent), eventBus.TypePublished);
        }

        // TODO: More tests
    }

    public class TestEventBus : IGigEventBus<NetworkManager>
    {
        public Type TypePublished { get; private set; }

        public bool Publish<TEvent>(TEvent @event) where TEvent : IGigEvent<NetworkManager>
        {
            TypePublished = typeof(TEvent);
            return true;
        }

        public bool Subscribe<TEvent>(Action<TEvent> eventHandler) where TEvent : IGigEvent<NetworkManager> => true;
        public bool Unsubscribe<TEvent>(Action<TEvent> eventHandler) where TEvent : IGigEvent<NetworkManager> => true;
    }

    public class TestCustomNetworkEvent : ICustomNetworkEvent
    {
        public TestCustomNetworkEvent(string testValue) => TestValue = testValue;

        public string TestValue { get; }
        public RaiseEventOptions RaiseEventOptions { get; } = RaiseEventOptions.Default;
        public SendOptions SendOptions { get; } = SendOptions.SendUnreliable;
    }

    public class TestCustomNetworkEventSerializer : ICustomNetworkEventSerializer<TestCustomNetworkEvent>
    {
        public object[] Serialize(TestCustomNetworkEvent @event) => new object[] {@event.TestValue};
        public TestCustomNetworkEvent Deserialize(object[] data) => new TestCustomNetworkEvent((string)data[0]);
    }
}