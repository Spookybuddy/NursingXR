using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine;

namespace GIGXR.Platform.Networking.Commands
{
    public class RejoinRoomNetworkCommand : BaseNetworkCommand
    {
        protected string RoomName { get; }

        public RejoinRoomNetworkCommand(string roomName)
        {
            RoomName = roomName;
        }

        protected override UniTask<bool> ExecuteInternalAsync()
        {
            if (!RejoinRoom(RoomName))
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
            Debug.Log($"Error rejoining room: {message}");
            Promise.TrySetResult(false);
        }

        protected virtual bool RejoinRoom(string roomName) => PhotonNetwork.RejoinRoom(roomName);
    }
}