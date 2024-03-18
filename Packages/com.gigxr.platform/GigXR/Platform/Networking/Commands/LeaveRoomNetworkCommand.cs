using Cysharp.Threading.Tasks;
using Photon.Pun;

namespace GIGXR.Platform.Networking.Commands
{
    public class LeaveRoomNetworkCommand : BaseNetworkCommand
    {
        protected override UniTask<bool> ExecuteInternalAsync()
        {
            if (!LeaveRoom())
            {
                return UniTask.FromResult(false);
            }

            return Promise.Task;
        }

        public override void OnConnectedToMaster()
        {
            Promise.TrySetResult(true);
        }

        protected virtual bool LeaveRoom()
        {
            if (PhotonNetwork.InRoom) 
                PhotonNetwork.LeaveRoom(false);
            
            return false;
        }
    }
}