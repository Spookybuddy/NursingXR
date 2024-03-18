namespace GIGXR.Platform.Networking.EventBus.Events.Matchmaking
{
    public class CreateRoomFailedNetworkEvent : BaseNetworkEvent
    {
        public short ReturnCode { get; }
        public string Message { get; }

        public CreateRoomFailedNetworkEvent(short returnCode, string message)
        {
            ReturnCode = returnCode;
            Message = message;
        }

        public override string ToString()
        {
            return $"ReturnCode: {ReturnCode}, Message: {Message}";
        }
    }
}