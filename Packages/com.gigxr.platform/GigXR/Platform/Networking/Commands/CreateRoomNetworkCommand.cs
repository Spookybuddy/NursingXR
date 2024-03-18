using Cysharp.Threading.Tasks;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace GIGXR.Platform.Networking.Commands
{
    public class CreateRoomNetworkCommand : BaseNetworkCommand
    {
        protected string RoomName { get; }
        protected RoomOptions RoomOptions { get; }

        public CreateRoomNetworkCommand(string roomName, RoomOptions roomOptions)
        {
            RoomName = roomName;
            RoomOptions = roomOptions;
        }

        protected override UniTask<bool> ExecuteInternalAsync()
        {
            if (!CreateRoom(RoomName, RoomOptions))
            {
                return UniTask.FromResult(false);
            }

            return Promise.Task;
        }

        public override void OnCreatedRoom()
        {
            Promise.TrySetResult(true);
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.Log($"Error creating room: {message}");
            Promise.TrySetResult(false);
        }

        protected virtual bool CreateRoom(string roomName, RoomOptions roomOptions) =>
            PhotonNetwork.CreateRoom(roomName, roomOptions);
    }
}