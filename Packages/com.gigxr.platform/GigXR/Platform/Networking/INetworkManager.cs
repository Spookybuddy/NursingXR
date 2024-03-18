using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GIGXR.Platform.Core.EventBus;
using GIGXR.Platform.Networking.EventBus;
using GIGXR.Platform.Scenarios.GigAssets;
using JetBrains.Annotations;
using Photon.Realtime;
using Photon.Pun;

namespace GIGXR.Platform.Networking
{
    /// <summary>
    /// This represents the main interface that should be used for other classes to communicate over the network.
    /// </summary>
    public interface INetworkManager
    {
        /// <summary>
        /// Returns whether the user is currently in a room.
        /// </summary>
        bool InRoom { get; }

        /// <summary>
        /// Returns whether the user is currently connected.
        /// </summary>
        bool IsConnected {  get; }

        /// <summary>
        /// Returns the current room the user is in, if applicable. Can be null.
        /// </summary>
        [CanBeNull]
        Room CurrentRoom { get; }

        /// <summary>
        /// Returns whether the current user is the master client of the current room.
        /// </summary>
        bool IsMasterClient { get; }

        /// <summary>
        /// Returns  the ID of the user acting as the master client of the current room.
        /// </summary>
        Guid MasterClientId { get; }

        /// <summary>
        /// Returns the time of the server in milliseconds.
        /// </summary>
        int ServerTime { get; }

        Player LocalPlayer { get; }

        Player[] AllPlayers { get; }

        /// <summary>
        /// Sets the user that will be represented on the network. This should be called before connecting.
        /// </summary>
        /// <param name="userId">The userId of the user, should correspond to their GMS accountId.</param>
        /// <param name="nickName">The nickname of the user.</param>
        void SetUser(string userId, string nickName);

        /// <summary>
        /// Connects to the Photon network.
        /// </summary>
        /// <remarks>
        /// The implementation should attempt reconnection automatically through intermittent network interruptions.
        /// </remarks>
        /// <returns>Whether the connection was successful.</returns>
        UniTask<bool> ConnectAsync();

        /// <summary>
        /// Disconnects from the Photon network.
        /// </summary>
        /// <returns>Whether the disconnection was successful.</returns>
        UniTask<bool> DisconnectAsync();

        /// <summary>
        /// Joins the default Photon lobby.
        /// </summary>
        /// <returns>True if the lobby was joined successfully</returns>
        UniTask<bool> JoinLobbyAsync();

        /// <summary>
        /// Leaves the Photon lobby.
        /// </summary>
        /// <returns></returns>
        UniTask<bool> LeaveLobbyAsync();

        /// <summary>
        /// Joins a Photon room by the given name.
        /// </summary>
        /// <param name="roomName">The room name to join, should correspond to a GMS sessionId.</param>
        /// <returns>Whether the join was successful.</returns>
        UniTask<bool> JoinRoomAsync(string roomName);

        /// <summary>
        /// Rejoins a Photon room by the given name. Required if a user has been in a room before.
        /// </summary>
        /// <param name="roomName">The room name to join, should correspond to a GMS sessionId.</param>
        /// <returns>Whether the join was successful.</returns>
        UniTask<bool> RejoinRoomAsync(string roomName);

        /// <summary>
        /// Creates a Photon room with an owner.
        /// </summary>
        /// <param name="roomName">The room name to join, should correspond to a GMS sessionId.</param>
        /// <param name="ownerId">The ownerId of the room, should correspond to a GMS accountId.</param>
        /// <returns>Whether the room was created.</returns>
        UniTask<bool> CreateRoomAsync(string roomName, string ownerId);

        /// <summary>
        /// Joins or creates a Photon room.
        /// </summary>
        /// <param name="roomName">The room name to join, should correspond to a GMS sessionId.</param>
        /// <param name="ownerId">The ownerId of the room, should correspond to a GMS accountId.</param>
        /// <returns>Returns true if a room was able to be joined.</returns>
        UniTask<bool> JoinOrCreateRoomAsync(string roomName, string ownerId);

        /// <summary>
        /// Leaves a Photon room.
        /// </summary>
        /// <returns>Whether the leave room operation was successful.</returns>
        UniTask<bool> LeaveRoomAsync();

        /// <summary>
        /// Closes a Photon room.
        /// </summary>
        /// <returns>Whether the close room operation was successful.</returns>
        UniTask<bool> CloseRoomAsync();

        /// <summary>
        /// Subscribes to an event.
        /// </summary>
        /// <param name="eventHandler">The handler that will be invoked when the event is published.</param>
        /// <typeparam name="TEvent">The type of event to listen for.</typeparam>
        /// <returns>Whether the subscription was successful.</returns>
        bool Subscribe<TEvent>(Action<TEvent> eventHandler) where TEvent : IGigEvent<NetworkManager>;

