using Cysharp.Threading.Tasks;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace GIGXR.Platform.Networking.Commands
{
    public class ConnectNetworkCommand : BaseNetworkCommand
    {
        protected override UniTask<bool> ExecuteInternalAsync()
        {
            if (NetworkClientState == ClientState.PeerCreated ||
                NetworkClientState == ClientState.Disconnected)
            {
                if (!ConnectUsingSettings())
                {
                    return UniTask.FromResult(false);
                }                
            }
            else
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
            Debug.LogWarning($"Error connecting: {cause}");
            Promise.TrySetResult(false);
        }

        protected virtual ClientState NetworkClientState => PhotonNetwork.NetworkClientState;

        protected virtual bool ConnectUsingSettings() => PhotonNetwork.ConnectUsingSettings();
    }
}