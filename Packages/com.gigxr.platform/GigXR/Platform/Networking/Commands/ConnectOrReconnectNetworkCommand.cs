using System.Threading;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using Photon.Realtime;

namespace GIGXR.Platform.Networking.Commands
{
    /// <summary>
    /// Composite command to either reconnect or connect to the Photon network.
    /// </summary>
    internal class ConnectOrReconnectNetworkCommand : INetworkCommand
    {
        public UniTask<bool> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            // This is the first connection is ClientState is PeerCreated, or the user logged out and is 
            // disconnected
            if (NetworkClientState == ClientState.PeerCreated ||
                NetworkClientState == ClientState.Disconnected)
            {
                // In the initial state, issue a connect command.
                var connectCommand = new ConnectNetworkCommand();
                return connectCommand.ExecuteAsync(cancellationToken);
            }

            // If not, a previous connection has been established so try to reconnect.
            var reconnectCommand = new ReconnectNetworkCommand();
            return reconnectCommand.ExecuteAsync(cancellationToken);
        }

        protected virtual ClientState NetworkClientState => PhotonNetwork.NetworkClientState;

        protected virtual string LocalUserName => PhotonNetwork.LocalPlayer.NickName;
    }
}