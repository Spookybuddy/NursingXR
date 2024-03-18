namespace GIGXR.Platform.Networking.EventBus.Events.Matchmaking
{
    public class JoinRoomFailedNetworkEvent : BaseNetworkEvent
    {
        public short ReturnCode { get; }
        public string Message { get; }
        public string NetworkName { get; }

        public JoinRoomFailedNetworkEvent(short returnCode, string networkName, string message)
        {
            ReturnCode = returnCode;
            Message = message;
            NetworkName = networkName;
        }

        public override string ToString()
        {
            return $"ReturnCode: {ReturnCode}, Message: {Message}";
        }
    }
}