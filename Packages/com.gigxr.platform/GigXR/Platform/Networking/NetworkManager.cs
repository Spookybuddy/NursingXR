using System;
using UnityEngine;
using ExitGames.Client.Photon;
using GIGXR.Platform.Core.EventBus;
using Photon.Pun;
using Photon.Realtime;
using GIGXR.Platform.Networking.Commands;
using GIGXR.Platform.Networking.EventBus;
using GIGXR.Platform.Networking.EventBus.Events;
using JetBrains.Annotations;
using GIGXR.Platform.AppEvents.Events.Authentication;
using GIGXR.Platform.AppEvents;
using System.Collections.Generic;
using GIGXR.Platform.Scenarios.GigAssets;
using Cysharp.Threading.Tasks;

namespace GIGXR.Platform.Networking
{
    /// <see cref="INetworkManager"/>
    public class NetworkManager : IDisposable, INetworkManager
    {
        // --- Private Variables:

        private readonly ProfileManager profileManager;
        private readonly ConnectionQualityHandler connectionQualityHandler;
        private readonly AutoReconnectHandler networkReconnectHandler;
        private readonly IGigEventBus<NetworkManager> eventBus;
        private readonly AppEventBus appEventBus;
        private readonly ICustomNetworkEventHandler customEventHandler;
        private readonly PhotonEventAdapterHandler photonEventAdapterHandler;

        private readonly TimeSpan defaultCommandTimeout = TimeSpan.FromSeconds(15);

        private readonly RoomOptions _roomOptions = new RoomOptions
        {
            MaxPlayers = 0,
            PublishUserId = true,
            CleanupCacheOnLeave = false,
            PlayerTtl = 0,
            EmptyRoomTtl = 0,
            CustomRoomProperties = new Hashtable()
        };

        // --- Public Properties:

        public virtual bool InRoom => PhotonNetwork.InRoom;

        public virtual bool IsConnected => PhotonNetwork.IsConnected;

        [CanBeNull] public virtual Room CurrentRoom => PhotonNetwork.CurrentRoom;

        public virtual bool IsMasterClient => PhotonNetwork.IsMasterClient;
        public virtual Guid MasterClientId => PhotonNetwork.MasterClient != null ? 
                                                Guid.Parse(PhotonNetwork.MasterClient.UserId) : 
                                                Guid.Empty;

        public virtual int ServerTime => PhotonNetwork.ServerTimestamp;

        // --- Protected Properties:

        [CanBeNull]
        protected virtual AuthenticationValues AuthValues
        {
            get => PhotonNetwork.AuthValues;
            set => PhotonNetwork.AuthValues = value;
        }

        public virtual Player LocalPlayer => PhotonNetwork.LocalPlayer;

        public virtual Player[] AllPlayers => PhotonNetwork.PlayerList;

        // --- Public Methods:

        public NetworkManager(
            ProfileManager profileManager,
            ConnectionQualityHandler connectionQualityHandler,
            IGigEventBus<NetworkManager> eventBus,
            AppEventBus appEventBus,
            ICustomNetworkEventHandler customEventHandler,
            PhotonEventAdapterHandler photonEventAdapterHandler)
        {
            this.profileManager = profileManager;
            this.connectionQualityHandler = connectionQualityHandler;
            this.eventBus = eventBus;
            this.appEventBus = appEventBus;
            this.customEventHandler = customEventHandler;
            this.photonEventAdapterHandler = photonEventAdapterHandler;

            networkReconnectHandler = new AutoReconnectHandler(false);

            connectionQualityHandler.PingUpdate += OnPingUpdate;
            appEventBus.Subscribe<FinishedLogOutEvent>(OnFinishedLogOutEvent);

            photonEventAdapterHandler.Enable();
        }

        public void Dispose()
        {
            customEventHandler.Disable();
            photonEventAdapterHandler.Disable();
            connectionQualityHandler.Disable();
            networkReconnectHandler.Disable();

            connectionQualityHandler.PingUpdate -= OnPingUpdate;

            appEventBus.Unsubscribe<FinishedLogOutEvent>(OnFinishedLogOutEvent);
        }

