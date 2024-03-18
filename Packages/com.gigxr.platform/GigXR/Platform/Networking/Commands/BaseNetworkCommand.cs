using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace GIGXR.Platform.Networking.Commands
{
    public abstract partial class BaseNetworkCommand : INetworkCommand
    {
        /// <summary>
        /// A <c>TaskCompletionSource</c> to convert event/callback-based code to the Task-based pattern.
        /// </summary>
        protected readonly UniTaskCompletionSource<bool> Promise = new UniTaskCompletionSource<bool>();

        /// <inheritdoc cref="INetworkCommand.ExecuteAsync"/>
        public async UniTask<bool> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                AddCallbackTarget();
                cancellationToken.Register(() => Promise.TrySetCanceled());
                return await UniTask.RunOnThreadPool(ExecuteInternalAsync, true, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"NetworkCommand was canceled: {this}");
                return false;
            }
            catch (Exception exception)
            {
                Debug.LogError($"Exception caught executing NetworkCommand: {this}");
                Debug.LogException(exception);
                return false;
            }
            finally
            {
                RemoveCallbackTarget();
            }
        }

        /// <summary>
        /// Subclasses should override this method to define the primary network command functionality.
        /// </summary>
        /// <returns>Whether the operation succeeded.</returns>
        protected abstract UniTask<bool> ExecuteInternalAsync();

        /// <inheritdoc cref="PhotonNetwork.AddCallbackTarget"/>
        protected virtual void AddCallbackTarget()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        /// <inheritdoc cref="PhotonNetwork.RemoveCallbackTarget"/>
        protected virtual void RemoveCallbackTarget()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }
    }

    public abstract partial class BaseNetworkCommand : IConnectionCallbacks
    {
        public virtual void OnConnected()
        {
        }

        public virtual void OnConnectedToMaster()
        {
        }

        public virtual void OnDisconnected(DisconnectCause cause)
        {
        }

        public virtual void OnRegionListReceived(RegionHandler regionHandler)
        {
        }

        public virtual void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
        }

        public virtual void OnCustomAuthenticationFailed(string debugMessage)
        {
        }
    }

    public abstract partial class BaseNetworkCommand : IMatchmakingCallbacks
    {
        public virtual void OnFriendListUpdate(List<FriendInfo> friendList)
        {
        }

        public virtual void OnCreatedRoom()
        {
        }

        public virtual void OnCreateRoomFailed(short returnCode, string message)
        {
        }

        public virtual void OnJoinedRoom()
        {
        }

        public virtual void OnJoinRoomFailed(short returnCode, string message)
        {
        }

        public virtual void OnJoinRandomFailed(short returnCode, string message)
        {
        }

        public virtual void OnLeftRoom()
        {
        }
    }

    public abstract partial class BaseNetworkCommand : IInRoomCallbacks
    {
        public virtual void OnPlayerEnteredRoom(Player newPlayer)
        {
        }

        public virtual void OnPlayerLeftRoom(Player otherPlayer)
        {
        }

        public virtual void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
        }

        public virtual void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
        }

        public virtual void OnMasterClientSwitched(Player newMasterClient)
        {
        }
    }

    public abstract partial class BaseNetworkCommand : ILobbyCallbacks
    {
        public virtual void OnJoinedLobby()
        {
        }

        public virtual void OnLeftLobby()
        {
        }

        public virtual void OnRoomListUpdate(List<RoomInfo> roomList)
        {
        }

        public virtual void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
        {
        }
    }

    public abstract partial class BaseNetworkCommand : IWebRpcCallback
    {
        public virtual void OnWebRpcResponse(OperationResponse response)
        {
        }
    }

    public abstract partial class BaseNetworkCommand : IErrorInfoCallback
    {
        public virtual void OnErrorInfo(ErrorInfo errorInfo)
        {
        }
    }
}