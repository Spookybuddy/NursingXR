using Cysharp.Threading.Tasks;
using GIGXR.Platform.Core;
using GIGXR.Platform.Core.EventBus;
using GIGXR.Platform.Networking;
using GIGXR.Platform.Networking.Commands;
using GIGXR.Platform.Networking.EventBus;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.GigAssets;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfflineNetworkManagerInjector : CoreInjectorComponent<INetworkManager>
{
    public override INetworkManager GetSingleton()
    {
        var eventBus = new GigEventBus<NetworkManager>();

        return new OfflineNetworkManager(eventBus);
    }
}

public class OfflineNetworkManager : INetworkManager
{
    public bool InRoom => true;

    public bool IsConnected => true;

    public Photon.Realtime.Room CurrentRoom => PhotonNetwork.CurrentRoom;

    public bool IsMasterClient => true;

    public Guid MasterClientId => Guid.Parse(PhotonNetwork.MasterClient.UserId);

    public int ServerTime => Time.frameCount;

    public Photon.Realtime.Player LocalPlayer => PhotonNetwork.LocalPlayer;

    public Photon.Realtime.Player[] AllPlayers => new Photon.Realtime.Player[1] { LocalPlayer };

    private readonly IGigEventBus<NetworkManager> eventBus;

    private readonly TimeSpan defaultCommandTimeout = TimeSpan.FromSeconds(15);

    public OfflineNetworkManager(IGigEventBus<NetworkManager> eventBus)
    {
        this.eventBus = eventBus;

        Debug.Log("Network is set to off-line mode.");
    }

    public void AddPathwayToRoom(string pathway)
    {

    }

    public void AddPlayModeToRoom(ScenarioControlTypes scenarioPlayMode)
    {

    }

    public void AddPropertyToLocalPlayer(string id, object property)
    {

    }

    public UniTask<bool> CloseRoomAsync()
    {
        return UniTask.FromResult(true);
    }

    public UniTask<bool> ConnectAsync()
    {
        return UniTask.FromResult(true);
    }

    public UniTask<bool> CreateRoomAsync(string roomName, string ownerId)
    {
        return UniTask.FromResult(true);
    }

    public UniTask<bool> DisconnectAsync()
    {
        return UniTask.FromResult(true);
    }

    public int GetAlternativePlayerReference(Guid playerId)
    {
        return PhotonNetwork.LocalPlayer.ActorNumber;
    }

    public Photon.Realtime.Player GetPlayerById(Guid playerId)
    {
        return PhotonNetwork.LocalPlayer;
    }

    public T GetPlayerProperty<T>(Photon.Realtime.Player player, string propertyId)
    {
        throw new NotImplementedException();
    }

    public T GetPlayerPropertyForUser<T>(string userId, string propertyId)
    {
        throw new NotImplementedException();
    }

    public UniTask<bool> JoinLobbyAsync()
    {
        return UniTask.FromResult(true);
    }

    public UniTask<bool> JoinOrCreateRoomAsync(string roomName, string ownerId)
    {
        return UniTask.FromResult(true);
    }

    public UniTask<bool> JoinRoomAsync(string roomName)
    {
        return UniTask.FromResult(true);
    }

    public UniTask<bool> LeaveLobbyAsync()
    {
        return UniTask.FromResult(true);
    }

    public UniTask<bool> LeaveRoomAsync()
    {
        var command = new NetworkCommandTimeoutDecorator(new LeaveRoomNetworkCommand(), defaultCommandTimeout);
        return command.ExecuteAsync();
    }

    public void MapNetworkObject(Guid assetId, Photon.Pun.PhotonView photonView, Photon.Realtime.Player owner)
    {

    }

    public void MapNetworkObjects(IReadOnlyDictionary<Guid, InstantiatedAsset> instantiatedAssets, Photon.Realtime.Player owner)
    {

    }

    public void OwnAllNetworkObjects(IReadOnlyDictionary<Guid, InstantiatedAsset> instantiatedAssets)
    {

    }

    public void OwnNetworkObject(Guid assetId, Photon.Pun.PhotonView photonView, bool setCustomProperty = true)
    {

    }

    public bool RaiseNetworkEvent<TNetworkEvent>(TNetworkEvent @event) where TNetworkEvent : ICustomNetworkEvent
    {
        return true;
    }

    public bool RegisterNetworkEvent<TNetworkEvent, TNetworkEventSerializer>(byte eventCode)
        where TNetworkEvent : ICustomNetworkEvent
        where TNetworkEventSerializer : ICustomNetworkEventSerializer<TNetworkEvent>
    {
        return true;
    }

    public UniTask<bool> RejoinRoomAsync(string roomName)
    {
        return UniTask.FromResult(true);
    }

    public void SetMasterClient(Photon.Realtime.Player newMasterClient)
    {
    }

    public void SetUser(string userId, string nickName)
    {
        PhotonNetwork.AuthValues = new AuthenticationValues(userId);
        LocalPlayer.NickName = nickName;
    }

    public bool Subscribe<TEvent>(Action<TEvent> eventHandler) where TEvent : IGigEvent<NetworkManager>
    {
        return eventBus.Subscribe(eventHandler);
    }

    public bool Unsubscribe<TEvent>(Action<TEvent> eventHandler) where TEvent : IGigEvent<NetworkManager>
    {
        return eventBus.Unsubscribe(eventHandler);
    }
}
