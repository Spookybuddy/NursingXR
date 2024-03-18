using Photon.Pun;

namespace GIGXR.Platform.Networking.Commands
{
    using Cysharp.Threading.Tasks;

    public class LeaveLobbyNetworkCommand : BaseNetworkCommand
    {
        public LeaveLobbyNetworkCommand()
        {
        }

        protected override UniTask<bool> ExecuteInternalAsync()
        {
            if (!LeaveLobby())
            {
                return UniTask.FromResult(false);
            }

            return Promise.Task;
        }

        public override void OnLeftLobby()
        {
            Promise.TrySetResult(true);
        }

        protected virtual bool LeaveLobby()
        {
            if (PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.JoinedLobby)
                return PhotonNetwork.LeaveLobby();
            else
                return false;
        }
    }
}