        /// <summary>
        /// Unsubscribes to an event.
        /// </summary>
        /// <param name="eventHandler">The handler that will be removed.</param>
        /// <typeparam name="TEvent">The type of event to remove the listener for.</typeparam>
        /// <returns>Whether the unsubscription was successful.</returns>
        bool Unsubscribe<TEvent>(Action<TEvent> eventHandler) where TEvent : IGigEvent<NetworkManager>;

        /// <summary>
        /// Registers a custom network event. A custom network event can be used to send and receive application-
        /// specific events that may optionally contain payloads.
        ///
        /// A custom event must be registered with an associated serializer to define how the custom payload should be
        /// serialized and deserialized across the network.
        /// </summary>
        /// <param name="eventCode">A unique event code between 1-199 to identify this event.</param>
        /// <typeparam name="TNetworkEvent">The type of event to register.</typeparam>
        /// <typeparam name="TNetworkEventSerializer">The serializer for the event.</typeparam>
        /// <returns>Whether the registration was successful.</returns>
        bool RegisterNetworkEvent<TNetworkEvent, TNetworkEventSerializer>(byte eventCode)
            where TNetworkEvent : ICustomNetworkEvent
            where TNetworkEventSerializer : ICustomNetworkEventSerializer<TNetworkEvent>;

        /// <summary>
        /// Raises a custom network event to be sent to other users in the same room.
        /// </summary>
        /// <param name="event">The custom event to send.</param>
        /// <typeparam name="TNetworkEvent">The type of event being sent.</typeparam>
        /// <returns>Whether the event was sent.</returns>
        bool RaiseNetworkEvent<TNetworkEvent>(TNetworkEvent @event) where TNetworkEvent : ICustomNetworkEvent;

        void SetMasterClient(Player newMasterClient);

        /// <summary>
        /// Provides a different reference that might be used to refer to the player over the network (i.e. ActorNumber in Photon)
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        int GetAlternativePlayerReference(Guid playerId);

        /// <summary>
        /// Adds a pathway to the Room's custom lobby to handle host transfer and drops.
        /// TODO Refactor this
        /// </summary>
        /// <param name="pathway"></param>
        void AddPathwayToRoom(string pathway);

        /// <summary>
        /// Adds the play mode to the Room's custom lobby to handle host transfer and drops.
        /// </summary>
        /// <param name="scenarioPlayMode"></param>
        void AddPlayModeToRoom(Scenarios.ScenarioControlTypes scenarioPlayMode);

        /// <summary>
        /// Set the local player as the owner for an instantiated asset(s).
        /// This is for initialization only; ownership transfers should be handled
        /// through the NetworkAuthority and PhotonView.RequestOwnership.
        /// </summary>
        /// <param name="instantiatedAssets"></param>
        void OwnAllNetworkObjects(IReadOnlyDictionary<Guid, InstantiatedAsset> instantiatedAssets);

        /// <summary>
        /// Allows the player with the given PhotonView to own the given Asset
        /// </summary>
        /// <param name="assetId">The ID of the asset to be owned</param>
        /// <param name="photonView">The PhotonView of the Asset</param>
        /// <param name="setCustomProperty">If false, will not call the PhotonNetwork SetCustomProperty call, the
        /// developer must do it themselves. Useful when trying to own multiple items at once.</param>
        void OwnNetworkObject(Guid assetId, PhotonView photonView, bool setCustomProperty = true);

        void MapNetworkObjects(IReadOnlyDictionary<Guid, InstantiatedAsset> instantiatedAssets, Player owner);

        void MapNetworkObject(Guid assetId, PhotonView photonView, Player owner);

        /// <summary>
        /// Returns the Network related class that is needed to switch Master Clients. In Photon's case, this is the Player class.
        /// </summary>
        /// <param name="playerId">The GUID of the player you want to find</param>
        /// <returns>The Player class with the Matching ID or null if they do not exist in the room</returns>
        Player GetPlayerById(Guid playerId);

        /// <summary>
        /// Allows the NetworkManager to map property strings to values on the local player
        /// </summary>
        /// <param name="id">The string name of the player property</param>
        /// <param name="property">The property object itself</param>
        public void AddPropertyToLocalPlayer(string id, object property);

        /// <summary>
        /// Retrieves the property value associated with the propertyId for the local player
        /// </summary>
        /// <typeparam name="T">The type the value was stored as</typeparam>
        /// <param name="propertyId">The string name of the player property</param>
        /// <returns></returns>
        public T GetPlayerProperty<T>(Player player, string propertyId);

        /// <summary>
        /// Retrieves the property value associated with the given propertyId
        /// </summary>
        /// <typeparam name="T">The type the value was stored as</typeparam>
        /// <param name="userId">The ID of the user</param>
        /// <param name="propertyId">The string name of the player property</param>
        /// <returns></returns>
        public T GetPlayerPropertyForUser<T>(string userId, string propertyId);
    }
}