namespace GIGXR.Platform.Networking.EventBus.Events
{
    public class PingValueUpdatedNetworkEvent : BaseNetworkEvent
    {
        public int PingValue { get; }
        public PingStatus PingStatus { get; }

        public PingValueUpdatedNetworkEvent(int pingValue, PingStatus pingStatus)
        {
            PingValue = pingValue;
            PingStatus = pingStatus;
        }

        public override string ToString()
        {
            return $"PingValue: {PingValue}, PingStatus: {PingStatus}";
        }
    }
}