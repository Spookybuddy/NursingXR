using Cysharp.Threading.Tasks;
using Photon.Pun;
using Photon.Realtime;

namespace GIGXR.Platform.Networking.Commands
{
    public class DisconnectNetworkCommand : BaseNetworkCommand
    {
        protected override UniTask<bool> ExecuteInternalAsync()
        {
            if (NetworkClientState == ClientState.Disconnected ||
                NetworkClientState == ClientState.PeerCreated)
            {
                // Already disconnected or never connected.
                return UniTask.FromResult(true);
            }

            if (NetworkClientState != ClientState.Disconnecting)
            {
                // Not already disconnecting, so start the disconnect process.
                Disconnect();
            }

            return Promise.Task;
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            // Debug.Log($"DisconnectCommand.OnDisconnected: {cause}");
            Promise.TrySetResult(true);
        }

        protected virtual ClientState NetworkClientState => PhotonNetwork.NetworkClientState;

        protected virtual void Disconnect() => PhotonNetwork.Disconnect();
    }
}