        /// <see cref="INetworkManager.SetUser"/>
        public void SetUser(string userId, string nickName)
        {
            var authValues = new AuthenticationValues(userId);
            AuthValues = authValues;
            LocalPlayer.NickName = nickName;

            // For compatibility with older builds. In newer builds the userId of the Photon user should be the gmsID.
            AddPropertyToLocalPlayer("gmsID", userId);
        }

        public void AddPropertyToLocalPlayer(string id, object property)
        {
            var customPlayerProperties = LocalPlayer.CustomProperties;

            if (customPlayerProperties == null)
                customPlayerProperties = new Hashtable();

            if(customPlayerProperties.ContainsKey(id))
                customPlayerProperties[id] = property; 
            else
                customPlayerProperties.Add(id, property);

            LocalPlayer.SetCustomProperties(customPlayerProperties);
        }

        public T GetPlayerProperty<T>(Player player, string propertyId)
        {
            if (player.CustomProperties != null &&
                player.CustomProperties.ContainsKey(propertyId))
            {
                return (T)player.CustomProperties[propertyId];
            }
            else
                throw new Exception($"Property {propertyId} Not Found");
        }

        public T GetPlayerPropertyForUser<T>(string userId, string propertyId)
        {
            foreach(var currentPlayer in AllPlayers)
            {
                if(currentPlayer.UserId == userId)
                {
                    return GetPlayerProperty<T>(currentPlayer, propertyId);
                }
            }

            throw new Exception($"Player {userId} Not Found");
        }

        /// <see cref="INetworkManager.ConnectAsync"/>
        public async UniTask<bool> ConnectAsync()
        {
            if (AuthValues == null)
            {
                Debug.LogWarning("You should call SetCredentials() before calling ConnectAsync()!");
            }

            var command = new NetworkCommandTimeoutDecorator(new ConnectOrReconnectNetworkCommand(), defaultCommandTimeout);

            var result = await command.ExecuteAsync();

            if(result)
            {
                connectionQualityHandler.Enable();
                networkReconnectHandler.Enable();
            }
            
            return result;
        }

        /// <see cref="INetworkManager.DisconnectAsync"/>
        public UniTask<bool> DisconnectAsync()
        {
            AuthValues = null;
            LocalPlayer.NickName = "";

            AddPropertyToLocalPlayer("gmsID", "");

            connectionQualityHandler.Disable();
            networkReconnectHandler.Disable();

            var command = new NetworkCommandTimeoutDecorator(new DisconnectNetworkCommand(), defaultCommandTimeout);

            return command.ExecuteAsync();
        }

        /// <see cref="INetworkManager.JoinLobbyAsync"/>
        public UniTask<bool> JoinLobbyAsync()
        {
            var command = new NetworkCommandTimeoutDecorator(new JoinLobbyNetworkCommand(), defaultCommandTimeout);

            return command.ExecuteAsync();
        }

        /// <see cref="INetworkManager.LeaveLobbyAsync"/>
        public UniTask<bool> LeaveLobbyAsync()
        {
            var command = new NetworkCommandTimeoutDecorator(new LeaveLobbyNetworkCommand(), defaultCommandTimeout);

            return command.ExecuteAsync();
        }

        /// <see cref="INetworkManager.JoinRoomAsync"/>
        public UniTask<bool> JoinRoomAsync(string roomName)
        { 
            // We need to send events while in a room, so bring up the handler
            customEventHandler.Enable();

            var command = new NetworkCommandTimeoutDecorator(new JoinRoomNetworkCommand(roomName), defaultCommandTimeout);

            return command.ExecuteAsync();
        }

        public UniTask<bool> RejoinRoomAsync(string roomName)
        {
            // We need to send events while in a room, so bring up the handler
            customEventHandler.Enable();

            var command = new NetworkCommandTimeoutDecorator(new RejoinRoomNetworkCommand(roomName), defaultCommandTimeout);

            return command.ExecuteAsync();
        }

