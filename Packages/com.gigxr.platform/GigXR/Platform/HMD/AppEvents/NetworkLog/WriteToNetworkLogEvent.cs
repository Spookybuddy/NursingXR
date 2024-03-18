namespace GIGXR.Platform.HMD.AppEvents.Events
{
    public class WriteToNetworkLogEvent : BaseNetworkLogEvent
    {
        public NetworkEventType EventType { get; }
        
        public string Message { get; }

        public WriteToNetworkLogEvent(NetworkEventType eventType, string message)
        {
            EventType = eventType;
            Message = message;
        }
    }
}