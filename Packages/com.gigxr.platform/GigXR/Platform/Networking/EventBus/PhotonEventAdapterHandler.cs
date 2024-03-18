using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using GIGXR.Platform.Core.EventBus;
using GIGXR.Platform.Networking.EventBus.Events.Connection;
using GIGXR.Platform.Networking.EventBus.Events.ErrorInfo;
using GIGXR.Platform.Networking.EventBus.Events.InRoom;
using GIGXR.Platform.Networking.EventBus.Events.Matchmaking;
using GIGXR.Platform.Networking.EventBus.Events.WebRpc;
using Photon.Pun;
using Photon.Realtime;

namespace GIGXR.Platform.Networking.EventBus
{
    /// <summary>
    /// Responsible for forwarding built-in Photon callbacks to the application event bus.
    /// </summary>
    public partial class PhotonEventAdapterHandler : IDisposable
    {
        private readonly IGigEventPublisher<NetworkManager> eventBus;
        public bool Enabled { get; private set; }

        public PhotonEventAdapterHandler(IGigEventPublisher<NetworkManager> eventBus)
        {
            this.eventBus = eventBus;

            // We want events to be reused as we do not cache any events from Photon
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.ReuseEventInstance = true;
        }

        public void Dispose()
        {
            RemoveCallbackTarget();
        }

        public void Enable()
        {
            if (Enabled)
                return;

            Enabled = true;
            AddCallbackTarget();
        }

        public void Disable()
        {
            if (!Enabled)
                return;

            Enabled = false;
            RemoveCallbackTarget();
        }

        /// <inheritdoc cref="PhotonNetwork.AddCallbackTarget"/>
        protected void AddCallbackTarget()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        /// <inheritdoc cref="PhotonNetwork.RemoveCallbackTarget"/>
        protected void RemoveCallbackTarget()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }
    }

    public partial class PhotonEventAdapterHandler : IConnectionCallbacks
    {
        public void OnConnected()
        {
            eventBus.Publish(new ConnectedNetworkEvent());
        }

        public void OnConnectedToMaster()
        {
            eventBus.Publish(new ConnectedToMasterNetworkEvent());
        }

        public void OnDisconnected(DisconnectCause cause)
        {
            // Convert Photon cause to GIGXR agnostic cause so that any class that needs to Subscribe to the DisconnectedNetworkEvent
            // will not have to know about Photon
            // Right now, the DisconnectionCause enum is a copy of DisconnectCause, so converting is easy, if has changed, this will no longer be valid
            eventBus.Publish(new DisconnectedNetworkEvent((DisconnectionCause)cause));
        }

        public void OnRegionListReceived(RegionHandler regionHandler)
        {
            // Ignored.
        }

        public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
            // Ignored.
        }

        public void OnCustomAuthenticationFailed(string debugMessage)
        {
            // Ignored.
        }
    }

    public partial class PhotonEventAdapterHandler : IMatchmakingCallbacks
    {
        public void OnFriendListUpdate(List<FriendInfo> friendList)
        {
            // Ignored.
        }

        public void OnCreatedRoom()
        {
            eventBus.Publish(new CreatedRoomNetworkEvent());
        }

        public void OnCreateRoomFailed(short returnCode, string message)
        {
            eventBus.Publish(new CreateRoomFailedNetworkEvent(returnCode, message));
        }

        public void OnJoinedRoom()
        {
            eventBus.Publish(new JoinedRoomNetworkEvent(PhotonNetwork.AuthValues?.UserId ?? Guid.Empty.ToString(),
                                                        PhotonNetwork.NickName ?? "User"));
        }

        public void OnJoinRoomFailed(short returnCode, string message)
        {
            UnityEngine.Debug.LogError($"[Photon] OnJoinRoomFailed: {message}");
            eventBus.Publish(new JoinRoomFailedNetworkEvent(returnCode, "Photon", message));
        }

        public void OnJoinRandomFailed(short returnCode, string message)
        {
            // Ignored.
        }

        public void OnLeftRoom()
        {
            eventBus.Publish(new LeftRoomNetworkEvent());
        }
    }

    public partial class PhotonEventAdapterHandler : IInRoomCallbacks
    {
        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            eventBus.Publish(new PlayerEnteredRoomNetworkEvent(newPlayer));
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            eventBus.Publish(new PlayerLeftRoomNetworkEvent(otherPlayer));
        }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            eventBus.Publish(new RoomPropertiesUpdateNetworkEvent(propertiesThatChanged));
        }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            eventBus.Publish(new PlayerPropertiesUpdateNetworkEvent(targetPlayer, changedProps));
        }

        public void OnMasterClientSwitched(Player newMasterClient)
        {
            eventBus.Publish(new MasterClientSwitchedEvent(newMasterClient));
        }
    }

    public partial class PhotonEventAdapterHandler : ILobbyCallbacks
    {
        public void OnJoinedLobby()
        {
            eventBus.Publish(new JoinedLobbyEvent());
        }

        public void OnLeftLobby()
        {
            eventBus.Publish(new LeftLobbyEvent());
        }

        public void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            eventBus.Publish(new RoomListUpdateEvent(roomList));
        }

        public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
        {
            eventBus.Publish(new LobbyStatisticsUpdateEvent(lobbyStatistics));
        }
    }

    public partial class PhotonEventAdapterHandler : IWebRpcCallback
    {
        public void OnWebRpcResponse(OperationResponse response)
        {
            eventBus.Publish(new WebRpcResponseNetworkEvent(response));
        }
    }

    public partial class PhotonEventAdapterHandler : IErrorInfoCallback
    {
        public void OnErrorInfo(Photon.Realtime.ErrorInfo errorInfo)
        {
            eventBus.Publish(new ErrorInfoNetworkEvent(errorInfo));
        }
    }
}