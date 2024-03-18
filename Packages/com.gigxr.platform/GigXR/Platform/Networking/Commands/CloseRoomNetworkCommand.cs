using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;

namespace GIGXR.Platform.Networking.Commands
{
    internal class CloseRoomNetworkCommand : INetworkCommand
    {
        public UniTask<bool> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var room = CurrentRoom;

            if (room == null)
                return UniTask.FromResult(false);

            room.IsOpen = false;
            room.IsVisible = false;

            var leaveRoomCommand = new LeaveRoomNetworkCommand();
            return leaveRoomCommand.ExecuteAsync(cancellationToken);
        }

        [CanBeNull] protected virtual Room CurrentRoom => PhotonNetwork.CurrentRoom;

    }
}