using ExitGames.Client.Photon;

namespace GIGXR.Platform.Networking.EventBus.Events.WebRpc
{
    public class WebRpcResponseNetworkEvent : BaseNetworkEvent
    {
        public OperationResponse Response { get; }

        public WebRpcResponseNetworkEvent(OperationResponse response)
        {
            Response = response;
        }

        public override string ToString()
        {
            return $"Response: {Response}";
        }
    }
}