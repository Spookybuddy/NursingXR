using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GIGXR.Platform.Core;
using GIGXR.Platform.Networking.Commands;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace GIGXR.Platform.Networking
{
    /// <summary>
    /// Responsible for reconnecting to Photon and/or rejoining a room when disconnected.
    /// </summary>
    public class AutoReconnectHandler : BaseBackgroundHandler
    {
        protected override int MillisecondsDelay { get; } = 500;

        private BaseNetworkCommand command;

        public AutoReconnectHandler(bool connectToRoom)
        {
            if(connectToRoom)
            {
                command = new ReconnectAndRejoinNetworkCommand();
            }
            else
            {
                command = new ReconnectNetworkCommand();
            }
        }

        protected override async UniTask BackgroundTaskInternalAsync(CancellationToken cancellationToken)
        {
            // The initial client state is <c>ClientState.PeerCreated</c> so this will not trigger in that case.
            // We only need to reconnect when the client state becomes disconnected
            await UniTask.WaitUntil(() => NetworkClientState == ClientState.Disconnected, PlayerLoopTiming.Update, cancellationToken);

            if(cancellationToken.IsCancellationRequested)
            {
                return;
            }

            // Only try to connect if you have available internet, otherwise when you disconnect, you will generate another disconnect event
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                try
                {
                    await command.ExecuteAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    Debug.LogWarning("Exception occurred while trying to launch the ReconnectAndRejoin Handler. Check your internet connection.");
                }
            }
            else if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogWarning("Client has been disconnected from Photon and there is no internet connection available.");
            }
        }

        protected virtual ClientState NetworkClientState => PhotonNetwork.NetworkClientState;
    }
}