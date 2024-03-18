namespace GIGXR.Platform.HMD.AppEvents.Events
{
    public class LockNetworkLogEvent : BaseNetworkLogEvent
    {
        public bool ReadOnly { get; }

        public LockNetworkLogEvent(bool lockStatus)
        {
            ReadOnly = lockStatus;
        }
    }
}