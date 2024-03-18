namespace GIGXR.Test.Editor.Platform.Networking.EventBus
{
    using ExitGames.Client.Photon;
    using GIGXR.Platform.Networking.EventBus;
    using GIGXR.Platform.Utilities;
    using NUnit.Framework;
    using Photon.Realtime;
    using System;
    using System.Collections;
    using UnityEngine.TestTools;

    public class CustomNetworkEventHandlerThrottleDecoratorTests
    {
        [UnityTest]
        public IEnumerator CustomNetworkEventHandlerThrottleDecorator_ThrottlesFastEvents()
        {
            // Arrange
            var parentHandler = new TestCustomNetworkEventHandler();
            var handler = new CustomNetworkEventHandlerThrottleDecorator
                (parentHandler, TimeSpan.FromMilliseconds(200), 2);

            // Act
            handler.Enable();
            handler.RaiseNetworkEvent(new TestCustomNetworkEvent("some-event"));
            handler.RaiseNetworkEvent(new TestCustomNetworkEvent("some-event"));
            handler.RaiseNetworkEvent(new TestCustomNetworkEvent("some-event"));
            handler.RaiseNetworkEvent(new TestCustomNetworkEvent("some-event"));
            handler.RaiseNetworkEvent(new TestCustomNetworkEvent("some-event"));
            handler.RaiseNetworkEvent(new TestCustomNetworkEvent("some-event"));
            yield return new WaitForTimeSpan(TimeSpan.FromMilliseconds(200));
            handler.Disable();
            // Assert
            Assert.AreEqual(2, parentHandler.RaiseEventCount);
        }

        [UnityTest]
        public IEnumerator CustomNetworkEventHandlerThrottleDecorator_ResetsOnMinimumEventInterval()
        {
            // Arrange
            var parentHandler = new TestCustomNetworkEventHandler();
            var handler = new CustomNetworkEventHandlerThrottleDecorator
                (parentHandler, TimeSpan.FromMilliseconds(200), 2);

            // Act
            handler.Enable();
            handler.RaiseNetworkEvent(new TestCustomNetworkEvent("some-event"));
            handler.RaiseNetworkEvent(new TestCustomNetworkEvent("some-event"));
            handler.RaiseNetworkEvent(new TestCustomNetworkEvent("some-event"));
            handler.RaiseNetworkEvent(new TestCustomNetworkEvent("some-event"));
            yield return new WaitForTimeSpan(TimeSpan.FromMilliseconds(200));
            handler.RaiseNetworkEvent(new TestCustomNetworkEvent("some-event"));
            handler.RaiseNetworkEvent(new TestCustomNetworkEvent("some-event"));
            handler.RaiseNetworkEvent(new TestCustomNetworkEvent("some-event"));
            handler.Disable();

            // Assert
            Assert.AreEqual(4, parentHandler.RaiseEventCount);
        }
    }

    public class TestCustomNetworkEventHandler : ICustomNetworkEventHandler
    {
        public int RaiseEventCount { get; private set; } = 0;

        public void Enable()
        {
        }

        public void Disable()
        {
        }

        public bool RegisterNetworkEvent<TNetworkEvent, TNetworkEventSerializer>(byte eventCode)
            where TNetworkEvent : ICustomNetworkEvent
            where TNetworkEventSerializer : ICustomNetworkEventSerializer<TNetworkEvent>
        {
            throw new NotImplementedException();
        }

        public bool RaiseNetworkEvent<TNetworkEvent>(TNetworkEvent @event)
            where TNetworkEvent : ICustomNetworkEvent
        {
            RaiseEventCount++;
            return true;
        }

        public bool RaiseNetworkEvent(ICustomNetworkEvent @event, Type type)
        {
            RaiseEventCount++;
            return true;
        }
    }

    public class TestCustomNetworkEvent2 : ICustomNetworkEvent
    {
        public TestCustomNetworkEvent2(string testValue) => TestValue = testValue;

        public string TestValue { get; }
        public RaiseEventOptions RaiseEventOptions => RaiseEventOptions.Default;
        public SendOptions SendOptions => SendOptions.SendUnreliable;
    }
}