namespace GIGXR.Platform.Networking.EventBus
{
    using Cysharp.Threading.Tasks;
    using GIGXR.Platform.Core;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// A decorator for CustomNetworkEventHandler that throttles network events for a given type.
    /// </summary>
    public class CustomNetworkEventHandlerThrottleDecorator : BaseBackgroundHandler, ICustomNetworkEventHandler
    {
        private readonly int countAllowedPerInterval;
        private readonly ICustomNetworkEventHandler handler;

        protected override int MillisecondsDelay { get; }

        private readonly Queue<(ICustomNetworkEvent, Type, DateTime)> EventMessageQueue = new Queue<(ICustomNetworkEvent, Type, DateTime)>();

        /// <summary>
        /// A constructor that specifies the minimumEventInterval.
        /// </summary>
        /// <param name="handler">The handler to decorate.</param>
        /// <param name="eventInterval">Event interval to monitor event frequency.</param>
        /// <param name="countAllowedPerInterval">How many events to allow per eventInterval.</param>
        public CustomNetworkEventHandlerThrottleDecorator(ICustomNetworkEventHandler handler,
            TimeSpan eventInterval, int countAllowedPerInterval)
        {
            this.handler = handler;
            this.countAllowedPerInterval = countAllowedPerInterval;
            MillisecondsDelay = (int)eventInterval.TotalMilliseconds / countAllowedPerInterval;
        }

        public new void Enable()
        {
            handler.Enable();
            base.Enable();
        }

        public new void Disable()
        {
            EventMessageQueue.Clear();
            handler.Disable();
            base.Disable();
        }

        protected override UniTask BackgroundTaskInternalAsync(CancellationToken cancellationToken)
        {
            int currentSendCount = 0;

            while (currentSendCount < countAllowedPerInterval && EventMessageQueue.Count > 0)
            {
                var currentEvent = EventMessageQueue.Dequeue();

                currentSendCount++;
                
                // Note: We can't return the value of RaiseNetworkEvent this way...
                handler.RaiseNetworkEvent(currentEvent.Item1, currentEvent.Item2);                
            }

            return UniTask.CompletedTask;
        }

        public bool RegisterNetworkEvent<TNetworkEvent, TNetworkEventSerializer>(byte eventCode)
            where TNetworkEvent : ICustomNetworkEvent
            where TNetworkEventSerializer : ICustomNetworkEventSerializer<TNetworkEvent> =>
            handler.RegisterNetworkEvent<TNetworkEvent, TNetworkEventSerializer>(eventCode);

        public bool RaiseNetworkEvent<TNetworkEvent>(TNetworkEvent @event)
            where TNetworkEvent : ICustomNetworkEvent
        {
            EventMessageQueue.Enqueue((@event, @event.GetType(), DateTime.UtcNow));

            // Since the events are added to the queue, it will make the call to the RaiseNetworkEvent
            return true;
        }

        public bool RaiseNetworkEvent(ICustomNetworkEvent @event, Type type)
        {
            EventMessageQueue.Enqueue((@event, type, DateTime.UtcNow));

            // Since the events are added to the queue, it will make the call to the RaiseNetworkEvent
            return true;
        }
    }
}