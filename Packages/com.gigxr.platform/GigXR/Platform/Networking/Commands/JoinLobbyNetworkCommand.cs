using Photon.Pun;
using UnityEngine;

namespace GIGXR.Platform.Networking.Commands
{
    using Cysharp.Threading.Tasks;

    public class JoinLobbyNetworkCommand : BaseNetworkCommand
    {
        public JoinLobbyNetworkCommand()
        {
        }

        protected override UniTask<bool> ExecuteInternalAsync()
        {
            if (!JoinLobby())
            {
                return UniTask.FromResult(false);
            }

            return Promise.Task;
        }

        public override void OnJoinedLobby()
        {
            Promise.TrySetResult(true);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.Log($"Error joining lobby: {message}");
            Promise.TrySetResult(false);
        }

        protected virtual bool JoinLobby() => PhotonNetwork.JoinLobby();
    }
}