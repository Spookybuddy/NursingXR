using Cysharp.Threading.Tasks;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace GIGXR.Platform.Networking.Commands
{
    public class JoinOrCreateRoomNetworkCommand : BaseNetworkCommand
    {
        protected string RoomName { get; }
        protected RoomOptions RoomOptions { get; }

        public JoinOrCreateRoomNetworkCommand(string roomName, RoomOptions roomOptions)
        {
            RoomName = roomName;
            RoomOptions = roomOptions;
        }

        protected override UniTask<bool> ExecuteInternalAsync()
        {
            if (!JoinOrCreateRoom(RoomName, RoomOptions, TypedLobby.Default))
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
            Promise.TrySetResult(false);
        }

        protected virtual bool JoinOrCreateRoom(string roomName, RoomOptions roomOptions, TypedLobby typedLobby) =>
            PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, typedLobby);
    }
}