        /// <see cref="INetworkManager.CreateRoomAsync"/>
        public UniTask<bool> CreateRoomAsync(string roomName, string ownerId)
        {
            // We need to send events while in a room, so bring up the handler
            customEventHandler.Enable();

            _roomOptions.CustomRoomProperties.SetOwnerId(ownerId.ToString());
            _roomOptions.CustomRoomProperties.SetHostId(ownerId.ToString());
            _roomOptions.CustomRoomProperties.SetEphemeralDataId(Guid.NewGuid());
            
            var command = new NetworkCommandTimeoutDecorator(
                new CreateRoomNetworkCommand(roomName.ToString(), _roomOptions), defaultCommandTimeout);

            return command.ExecuteAsync();
        }

        /// <see cref="INetworkManager.JoinOrCreateRoomAsync"/>
        public UniTask<bool> JoinOrCreateRoomAsync(string roomName, string ownerId)
        {
            // We need to send events while in a room, so bring up the handler
            customEventHandler.Enable();

            if (_roomOptions.CustomRoomProperties.GetOwnerId() == Guid.Empty)
            {
                _roomOptions.CustomRoomProperties.SetOwnerId(ownerId);
            }

            if (_roomOptions.CustomRoomProperties.GetHostId() == Guid.Empty)
            {
                _roomOptions.CustomRoomProperties.SetHostId(ownerId);
            }

            if (_roomOptions.CustomRoomProperties.GetEphemeralDataId() == Guid.Empty)
            {
                _roomOptions.CustomRoomProperties.SetEphemeralDataId(Guid.NewGuid());
            }

            var command = new NetworkCommandTimeoutDecorator(
                new JoinOrCreateRoomNetworkCommand(roomName.ToString(), _roomOptions), defaultCommandTimeout);

            return command.ExecuteAsync();
        }

        /// <see cref="INetworkManager.LeaveRoomAsync"/>
        public UniTask<bool> LeaveRoomAsync()
        {
            // We only need to send events while in a room, so bring down the handler
            customEventHandler.Disable();

            // Clear out the room options for this room
            _roomOptions.CustomRoomProperties = new Hashtable();

            var command = new NetworkCommandTimeoutDecorator(new LeaveRoomNetworkCommand(), defaultCommandTimeout);
            return command.ExecuteAsync();
        }

        /// <see cref="INetworkManager.CloseRoomAsync"/>
        public UniTask<bool> CloseRoomAsync()
        {
            // We only need to send events while in a room, so bring down the handler
            customEventHandler.Disable();

            // Clear out the room options for this room
            _roomOptions.CustomRoomProperties = new Hashtable();

            var command = new NetworkCommandTimeoutDecorator(new CloseRoomNetworkCommand(), defaultCommandTimeout);
            return command.ExecuteAsync();
        }

        /// <see cref="INetworkManager.Subscribe{TEvent}"/>
        public bool Subscribe<TEvent>(Action<TEvent> eventHandler) where TEvent : IGigEvent<NetworkManager>
        {
            return eventBus.Subscribe(eventHandler);
        }

        /// <see cref="INetworkManager.Unsubscribe{TEvent}"/>
        public bool Unsubscribe<TEvent>(Action<TEvent> eventHandler) where TEvent : IGigEvent<NetworkManager>
        {
            return eventBus.Unsubscribe(eventHandler);
        }

        /// <see cref="INetworkManager.RegisterNetworkEvent{TNetworkEvent,TNetworkEventSerializer}"/>
        public bool RegisterNetworkEvent<TNetworkEvent, TNetworkEventSerializer>(byte eventCode)
            where TNetworkEvent : ICustomNetworkEvent
            where TNetworkEventSerializer : ICustomNetworkEventSerializer<TNetworkEvent>
        {
            return customEventHandler.RegisterNetworkEvent<TNetworkEvent, TNetworkEventSerializer>(eventCode);
        }

        /// <see cref="INetworkManager.RaiseNetworkEvent{TNetworkEvent}"/>
        public bool RaiseNetworkEvent<TNetworkEvent>(TNetworkEvent @event) where TNetworkEvent : ICustomNetworkEvent
        {
            return customEventHandler.RaiseNetworkEvent(@event);
        }

