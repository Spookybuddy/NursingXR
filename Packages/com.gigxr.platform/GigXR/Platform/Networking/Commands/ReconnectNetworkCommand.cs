using Cysharp.Threading.Tasks;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace GIGXR.Platform.Networking.Commands
{
    public class ReconnectNetworkCommand : BaseNetworkCommand
    {
        protected override UniTask<bool> ExecuteInternalAsync()
        {
            if (NetworkClientState != ClientState.Disconnected)
            {
                Debug.LogWarning(
                    $"Can only reconnect in the state of Disconnected. Current state: {PhotonNetwork.NetworkClientState}");
                return UniTask.FromResult(false);
            }

            if (!Reconnect())
            {
                return UniTask.FromResult(false);
            }

            return Promise.Task;
        }

        public override void OnConnectedToMaster()
        {
            Promise.TrySetResult(true);
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarning($"Error reconnecting: {cause}");
            Promise.TrySetResult(false);
        }

        protected virtual ClientState NetworkClientState => PhotonNetwork.NetworkClientState;

        protected virtual bool Reconnect() => PhotonNetwork.Reconnect();
    }
}