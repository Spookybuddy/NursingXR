using Photon.Pun;
using UnityEngine;

namespace GIGXR.Platform.Networking.Commands
{
    using Cysharp.Threading.Tasks;

    public class JoinRoomNetworkCommand : BaseNetworkCommand
    {
        protected string RoomName { get; }

        public JoinRoomNetworkCommand(string roomName)
        {
            RoomName = roomName;
        }

        protected override UniTask<bool> ExecuteInternalAsync()
        {
            if (!JoinRoom(RoomName))
            {
                return UniTask.FromResult(false);
            }

            return Promise.Task;
        }

        public override void OnJoinedRoom()
        {
            Promise.TrySetResult(true);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.Log($"Error joining room: {message}");
            Promise.TrySetResult(false);
        }

        protected virtual bool JoinRoom(string roomName) => PhotonNetwork.JoinRoom(roomName);
    }
}