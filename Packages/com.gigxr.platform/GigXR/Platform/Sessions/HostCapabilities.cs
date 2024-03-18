using Cysharp.Threading.Tasks;
using GIGXR.GMS.Clients;
using GIGXR.GMS.Models.Sessions.Requests;
using GIGXR.Platform.AppEvents;
using GIGXR.Platform.AppEvents.Events;
using GIGXR.Platform.AppEvents.Events.Calibration;
using GIGXR.Platform.AppEvents.Events.UI;
using GIGXR.Platform.Data;
using GIGXR.Platform.Networking;
using GIGXR.Platform.Networking.EventBus.Events;
using GIGXR.Platform.Networking.EventBus.Events.InRoom;
using GIGXR.Platform.Networking.EventBus.Events.Scenarios;
using GIGXR.Platform.Networking.EventBus.Events.Sessions;
using GIGXR.Platform.Networking.EventBus.Events.Stages;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.EventArgs;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Scenarios.GigAssets.Data;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using GIGXR.Platform.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GIGXR.Platform.Sessions
{
    /// <summary>
    /// The set of actions and responsibilities of a host user. A host is someone who is leading a session
    /// for other users.
    /// </summary>
    public class HostCapabilities : ISessionCapability, IDisposable
    {
        private bool hostIsCurrentlyTransferingHostStatus = false;

        #region Dependencies

        private ISessionManager SessionManager { get; }

        private IScenarioManager ScenarioManager { get; }

        private INetworkManager NetworkManager { get; }

        private IGigAssetManager AssetManager { get; }

        private GmsApiClient ApiClient { get; }

        private AppEventBus EventBus { get; }

        #endregion

        public HostCapabilities(ISessionManager sessionManager, IScenarioManager scenarioManager, INetworkManager networkManager,
                                GmsApiClient gmsApiClient, AppEventBus appEvent)
        {
            SessionManager = sessionManager;
            ScenarioManager = scenarioManager;
            NetworkManager = networkManager;
            AssetManager = ScenarioManager.AssetManager;
            ApiClient = gmsApiClient;
            EventBus = appEvent;
        }

        #region ISessionCapabilityImplementation

        public void Activate()
        {
            // The host needs physics to run so colliders and triggers will work, but only if the scenario is playing
            if (ScenarioManager.ScenarioStatus == Scenarios.Data.ScenarioStatus.Playing)
            {
                Physics.autoSimulation = true;
            }

            SubscribeToEvents();
        }

        public void Deactivate()
        {
            UnsubscribeToEvents();
        }

        #endregion

        #region Scenario Local EventHandlers

        private void ScenarioManager_ScenarioStopped(object sender, ScenarioStoppedEventArgs e)
        {
            NetworkManager.RaiseNetworkEvent(new ScenarioStoppedNetworkEvent(
                new ScenarioSyncData
                (
                    NetworkManager.ServerTime,
                    Scenarios.Data.ScenarioStatus.Stopped,
                    (int)ScenarioManager.ActiveScenarioTimer.TimeInSimulation.TotalMilliseconds,
                    (int)ScenarioManager.ActiveScenarioTimer.TimeInPlayingScenario.TotalMilliseconds,
                    0
                )
            ));

            Physics.autoSimulation = false;
        }

        private void ScenarioManager_ScenarioPaused(object sender, ScenarioPausedEventArgs e)
        {
            NetworkManager.RaiseNetworkEvent(new ScenarioPausedNetworkEvent(
                new ScenarioSyncData
                (
                    NetworkManager.ServerTime,
                    Scenarios.Data.ScenarioStatus.Paused,
                    (int)ScenarioManager.ActiveScenarioTimer.TimeInSimulation.TotalMilliseconds,
                    (int)ScenarioManager.ActiveScenarioTimer.TimeInPlayingScenario.TotalMilliseconds,
                    0
                )
            ));

            Physics.autoSimulation = false;
        }

        private void ScenarioManager_ScenarioPlaying(object sender, ScenarioPlayingEventArgs e)
        {
            NetworkManager.RaiseNetworkEvent(new ScenarioPlayingNetworkEvent
            (
                new ScenarioSyncData
                (
                    NetworkManager.ServerTime,
                    Scenarios.Data.ScenarioStatus.Playing,
                    (int)ScenarioManager.ActiveScenarioTimer.TimeInSimulation.TotalMilliseconds,
                    (int)ScenarioManager.ActiveScenarioTimer.TimeInPlayingScenario.TotalMilliseconds,
                    0
                )
            ));

            Physics.autoSimulation = true;
        }

        #endregion

        #region HostManagement

        private void OnStartHostRequestEvent(StartHostRequestEvent @event)
        {
            // The host has not made a transfer request to a user
            if (!hostIsCurrentlyTransferingHostStatus ||
                @event.RestartRequest)
            {
                int actorNumber = NetworkManager.GetAlternativePlayerReference
                    (@event.CurrentUserId);

                if (actorNumber != -1)
                {
                    hostIsCurrentlyTransferingHostStatus = true;

                    // Send out over the network to the specified user if they would like to become the host
                    NetworkManager.RaiseNetworkEvent
                        (
                            new RequestUserToHostNetworkEvent
                                (actorNumber, NetworkManager.LocalPlayer.NickName)
                        );

                    EventBus.Publish
                        (
                            new StartNewHostPromptEvent
                                (
                                    @event.CurrentUserName,
                                    actorNumber,
                                    @event.CurrentUserId
                                )
                        );
                }
                else
                {
                    Debug.LogError
                        (
                            $"Could not find Actor Number when making a host request to user {@event.CurrentUserId}."
                        );
                }
            }
            // Else the host has already made a transfer request to another user, do not make another one
        }

        private void OnCancelHostRequestEvent(CancelHostRequestEvent @event)
        {
            // No longer sending a host request to another user
            hostIsCurrentlyTransferingHostStatus = false;

            // Removes the Prompt on the host side that stated they were waiting on the client
            EventBus.Publish(new RemoveHostRequestEvent());

            // Send out over the network to the specified user that the request to make them host has been canceled
            NetworkManager.RaiseNetworkEvent
                (
                    new CancelRequestUserToHostNetworkEvent
                        (@event.ActorNumber, NetworkManager.LocalPlayer.NickName)
                );
        }

        private void OnHostTransferCompleteEvent(HostTransferCompleteEvent @event)
        {
            // No longer sending a host request to another user
            hostIsCurrentlyTransferingHostStatus = false;
        }

        private void OnAcceptHostRequestNetworkEvent(AcceptHostRequestNetworkEvent @event)
        {
            // No longer sending a host request to another user
            hostIsCurrentlyTransferingHostStatus = false;

            NetworkManager.RaiseNetworkEvent(new PromoteToHostNetworkEvent(@event.UserId));

            EventBus.Publish(new ShowNewHostEvent(@event.UserName));
        }

        private void OnRejectHostRequestNetworkEvent(RejectHostRequestNetworkEvent @event)
        {
            // No longer sending a host request to another user
            hostIsCurrentlyTransferingHostStatus = false;

            EventBus.Publish(new ShowRejectedHostPromptEvent(@event.UserName));
        }

        private void OnHostTransferTimeoutEvent(HostTransferTimeoutEvent @event)
        {
            hostIsCurrentlyTransferingHostStatus = false;
        }

        private void OnSetContentMarkerEvent(SetContentMarkerEvent @event)
        {
            NetworkManager.RaiseNetworkEvent(new ContentMarkerUpdateNetworkEvent(@event.contentMarkerPosition,
                                                                                 @event.contentMarkerRotation));
        }

        #endregion

        #region Room Network EventHandler

        private async void OnPlayerLeftRoomNetworkEvent(PlayerLeftRoomNetworkEvent @event)
        {
            var userId = @event.Player.UserId;

            // if another player has left, update their participant status to attended. This is in case the user has crashed out or quit the app instead
            // of leaving the session and marking themselves attended.
            if (SessionManager.ActiveSession != null)
            {
                await ApiClient.SessionsApi.UpdateParticipantStatusAsync
                    (
                        SessionManager.ActiveSession.SessionId,
                        Guid.Parse(userId),
                        SessionParticipantStatus.Attended
                    );
            }
        }

        private async void OnPlayerEnteredRoomNetworkEvent(PlayerEnteredRoomNetworkEvent @event)
        {
            // When you are in a lobby and a player joins, check to see if they are the owner of the Photon room
            if (Guid.Parse(@event.Player.UserId) != ApiClient.AccountsApi.AuthenticatedAccount.AccountId)
            {
                // Update GMS with the current data
                await UpdateGMSEphemeralDataAsync();

                // Tell all user to get the latest data from GMS
                NetworkManager.RaiseNetworkEvent
                    (
                        new UpdateFromGMSNetworkEvent
                            (
                                SessionManager.ActiveSession.SessionId,
                                @event.Player.ActorNumber,
                                SessionManager.HostId,
                                ScenarioManager.StageManager.CurrentStage.StageId,
                                new ScenarioSyncData
                                (
                                    NetworkManager.ServerTime,
                                    ScenarioManager.ScenarioStatus,
                                    (int)ScenarioManager.ActiveScenarioTimer.TimeInSimulation.TotalMilliseconds,
                                    (int)ScenarioManager.ActiveScenarioTimer.TimeInPlayingScenario.TotalMilliseconds,
                                    (int)ScenarioManager.StageManager.TimeInPlayingStage.TotalMilliseconds
                                )
                            )
                    );

                ScenarioManager.AssetManager.SyncAllRuntimeRoomData();
            }
        }

        private async UniTask UpdateGMSEphemeralDataAsync()
        {
            try
            {
                // Since both these tasks can take a long while (relative to a frame), run them asynchronously in a thread since they do
                // not require the main thread
                await UniTask.RunOnThreadPool(
                        async () =>
                        {
                            // Export Scenario data without saving
                            var scenario = await ScenarioManager.ExportScenarioAsync(true);

                            await UniTask.SwitchToMainThread();

                            await ApiClient.SessionsApi.CreateEphemeralDataAsync
                                (
                                    NetworkManager.CurrentRoom.CustomProperties
                                        .GetEphemeralDataId(),
                                    new CreateEphemeralDataRequest(JObject.FromObject(scenario))
                                );
                        });
            }
            catch (Exception e)
            {
                Debug.LogError
                    (
                        $"Error while trying to update the GMS JSON for Session {SessionManager.ActiveSession.SessionName}"
                    );

                Debug.LogException(e);
            }
        }

        #endregion

        #region Asset Local EventHandlers

        private void AssetManager_AssetPropertyUpdated(object sender, AssetPropertyChangeEventArgs e)
        {
            // Only try to forward asset properties if the NetworkManager reports in a network room
            // When the client is joining a room, they will update their values, but don't allow those to be requested
            if (NetworkManager.InRoom &&
                e.Origin != AssetPropertyChangeOrigin.StageChange &&
                !ScenarioManager.IsSwitchingStatus)
            {
                NetworkManager.RaiseNetworkEvent
                        (
                            new AssetPropertyUpdateNetworkEvent
                                (
                                    e.AssetId,
                                    e.AssetPropertyName,
                                    SerializationUtilities
                                        .ObjectToByteArray
                                            (e.AssetPropertyValue)
                                )
                        );
            }
        }

        #endregion

        #region Asset Network EventHandlers

        private void OnRequestPropertyUpdateNetworkEvent(RequestPropertyUpdateNetworkEvent e)
        {
            // Check the asset for it's own authority asset type
            var assetWithChanges = ScenarioManager.AssetManager.GetById(e.AssetId);

            var authorityComponent = assetWithChanges?.GetComponent<NetworkAuthorityAssetTypeComponent>();

            // if no authority component, assume there are not authority needs; anyone can edit
            if (authorityComponent == null || authorityComponent.HasAuthority(e.UserId))
            {
                // Update the value for the asset, which will propagate to all users
                ScenarioManager.AssetManager.UpdateAssetProperty
                    (
                        e.AssetId,
                        e.AssetPropertyName,
                        e.Value
                    );

                NetworkManager.RaiseNetworkEvent
                    (
                        new ClientPropertyUpdatedNetworkEvent
                            (e.AssetId, e.AssetPropertyName)
                    );
            }
            else
            {
                DenyPropertyChange(assetWithChanges, e);
            }
        }

        private void OnClientReadyNetworkEvent(ClientReadyNetworkEvent @event)
        {
            ScenarioManager.AssetManager.SyncAllRuntimeRoomData();
        }

        private void OnRequestContentMarkerNetworkEvent(RequestContentMarkerNetworkEvent @event)
        {
            NetworkManager.RaiseNetworkEvent(new ContentMarkerUpdateNetworkEvent(AssetManager.CalibrationRootProvider.ContentMarkerRoot.localPosition,
                                                                                 AssetManager.CalibrationRootProvider.ContentMarkerRoot.localRotation,
                                                                                 @event.RequestingActor));

        }

        private void DenyPropertyChange
            (GameObject assetWithChanges, RequestPropertyUpdateNetworkEvent e)
        {
            var assetMediator = assetWithChanges.GetComponent<IAssetMediator>();

            //  Send a rejection event by sending them back the correct value
            NetworkManager.RaiseNetworkEvent
                (
                    new RejectPropertyUpdateNetworkEvent
                        (
                            e.AssetId,
                            e.AssetPropertyName,
                            assetMediator.GetAssetPropertyByteArray
                                (e.AssetPropertyName),
                            NetworkManager.GetAlternativePlayerReference
                                (e.UserId)
                        )
                );
        }

        #endregion

        #region Stage Local EventHandlers

        private void StageManager_StageSwitched
            (object sender, Scenarios.Stages.EventArgs.StageSwitchedEventArgs e)
        {
            NetworkManager.RaiseNetworkEvent
                (
                    new StagesSwitchedNetworkEvent
                    (
                        e.NewStageID,
                        new ScenarioSyncData
                        (
                            NetworkManager.ServerTime,
                            ScenarioManager.ScenarioStatus,
                            (int)ScenarioManager.ActiveScenarioTimer.TimeInSimulation.TotalMilliseconds - e.PreviousStageTime,
                            (int)ScenarioManager.ActiveScenarioTimer.TimeInPlayingScenario.TotalMilliseconds,
                            0
                        )
                    )
                );
        }

        private void StageManager_StagesSwapped
            (object sender, Scenarios.Stages.EventArgs.StagesSwappedEventArgs e)
        {
            NetworkManager.RaiseNetworkEvent
                (new StagesSwappedNetworkEvent(e.FirstStage, e.SecondStage));
        }

        private void StageManager_StagesLoaded
            (object sender, Scenarios.Stages.EventArgs.StagesLoadedEventArgs e)
        {
            NetworkManager.RaiseNetworkEvent(new StageLoadedNetworkEvent());
        }

        private void StageManager_StageRenamed
            (object sender, Scenarios.Stages.EventArgs.StageRenamedEventArgs e)
        {
            NetworkManager.RaiseNetworkEvent
                (new StageRenamedNetworkEvent(e.StageID, e.NewName));
        }

        private void StageManager_StageRemoved
            (object sender, Scenarios.Stages.EventArgs.StageRemovedEventArgs e)
        {
            NetworkManager.RaiseNetworkEvent(new StageRemovedNetworkEvent(e.RemovedStageID));
        }

        private void StageManager_StageDuplicated
            (object sender, Scenarios.Stages.EventArgs.StageDuplicatedEventArgs e)
        {
            NetworkManager.RaiseNetworkEvent(new StageDuplicatedNetworkEvent(e.NewStageID));
        }

        private void StageManager_StageCreated
            (object sender, Scenarios.Stages.EventArgs.StageCreatedEventArgs e)
        {
            NetworkManager.RaiseNetworkEvent(new StageCreatedNetworkEvent(e.StageID));
        }

        private void StageManager_AllStagesRemoved
            (object sender, Scenarios.Stages.EventArgs.AllStagesRemovedEventArgs e)
        {
            NetworkManager.RaiseNetworkEvent(new AllStagesRemovedNetworkEvent());
        }

        #endregion

        private void SubscribeToEvents()
        {
            // Local Scenario Events

            ScenarioManager.ScenarioPlaying += ScenarioManager_ScenarioPlaying;
            ScenarioManager.ScenarioPaused += ScenarioManager_ScenarioPaused;
            ScenarioManager.ScenarioStopped += ScenarioManager_ScenarioStopped;

            // Local Stage Events

            ScenarioManager.StageManager.AllStagesRemoved += StageManager_AllStagesRemoved;
            ScenarioManager.StageManager.StageCreated += StageManager_StageCreated;
            ScenarioManager.StageManager.StageDuplicated += StageManager_StageDuplicated;
            ScenarioManager.StageManager.StageRemoved += StageManager_StageRemoved;
            ScenarioManager.StageManager.StageRenamed += StageManager_StageRenamed;
            ScenarioManager.StageManager.StagesLoaded += StageManager_StagesLoaded;
            ScenarioManager.StageManager.StagesSwapped += StageManager_StagesSwapped;
            ScenarioManager.StageManager.StageSwitched += StageManager_StageSwitched;

            // Room Changes

            NetworkManager.Subscribe<PlayerLeftRoomNetworkEvent>(OnPlayerLeftRoomNetworkEvent);

            NetworkManager.Subscribe<PlayerEnteredRoomNetworkEvent>
                (OnPlayerEnteredRoomNetworkEvent);

            // Networked Host Management

            NetworkManager.Subscribe<AcceptHostRequestNetworkEvent>
                (OnAcceptHostRequestNetworkEvent);

            NetworkManager.Subscribe<RejectHostRequestNetworkEvent>
                (OnRejectHostRequestNetworkEvent);

            // Local Asset Management
            ScenarioManager.AssetManager.AssetPropertyUpdated += AssetManager_AssetPropertyUpdated;

            // Networked Asset Management

            NetworkManager.Subscribe<RequestPropertyUpdateNetworkEvent>(OnRequestPropertyUpdateNetworkEvent);
            NetworkManager.Subscribe<ClientReadyNetworkEvent>(OnClientReadyNetworkEvent);

            NetworkManager.Subscribe<RequestContentMarkerNetworkEvent>(OnRequestContentMarkerNetworkEvent);

            // Host Management

            EventBus.Subscribe<StartHostRequestEvent>(OnStartHostRequestEvent);
            EventBus.Subscribe<CancelHostRequestEvent>(OnCancelHostRequestEvent);
            EventBus.Subscribe<HostTransferCompleteEvent>(OnHostTransferCompleteEvent);
            EventBus.Subscribe<HostTransferTimeoutEvent>(OnHostTransferTimeoutEvent);
            EventBus.Subscribe<SetContentMarkerEvent>(OnSetContentMarkerEvent);
        }

        private void UnsubscribeToEvents()
        {
            // Local

            ScenarioManager.ScenarioPlaying -= ScenarioManager_ScenarioPlaying;
            ScenarioManager.ScenarioPaused -= ScenarioManager_ScenarioPaused;
            ScenarioManager.ScenarioStopped -= ScenarioManager_ScenarioStopped;

            ScenarioManager.StageManager.AllStagesRemoved -= StageManager_AllStagesRemoved;
            ScenarioManager.StageManager.StageCreated -= StageManager_StageCreated;
            ScenarioManager.StageManager.StageDuplicated -= StageManager_StageDuplicated;
            ScenarioManager.StageManager.StageRemoved -= StageManager_StageRemoved;
            ScenarioManager.StageManager.StageRenamed -= StageManager_StageRenamed;
            ScenarioManager.StageManager.StagesLoaded -= StageManager_StagesLoaded;
            ScenarioManager.StageManager.StagesSwapped -= StageManager_StagesSwapped;
            ScenarioManager.StageManager.StageSwitched -= StageManager_StageSwitched;

            ScenarioManager.AssetManager.AssetPropertyUpdated -= AssetManager_AssetPropertyUpdated;

            // Network

            NetworkManager.Unsubscribe<PlayerEnteredRoomNetworkEvent>
                (OnPlayerEnteredRoomNetworkEvent);

            NetworkManager.Unsubscribe<RequestPropertyUpdateNetworkEvent>
                (OnRequestPropertyUpdateNetworkEvent);

            NetworkManager.Unsubscribe<ClientReadyNetworkEvent>
                (OnClientReadyNetworkEvent);

            NetworkManager.Unsubscribe<RequestContentMarkerNetworkEvent>(OnRequestContentMarkerNetworkEvent);

            NetworkManager.Unsubscribe<AcceptHostRequestNetworkEvent>
                (OnAcceptHostRequestNetworkEvent);

            NetworkManager.Unsubscribe<RejectHostRequestNetworkEvent>
                (OnRejectHostRequestNetworkEvent);

            // EventBus

            EventBus.Unsubscribe<StartHostRequestEvent>(OnStartHostRequestEvent);
            EventBus.Unsubscribe<CancelHostRequestEvent>(OnCancelHostRequestEvent);
            EventBus.Unsubscribe<SetContentMarkerEvent>(OnSetContentMarkerEvent);
        }


        #region IDisposable Implementation

        public void Dispose()
        {
            UnsubscribeToEvents();
        }

        #endregion
    }
}