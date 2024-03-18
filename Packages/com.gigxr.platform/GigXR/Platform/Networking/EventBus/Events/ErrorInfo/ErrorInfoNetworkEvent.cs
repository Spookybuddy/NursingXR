namespace GIGXR.Platform.Networking.EventBus.Events.ErrorInfo
{
    public class ErrorInfoNetworkEvent : BaseNetworkEvent
    {
        public Photon.Realtime.ErrorInfo ErrorInfo { get; }

        public ErrorInfoNetworkEvent(Photon.Realtime.ErrorInfo errorInfo)
        {
            ErrorInfo = errorInfo;
        }

        public override string ToString()
        {
            return $"ErrorInfo: {ErrorInfo}";
        }
    }
}