using Cysharp.Threading.Tasks;
using GIGXR.GMS.Clients;
using GIGXR.Platform.AppEvents;
using GIGXR.Platform.AppEvents.Events.Calibration;
using GIGXR.Platform.AppEvents.Events.Scenarios;
using GIGXR.Platform.AppEvents.Events.UI;
using GIGXR.Platform.Interfaces;
using GIGXR.Platform.Networking;
using GIGXR.Platform.Networking.EventBus.Events;
using GIGXR.Platform.Networking.EventBus.Events.InRoom;
using GIGXR.Platform.Networking.EventBus.Events.Scenarios;
using GIGXR.Platform.Networking.EventBus.Events.Sessions;
using GIGXR.Platform.Networking.EventBus.Events.Stages;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.Data;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Scenarios.GigAssets.Data;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using GIGXR.Platform.Utilities;
using GIGXR.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.Sessions
{
    /// <summary>
    /// The set of actions and responsibilities of a client user. A client is someone who is not hosting the 
    /// session and is experiencing the scenario.
    /// </summary>
    public class ClientCapabilities : ISessionCapability, IDisposable
    {
        // Holds an asset ID and property name to map when a specific value is sent over the network so it does not forward networked changes back to the host
        private Dictionary<(Guid, string), byte[]> clientNetworkedChangedPropertyFlag;

        // Like above, but holds the property changes that the client makes so when the host updates the value, it can be ignored
        private Dictionary<(Guid, string), byte[]> clientRequests;

        private ResyncPhysicsBackgroundHandler resyncClientPhysics;

        private bool isReadyInScenario = false;

        #region Dependencies

        private ISessionManager SessionManager { get; }

        private IScenarioManager ScenarioManager { get; }

        private INetworkManager NetworkManager { get; }

        private GmsApiClient ApiClient { get; }

        private AppEventBus EventBus { get; }

        private ICalibrationManager CalibrationManager { get; }

        #endregion

        public ClientCapabilities(ISessionManager sessionManager, IScenarioManager scenarioManager, INetworkManager networkManager, ICalibrationManager calibrationManager, GmsApiClient gmsApiClient, AppEventBus appEvent)
        {
            SessionManager = sessionManager;
            ScenarioManager = scenarioManager;
            NetworkManager = networkManager;
            CalibrationManager = calibrationManager;
            ApiClient = gmsApiClient;
            EventBus = appEvent;

            clientNetworkedChangedPropertyFlag = new Dictionary<(Guid, string), byte[]>();
            clientRequests = new Dictionary<(Guid, string), byte[]>();
        }

        #region ISessionCapabilityImplementation

        public void Activate()
        {
            // The client does not run physics as transforms are synced by the Host
            Physics.autoSimulation = false;

            resyncClientPhysics = new ResyncPhysicsBackgroundHandler();
            resyncClientPhysics.Enable();

            // Start listening to all session, network, etc related events
            SubscribeToEvents();
        }

        public void Deactivate()
        {
            // Stop listening to all events
            UnsubscribeToEvents();

            if (resyncClientPhysics != null)
            {
                resyncClientPhysics.Disable();

                resyncClientPhysics = null;
            }
        }

        #endregion

        #region HostManagement

        private void OnRequestUserToHostNetworkEvent(RequestUserToHostNetworkEvent @event)
        {
            EventBus.Publish
                (
                    new ClientStartNewHostEvent
                        (
                            () => NetworkManager.RaiseNetworkEvent
                                (
                                    new AcceptHostRequestNetworkEvent
                                        (
                                            NetworkManager.LocalPlayer.NickName,
                                            ApiClient.AccountsApi.AuthenticatedAccount.AccountId
                                        )
                                ),
                            () => NetworkManager.RaiseNetworkEvent
                                (
                                    new RejectHostRequestNetworkEvent
                                        (NetworkManager.LocalPlayer.NickName)
                                ),
                            @event.HostName
                        )
                );
        }

        private void OnCancelRequestUserToHostNetworkEvent(CancelRequestUserToHostNetworkEvent @event)
        {
            EventBus.Publish(new ClientHostCancelledTransferEvent(@event.HostName));
        }

        #endregion

        #region Asset Local EventHandlers

        private void AssetManager_AssetPropertyUpdated(object sender, AssetPropertyChangeEventArgs e)
        {
            // Only try to forward asset properties if the NetworkManager reports in a network room
            // When the client is joining a room, they will update their values, but don't allow those to be requested
            if (NetworkManager.InRoom &&  
                isReadyInScenario && 
                e.Origin != AssetPropertyChangeOrigin.StageChange &&
                !ScenarioManager.IsSwitchingStatus)
            {
                var assetMediator = ScenarioManager.AssetManager.GetById(e.AssetId)?.GetComponent<IAssetMediator>();

                // Construct a tuple based on the asset+property change
                var assetPropertyChange = (e.AssetId, e.AssetPropertyName);

                // The ScenarioManager.AssetManager.GetById will fail in the first frame when an object is instantiated, but this isn't an issue here
                if (assetMediator != null)
                {
                    // Only raise network request events for your own local changes, not networked ones
                    if (!clientNetworkedChangedPropertyFlag.ContainsKey(assetPropertyChange))
                    {
                        // The user has made another request while the last one hasn't been processed, update the values now
                        if (clientRequests.ContainsKey(assetPropertyChange))
                        {
                            clientRequests[(e.AssetId, e.AssetPropertyName)]
                                = assetMediator.GetAssetPropertyByteArray(e.AssetPropertyName);
                        }
                        else
                        {
                            clientRequests.Add
                                (
                                    (e.AssetId, e.AssetPropertyName),
                                    assetMediator.GetAssetPropertyByteArray(e.AssetPropertyName)
                                );
                        }

                        NetworkManager.RaiseNetworkEvent
                            (
                                new RequestPropertyUpdateNetworkEvent
                                    (
                                        ApiClient.AccountsApi.AuthenticatedAccount.AccountId,
                                        e.AssetId,
                                        e.AssetPropertyName,
                                        SerializationUtilities
                                            .ObjectToByteArray
                                                (e.AssetPropertyValue)
                                    )
                            );
                    }
                    else
                    {
                        clientNetworkedChangedPropertyFlag.Remove(assetPropertyChange);
                    }
                }
            }
        }

        #endregion

        #region Asset Network EventHandlers

        private void OnAssetPropertyUpdateNetworkEvent(AssetPropertyUpdateNetworkEvent e)
        {
            HandleClientNetworkUpdate
                (
                    e.AssetId,
                    e.PropertyName,
                    e.Value,
                    () => ScenarioManager.AssetManager.UpdateAssetProperty
                        (
                            e.AssetId,
                            e.PropertyName,
                            e.Value
                        )
                );
        }

        private void OnRejectPropertyUpdateNetworkEvent(RejectPropertyUpdateNetworkEvent @event)
        {
            clientRequests.Remove((@event.AssetId, @event.PropertyName));

            // RejectPropertyUpdateNetworkEvent is derived from AssetPropertyUpdateNetworkEvent, so use the same method to update the asset back to the correct value
            OnAssetPropertyUpdateNetworkEvent(@event);
        }

        private void HandleClientNetworkUpdate
        (
            Guid assetId,
            string propertyName,
            byte[] value,
            Action action
        )
        {
            var assetPropertyChangeInfo = (assetId, propertyName);

            // This is a request that the client themselves has made, ignore
            if (clientRequests.ContainsKey(assetPropertyChangeInfo))
            {
                // The user does not need to do anything here, so break out here
                return;
            }

            if (!clientNetworkedChangedPropertyFlag.ContainsKey(assetPropertyChangeInfo))
            {
                // Flag this Asset+Property from having been changed by the network so the next local event is not propagated back to the network as a request
                clientNetworkedChangedPropertyFlag.Add(assetPropertyChangeInfo, value);

                action.Invoke();
            }
            // The latest property change should always be used, if this is called again, update the property value to the latest
            else
            {
                clientNetworkedChangedPropertyFlag.Remove(assetPropertyChangeInfo);

                HandleClientNetworkUpdate
                    (
                        assetId,
                        propertyName,
                        value,
                        action
                    );
            }
        }

        private void OnClientPropertyUpdatedNetworkEvent(ClientPropertyUpdatedNetworkEvent @event)
        {
            clientRequests.Remove((@event.AssetId, @event.PropertyName));
        }

        #endregion

        #region Stage Network EventHandlers

        private void OnStagesSwitchedNetworkEvent(StagesSwitchedNetworkEvent e)
        {
            ScenarioManager.StageManager.SwitchToStage(e.StageID);

            ScenarioManager.TrySyncScenarioAsync(NetworkManager.ServerTime, e.SyncData);
        }

        private void OnStagesSwappedNetworkEvent(StagesSwappedNetworkEvent e)
        {
            ScenarioManager.StageManager.SwapStages(e.Stage1ID, e.Stage2ID);
        }

        private void OnStageDuplicatedNetworkEvent(StageDuplicatedNetworkEvent e)
        {
            ScenarioManager.StageManager.DuplicateStage(e.StageID);
        }

        private void OnStageCreatedNetworkEvent(StageCreatedNetworkEvent e)
        {
            Debug.Log($"Stage {e.StageID} has been created");
            ScenarioManager.StageManager.CreateStageWithID(e.StageID);
        }

        private void OnAllStagesRemovedNetworkEvent(AllStagesRemovedNetworkEvent e)
        {
            ScenarioManager.StageManager.RemoveAllStages();
        }

        private void OnStageRemovedNetworkEvent(StageRemovedNetworkEvent e)
        {
            ScenarioManager.StageManager.RemoveStage(e.StageId);
        }

        private void OnStageRenamedNetworkEvent(StageRenamedNetworkEvent e)
        {
            ScenarioManager.StageManager.RenameStage(e.StageId, e.NewName);
        }

        #endregion

        #region Session Network EventHandlers

        private async void OnUpdateFromGMSNetworkEvent(UpdateFromGMSNetworkEvent @event)
        {
            // Grab the latest data from the ephemeral data store
            var ephemeralData = await ApiClient.SessionsApi.GetEphemeralDataAsync
                (NetworkManager.CurrentRoom.CustomProperties.GetEphemeralDataId());

            try
            {
                if(ephemeralData.EphemeralData == null)
                {
                    Debug.LogWarning($"[ClientCapabilities] There was no ephemeral data to deserialize while syncing with host. Moving on.");

                    await SetupScenarioAfterLoading(@event);

                    return;
                }

                var scenario = ephemeralData.EphemeralData.ToObject(ScenarioManager.ScenarioClassType, DefaultNewtonsoftJsonConfiguration.JsonSerializer);

                await SessionManager.ScenarioManager.AssetManager.ReloadStagesAndAssetsAsync(scenario, @event.StageId)
                    .ContinueWith(async () => 
                    {
                        try
                        {
                            await SetupScenarioAfterLoading(@event);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[ClientCapabilities] There was an exception that occurred while setting up new scenario data and cannot continue: {ex.Message}");

                            await SessionManager.LeaveSessionAsync();
                        }
                    });
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ClientCapabilities] There was an exception that occurred while loading the new scenario data and cannot continue: {ex.Message}");

                await SessionManager.LeaveSessionAsync();

                return;
            }
        }

        private async UniTask SetupScenarioAfterLoading(UpdateFromGMSNetworkEvent @event)
        {
            // We need the networked objects to have the correct photon Ids so that transform data is propagated
            NetworkManager.MapNetworkObjects(ScenarioManager.AssetManager.AllInstantiatedAssets, NetworkManager.GetPlayerById(SessionManager.HostId));

            ScenarioManager.StageManager.SwitchToStage(@event.StageId);

            await ScenarioManager.TrySyncScenarioAsync(NetworkManager.ServerTime, @event.ScenarioSyncData);

            // Send the local event based on the scenario status for the client
            switch (@event.ScenarioSyncData.ScenarioStatus)
            {
                case ScenarioStatus.Unloaded:
                    break;
                case ScenarioStatus.Loading:
                    break;
                case ScenarioStatus.Loaded:
                    break;
                case ScenarioStatus.Playing:
                    EventBus.Publish(new ScenarioPlayingLocalEvent());
                    break;
                case ScenarioStatus.Paused:
                    EventBus.Publish(new ScenarioPausingLocalEvent());
                    break;
                case ScenarioStatus.Stopped:
                    EventBus.Publish(new ScenarioStoppingLocalEvent());
                    break;
                case ScenarioStatus.Unloading:
                    break;
                default:
                    break;
            }

            // Send an event to the host to let them know you are now able to sync objects since you will always
            // miss the first sync while loading the scenario
            NetworkManager.RaiseNetworkEvent(new ClientReadyNetworkEvent());

            EventBus.Publish(new FinishingSyncWithHostLocalEvent());

            isReadyInScenario = true;
        }

        #endregion

        private void OnPlayerEnteredRoomNetworkEvent(PlayerEnteredRoomNetworkEvent @event)
        {
            // The only reason a client needs to worry about when a user joins the room is if it's the case where they are the 
            // Master Client because the host has disconnected and they must give it back
            if(NetworkManager.IsMasterClient && Guid.Parse(@event.Player.UserId) == SessionManager.HostId)
            {
                // Technically we only need to set the MasterClient on the Photon side as the room is aware of who the host is
                // but we don't want to introduce a pure Photon call, so just redo it all here
                NetworkManager.SetMasterClient(@event.Player);
            }
        }

        private async void OnHostClosedSessionNetworkEvent(HostClosedSessionNetworkEvent @event)
        {
            await SessionManager.LeaveSessionAsync();
        }

        private void OnContentMarkerUpdateNetworkEvent(ContentMarkerUpdateNetworkEvent @event)
        {
            if(CalibrationManager.CurrentContentMarkerControlMode == ContentMarkerControlMode.Host)
            {
                EventBus.Publish(new SetContentMarkerEvent(@event.NewPosition,
                                                           @event.NewRotation));
            }
        }

        #region Scenario Network EventHandlers

        private void OnScenarioPlayingNetworkEvent(ScenarioPlayingNetworkEvent @event)
        {
            EventBus.Publish(new ScenarioPlayingLocalEvent(@event.SyncData));
        }

        private void OnScenarioPausedNetworkEvent(ScenarioPausedNetworkEvent @event)
        {
            EventBus.Publish(new ScenarioPausingLocalEvent(@event.SyncData));
        }

        private void OnScenarioStoppedNetworkEvent(ScenarioStoppedNetworkEvent @event)
        {
            EventBus.Publish(new ScenarioStoppingLocalEvent(@event.SyncData));
        }

        #endregion

        #region Scenario Local EventHandlers

        private void OnScenarioPlayingLocalEvent(ScenarioPlayingLocalEvent @event)
        {
            SessionManager.LeaveWaitingRoomLobby();

            if (@event.SyncData.HasValue)
            {
                ScenarioManager.TrySyncScenarioAsync(NetworkManager.ServerTime, @event.SyncData.Value);
            }
            else
            {
                ScenarioManager.PlayScenarioAsync();
            }
        }

        private void OnScenarioStoppingLocalEvent(ScenarioStoppingLocalEvent @event)
        {
            SessionManager.LeaveWaitingRoomLobby();

            if (@event.SyncData.HasValue)
            {
                ScenarioManager.TrySyncScenarioAsync(NetworkManager.ServerTime, @event.SyncData.Value);
            }
            else
            {
                ScenarioManager.StopScenarioAsync();
            }
        }

        private void OnScenarioPausingLocalEvent(ScenarioPausingLocalEvent @event)
        {
            SessionManager.LeaveWaitingRoomLobby();

            if (@event.SyncData.HasValue)
            {
                ScenarioManager.TrySyncScenarioAsync(NetworkManager.ServerTime, @event.SyncData.Value);
            }
            else
            {
                ScenarioManager.PauseScenarioAsync();
            }
        }

        #endregion

        private void SubscribeToEvents()
        {
            // Local Scenario Events

            EventBus.Subscribe<ScenarioPlayingLocalEvent>(OnScenarioPlayingLocalEvent);
            EventBus.Subscribe<ScenarioStoppingLocalEvent>(OnScenarioStoppingLocalEvent);
            EventBus.Subscribe<ScenarioPausingLocalEvent>(OnScenarioPausingLocalEvent);

            // Session Management

            NetworkManager.Subscribe<UpdateFromGMSNetworkEvent>(OnUpdateFromGMSNetworkEvent);
            NetworkManager.Subscribe<PlayerEnteredRoomNetworkEvent>(OnPlayerEnteredRoomNetworkEvent);
            NetworkManager.Subscribe<HostClosedSessionNetworkEvent>(OnHostClosedSessionNetworkEvent);
            NetworkManager.Subscribe<ContentMarkerUpdateNetworkEvent>(OnContentMarkerUpdateNetworkEvent);

            // Scenario Management

            NetworkManager.Subscribe<ScenarioPlayingNetworkEvent>(OnScenarioPlayingNetworkEvent);
            NetworkManager.Subscribe<ScenarioPausedNetworkEvent>(OnScenarioPausedNetworkEvent);
            NetworkManager.Subscribe<ScenarioStoppedNetworkEvent>(OnScenarioStoppedNetworkEvent);

            // Stage Management

            NetworkManager.Subscribe<AllStagesRemovedNetworkEvent>(OnAllStagesRemovedNetworkEvent);
            NetworkManager.Subscribe<StageCreatedNetworkEvent>(OnStageCreatedNetworkEvent);
            NetworkManager.Subscribe<StageDuplicatedNetworkEvent>(OnStageDuplicatedNetworkEvent);
            NetworkManager.Subscribe<StageRenamedNetworkEvent>(OnStageRenamedNetworkEvent);
            NetworkManager.Subscribe<StageRemovedNetworkEvent>(OnStageRemovedNetworkEvent);
            NetworkManager.Subscribe<StagesSwappedNetworkEvent>(OnStagesSwappedNetworkEvent);
            NetworkManager.Subscribe<StagesSwitchedNetworkEvent>(OnStagesSwitchedNetworkEvent);

            // Networked Host Management

            NetworkManager.Subscribe<RequestUserToHostNetworkEvent>
                (OnRequestUserToHostNetworkEvent);

            NetworkManager.Subscribe<CancelRequestUserToHostNetworkEvent>
                (OnCancelRequestUserToHostNetworkEvent);

            // Local Asset Management
            ScenarioManager.AssetManager.AssetPropertyUpdated += AssetManager_AssetPropertyUpdated;

            // Networked Asset Management

            NetworkManager.Subscribe<AssetPropertyUpdateNetworkEvent>
                (OnAssetPropertyUpdateNetworkEvent);

            NetworkManager.Subscribe<RejectPropertyUpdateNetworkEvent>
                (OnRejectPropertyUpdateNetworkEvent);

            NetworkManager.Subscribe<ClientPropertyUpdatedNetworkEvent>
                (OnClientPropertyUpdatedNetworkEvent);
        }

        private void UnsubscribeToEvents()
        {
            // Local

            ScenarioManager.AssetManager.AssetPropertyUpdated -= AssetManager_AssetPropertyUpdated;

            EventBus.Unsubscribe<ScenarioPlayingLocalEvent>(OnScenarioPlayingLocalEvent);
            EventBus.Unsubscribe<ScenarioStoppingLocalEvent>(OnScenarioStoppingLocalEvent);
            EventBus.Unsubscribe<ScenarioPausingLocalEvent>(OnScenarioPausingLocalEvent);

            // Network

            NetworkManager.Unsubscribe<UpdateFromGMSNetworkEvent>(OnUpdateFromGMSNetworkEvent);
            NetworkManager.Unsubscribe<PlayerEnteredRoomNetworkEvent>(OnPlayerEnteredRoomNetworkEvent);
            NetworkManager.Unsubscribe<HostClosedSessionNetworkEvent>(OnHostClosedSessionNetworkEvent);
            NetworkManager.Unsubscribe<ContentMarkerUpdateNetworkEvent>(OnContentMarkerUpdateNetworkEvent);

            NetworkManager.Unsubscribe<ScenarioPlayingNetworkEvent>(OnScenarioPlayingNetworkEvent);
            NetworkManager.Unsubscribe<ScenarioPausedNetworkEvent>(OnScenarioPausedNetworkEvent);
            NetworkManager.Unsubscribe<ScenarioStoppedNetworkEvent>(OnScenarioStoppedNetworkEvent);

            NetworkManager.Unsubscribe<AssetPropertyUpdateNetworkEvent>(OnAssetPropertyUpdateNetworkEvent);
            NetworkManager.Unsubscribe<RequestUserToHostNetworkEvent>(OnRequestUserToHostNetworkEvent);
            NetworkManager.Unsubscribe<CancelRequestUserToHostNetworkEvent>(OnCancelRequestUserToHostNetworkEvent);

            // Stage Management

            NetworkManager.Unsubscribe<AllStagesRemovedNetworkEvent>(OnAllStagesRemovedNetworkEvent);
            NetworkManager.Unsubscribe<StageCreatedNetworkEvent>(OnStageCreatedNetworkEvent);
            NetworkManager.Unsubscribe<StageDuplicatedNetworkEvent>(OnStageDuplicatedNetworkEvent);
            NetworkManager.Unsubscribe<StageRenamedNetworkEvent>(OnStageRenamedNetworkEvent);
            NetworkManager.Unsubscribe<StageRemovedNetworkEvent>(OnStageRemovedNetworkEvent);
            NetworkManager.Unsubscribe<StagesSwappedNetworkEvent>(OnStagesSwappedNetworkEvent);
            NetworkManager.Unsubscribe<StagesSwitchedNetworkEvent>(OnStagesSwitchedNetworkEvent);

            // Networked Asset Management

            NetworkManager.Unsubscribe<RejectPropertyUpdateNetworkEvent>(OnRejectPropertyUpdateNetworkEvent);
            NetworkManager.Unsubscribe<ClientPropertyUpdatedNetworkEvent>(OnClientPropertyUpdatedNetworkEvent);
        }

        #region IDisposable Implementation

        public void Dispose()
        {
            clientNetworkedChangedPropertyFlag.Clear();
            clientRequests.Clear();

            UnsubscribeToEvents();
        }

        #endregion
    }
}