        public void SetMasterClient(Player newMasterClient)
        {
            _roomOptions.CustomRoomProperties.SetHostId(newMasterClient.UserId);

            CurrentRoom.SetCustomProperties(_roomOptions.CustomRoomProperties);

            PhotonNetwork.SetMasterClient(newMasterClient);
        }

        public void AddPathwayToRoom(string pathway)
        {
            _roomOptions.CustomRoomProperties.SetScenarioPathway(pathway);

            CurrentRoom.SetCustomProperties(_roomOptions.CustomRoomProperties);
        }

        public void AddPlayModeToRoom(Scenarios.ScenarioControlTypes scenarioPlayMode)
        {
            _roomOptions.CustomRoomProperties.SetPlayMode(scenarioPlayMode);

            CurrentRoom.SetCustomProperties(_roomOptions.CustomRoomProperties);
        }

        public void OwnAllNetworkObjects(IReadOnlyDictionary<Guid, InstantiatedAsset> instantiatedAssets)
        {
            foreach(var currentAsset in instantiatedAssets)
            {
                var photonView = currentAsset.Value.GameObject.GetComponent<PhotonView>();

                OwnNetworkObject(currentAsset.Key, photonView, false);
            }

            CurrentRoom.SetCustomProperties(_roomOptions.CustomRoomProperties);
        }

        public void OwnNetworkObject(Guid assetId, PhotonView photonView, bool setCustomProperty = true)
        {
            // Check to see if their is an allocated photon ID
            if (CurrentRoom.CustomProperties.GetPhotonViewIdFromAssetId(assetId.ToString()) != -1)
            {
                MapNetworkObject(assetId, photonView, LocalPlayer);

                photonView.RequestOwnership();
            }
            else if (PhotonNetwork.AllocateViewID(photonView))
            {
                // Save the ID to the PhotonRoom's hashtable so that new users will be able to match our AssetTypeIds to the PhotonView Ids
                _roomOptions.CustomRoomProperties.MapGigAssetIdToPhotonViewId(assetId.ToString(), photonView.ViewID);

                if(setCustomProperty)
                {
                    CurrentRoom.SetCustomProperties(_roomOptions.CustomRoomProperties);
                }

                photonView.RequestOwnership();
            }
        }

        /// <summary>
        /// Get IDs from the photon room's hashtable, to assign photon IDs to assets.
        /// </summary>
        /// <param name="instantiatedAssets"></param>
        public void MapNetworkObjects(IReadOnlyDictionary<Guid, InstantiatedAsset> instantiatedAssets, Player owner)
        {
            foreach (var currentAsset in instantiatedAssets)
            {
                var photonView = currentAsset.Value.GameObject.GetComponent<PhotonView>();

                MapNetworkObject(currentAsset.Key, photonView, owner);
            }
        }

        /// <summary>
        /// Assign a photon ID to the PhotonView based on its assetId using the photon room's hashtable.
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="photonView"></param>
        public void MapNetworkObject(Guid assetId, PhotonView photonView, Player owner)
        {
            var viewId = CurrentRoom.CustomProperties.GetPhotonViewIdFromAssetId(assetId.ToString());

            if (photonView.ViewID != viewId)
                photonView.ViewID = viewId;
        }

        // --- Private Methods:

        private void OnPingUpdate(int pingValue, PingStatus pingStatus)
        {
            eventBus.Publish(new PingValueUpdatedNetworkEvent(pingValue, pingStatus));
        }

        private async void OnFinishedLogOutEvent(FinishedLogOutEvent @event)
        {
            // Remove yourself from Photon
            await DisconnectAsync();
        }

        public virtual int GetAlternativePlayerReference(Guid playerId)
        {
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (Guid.Parse(player.UserId) == playerId)
                {
                    return player.ActorNumber;
                }
            }

            return -1;
        }

        public virtual Player GetPlayerById(Guid playerId)
        {
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (Guid.Parse(player.UserId) == playerId)
                {
                    return player;
                }
            }

            return null;
        }
    }
}