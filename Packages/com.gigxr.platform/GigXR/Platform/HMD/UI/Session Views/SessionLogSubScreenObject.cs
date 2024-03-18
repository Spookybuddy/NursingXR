namespace GIGXR.Platform.HMD.UI
{
    using TMPro;
    using GIGXR.Platform.Interfaces;
    using GIGXR.Platform.Sessions;
    using GIGXR.Platform.AppEvents.Events.UI;
    using GIGXR.Platform.AppEvents.Events.Session;
    using Microsoft.MixedReality.Toolkit.Utilities;
    using UnityEngine;
    using UnityEngine.Localization.Tables;
    using GIGXR.Platform.Networking;
    using GIGXR.Platform.Managers;
    using System.Collections.Generic;
    using GIGXR.GMS.Models.Sessions.Requests;
    using GIGXR.Dictation;
    using System.Threading;
    using GIGXR.Platform.UI;
    using GIGXR.Platform.Core.DependencyInjection;
    using Microsoft.MixedReality.Toolkit.UI;
    using GIGXR.Platform.Networking.EventBus.Events.Matchmaking;
    using System;
    using GIGXR.Platform.AppEvents.Events;
    using GIGXR.Platform.HMD.AppEvents.Events;
    using GIGXR.Platform.Core.User;

    /// <summary>
    /// Not an actual SubScreenObject, but an accessory component to help with the SessionList.
    /// </summary>
    public class SessionLogSubScreenObject : BaseUiObject, IScrollInput, IDictationInput
    {
        #region EditorSetVariables

        [Header("Session Management")]
        [SerializeField]
        private ButtonComponent sessionTitleButtonObject;

        [SerializeField]
        private TMP_InputField sessionNameInput;

        [Header("Sidebar Buttons")]
        [SerializeField]
        private GameObject[] ownerButtons;

        [SerializeField]
        private GameObject lockButton;

        [SerializeField]
        private GameObject unlockButton;

        [SerializeField]
        private GameObject[] hostButtons;

        [SerializeField]
        private GameObject[] clientButtons;

        [SerializeField]
        // Buttons that every user will need
        private GameObject[] universalButtons;

        [Header("Dictation Buttons")]
        [SerializeField]
        private GameObject startDictationButton;

        [SerializeField]
        private GameObject stopDictationButton;

        [Header("User Cards and Grid")]
        [Tooltip("How many user cards to show on the screen at any one time."), SerializeField]
        private int userCardsToShow = 6;

        private int userCardScrollAmount { get { return userCardsToShow / 2; } }

        [SerializeField]
        private GridObjectCollection userGrid;

        #endregion

        private GridObjectCollection sideBarGrid;

        private StringTable _sessionStringTable;

        // GameObject that holds the Dictation buttons to rename the session
        private GameObject dictationGroup;

        private BaseScreenObject attachedScreenObject;

        private UIPlacementData sessionLogPlacement;

        // When true, the prompt will display a warning that there are unsaved changes in the session
        private bool changesMadeToSessionRequiresSaving = false;

        private CancellationTokenSource savingIndicator;

        private bool isShowingPrompt = false;

        #region Dependencies

        private ISessionManager SessionManager { get; set; }

        private INetworkManager NetworkManager { get; set; }

        private IDictationManager DictationManager { get; set; }

        #endregion

        #region UnityAPI

        protected void Awake()
        {
            // Could move this somewhere else, but this allows the UserCards to know where to parent themselves
            // when they are instantiated over the network
            UserCard.SetupAllUserCards(userGrid.transform, userCardsToShow);

            dictationGroup = startDictationButton.transform.parent.gameObject;

            sideBarGrid = universalButtons[0].GetComponentInParent<GridObjectCollection>();

            // Start with the ability to rename a session disabled, if they start a session and they are the creator
            // of it, then they will be able to rename the session
            SetRenameSessionAbility(false);

            attachedScreenObject = GetComponentInParent<BaseScreenObject>();

            sessionLogPlacement = new UIPlacementData()
            {
                HostTransform = attachedScreenObject.RootScreenObject.transform,
                // This makes sure this is slightly ahead of all other prompts
                PositionOffset = new Vector3(0.0f, 0.0f, -0.03f)
            };
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<SessionRenamedEvent>(OnSessionRenamedEvent);
            EventBus.Unsubscribe<HostTransferCompleteEvent>(OnHostTransferCompleteEvent);
            EventBus.Unsubscribe<ReturnToSessionListEvent>(OnReturnToSessionListEvent);

            NetworkManager.Unsubscribe<JoinedRoomNetworkEvent>(OnJoinedRoomNetworkEvent);

            SessionManager.ScenarioManager.ScenarioStatusChanged -= ScenarioManager_ScenarioStatusChanged;

            SessionManager.SessionStartedSave -= SessionManager_SessionStartedSave;
            SessionManager.SessionFinishedSave -= SessionManager_SessionFinishedSave;
        }

        #endregion

        #region PublicAPI

        [InjectDependencies]
        public void Construct
        (
            ISessionManager sessionManager,
            INetworkManager networkManager,
            IDictationManager dictationManager
        )
        {
            SessionManager = sessionManager;
            NetworkManager = networkManager;
            DictationManager = dictationManager;
        }

        protected override void SubscribeToEventBuses()
        {
            EventBus.Subscribe<SessionRenamedEvent>(OnSessionRenamedEvent);
            EventBus.Subscribe<HostTransferCompleteEvent>(OnHostTransferCompleteEvent);
            EventBus.Subscribe<ReturnToSessionListEvent>(OnReturnToSessionListEvent);

            NetworkManager.Subscribe<JoinedRoomNetworkEvent>(OnJoinedRoomNetworkEvent);

            SessionManager.SessionStartedSave += SessionManager_SessionStartedSave;
            SessionManager.SessionFinishedSave += SessionManager_SessionFinishedSave;

            SessionManager.ScenarioManager.ScenarioStatusChanged += ScenarioManager_ScenarioStatusChanged;
        }

        private void SessionManager_SessionStartedSave(object sender, EventArgs e)
        {
            if (e is SessionManager.SessionSavedArgs savedSessionArgs)
            {
                // Only display this message when the session is suppose to be marked as Saved as that implies a long, time action
                // for serializing the data and making the GMS networked call
                if (savedSessionArgs.MarkedAsSavedInGMS)
                {
                    savingIndicator = new CancellationTokenSource();

                    // Show a prompt indicating a save is in progress as it can take a bit of time
                    EventBus.Publish
                        (
                            new ShowCancellablePromptEvent
                                (
                                    token: savingIndicator.Token,
                                    mainText: _sessionStringTable.GetEntry("savingText").GetLocalizedString(),
                                    buttons: null,
                                    windowWidth: PromptManager.WindowStates.Wide,
                                    placementData: sessionLogPlacement
                                )
                        );
                }
            }
        }

        private void SessionManager_SessionFinishedSave(object sender, EventArgs e)
        {
            if (e is SessionManager.SessionSavedArgs savedSessionArgs)
            {
                if (savedSessionArgs.MarkedAsSavedInGMS)
                {
                    savingIndicator?.Cancel();
                    savingIndicator?.Dispose();

                    savingIndicator = null;
                }
            }
        }

        private void ScenarioManager_ScenarioStatusChanged(object sender, Scenarios.EventArgs.ScenarioStatusChangedEventArgs e)
        {
            // We only care about tracking properties changes during Edit mode, property values will change during Play Mode
            if (e.OldStatus == Scenarios.Data.ScenarioStatus.Stopped)
            {
                SessionManager.ScenarioManager.AssetManager.AssetPropertyUpdated -= AssetManager_AssetPropertyUpdated;
            }

            if (e.NewStatus == Scenarios.Data.ScenarioStatus.Stopped)
            {
                // HACK Some properties seem to fire off right when subscribing to this, so wait a second to avoid these
                Invoke(nameof(RegisterAssetProperty), 1.0f);
            }
        }

        private void RegisterAssetProperty()
        {
            SessionManager.ScenarioManager.AssetManager.AssetPropertyUpdated += AssetManager_AssetPropertyUpdated;
        }

        private void AssetManager_AssetPropertyUpdated(object sender, Scenarios.GigAssets.EventArgs.AssetPropertyChangeEventArgs e)
        {
            //Debug.Log($"[SessionLogSubScreen] AssetPropertyUpdated {sender}, AssetPropertyName {e.AssetPropertyName}");

            // HACK TargetInfo from Attachments seem to sometimes randomly pop up, we're just going to ignore them from requiring a save
            if (e.AssetPropertyName == "targetInfo")
                return;

            // The property that changed does not matter, as long as there is one change, we need to save
            changesMadeToSessionRequiresSaving = true;
        }

        public void SetSessionStringTable(StringTable stringTable)
        {
            _sessionStringTable = stringTable;
        }

        /// <summary>
        /// Called via the UnityEditor.
        /// </summary>
        public void PromptLeaveSession()
        {
            if (isShowingPrompt)
                return;

            isShowingPrompt = true;

            var confirmCancelButtonList = new List<ButtonPromptInfo>()
            {
                new ButtonPromptInfo()
                {
                    buttonText = _sessionStringTable.GetEntry("yesText").GetLocalizedString(),
                    onPressAction = async () =>
                    {
                        isShowingPrompt = false;

                        await SessionManager.LeaveSessionAsync();
                    }
                },
                new ButtonPromptInfo()
                {
                    buttonText = _sessionStringTable.GetEntry("noText").GetLocalizedString(),
                    onPressAction = () =>
                    {
                        isShowingPrompt = false;
                    }
                }
            };

            EventBus.Publish
                (
                    new ShowPromptEvent
                        (
                            _sessionStringTable.GetEntry("leavePromptText").GetLocalizedString(),
                            confirmCancelButtonList,
                            sessionLogPlacement
                        )
                );
        }

        /// <summary>
        /// Called via the UnityEditor.
        /// </summary>
        public void PromptCloseSession()
        {
            if (isShowingPrompt)
                return;

            isShowingPrompt = true;

            // The session has never been Saved before, or there are changed in Edit mode that have occurred
            if (!SessionManager.ActiveSession.Saved || changesMadeToSessionRequiresSaving)
            {
                var confirmCancelButtonList = new List<ButtonPromptInfo>()
                {
                    new ButtonPromptInfo()
                    {
                        buttonText = _sessionStringTable.GetEntry("saveAndCloseText").GetLocalizedString(),
                        onPressAction = async () =>
                        {
                            isShowingPrompt = false;

                            await SessionManager.SaveSessionAsync(true);

                            await SessionManager.StopSessionAsync();
                        }
                    },
                    new ButtonPromptInfo()
                    {
                        buttonText = _sessionStringTable.GetEntry("closeText").GetLocalizedString(),
                        onPressAction = async () =>
                        {
                            isShowingPrompt = false;

                            await SessionManager.StopSessionAsync();
                        }
                    },
                    new ButtonPromptInfo()
                    {
                        buttonText = _sessionStringTable.GetEntry("cancelText").GetLocalizedString(),
                        onPressAction = () =>
                        {
                            isShowingPrompt = false;
                        }
                    }
                };

                EventBus.Publish
                    (
                        new ShowPromptEvent
                            (
                                _sessionStringTable.GetEntry("closePromptSavedText").GetLocalizedString(),
                                confirmCancelButtonList,
                                sessionLogPlacement
                            )
                    );
            }
            else
            {
                var yesnoButtonList = new List<ButtonPromptInfo>()
                {
                    new ButtonPromptInfo()
                    {
                        buttonText = _sessionStringTable.GetEntry("yesText").GetLocalizedString(),
                        onPressAction = async () =>
                        {
                            isShowingPrompt = false;

                            await SessionManager.StopSessionAsync();
                        }
                    },
                    new ButtonPromptInfo()
                    {
                        buttonText = _sessionStringTable.GetEntry("noText").GetLocalizedString(),
                        onPressAction = () =>
                        {
                            isShowingPrompt = false;
                        }
                    },
                };

                EventBus.Publish
                    (
                        new ShowPromptEvent
                            (
                                _sessionStringTable.GetEntry("closePromptText").GetLocalizedString(),
                                yesnoButtonList,
                                sessionLogPlacement
                            )
                    );
            }
        }

        /// <summary>
        /// Called via the UnityEditor.
        /// </summary>
        public void PromptSaveSession()
        {
            if (isShowingPrompt)
                return;

            isShowingPrompt = true;

            var confirmCancelButtonList = new List<ButtonPromptInfo>()
            {
                new ButtonPromptInfo()
                {
                    buttonText = _sessionStringTable.GetEntry("yesText").GetLocalizedString(),
                    onPressAction = async () =>
                    {
                        isShowingPrompt = false;

                        await SessionManager.SaveSessionAsync(true);

                        changesMadeToSessionRequiresSaving = false;

                        // Show timed prompt over this screen that the session has been saved, no buttons or cancel button
                        EventBus.Publish
                            (
                                new ShowTimedPromptEvent
                                    (
                                        mainText: _sessionStringTable.GetEntry("savedSessionText")
                                            .GetLocalizedString
                                                (SessionManager.ActiveSession.SessionName),
                                        buttons: null,
                                        placementData: sessionLogPlacement
                                    )
                            );
                    }
                },
                new ButtonPromptInfo()
                {
                    buttonText = _sessionStringTable.GetEntry("noText").GetLocalizedString(),
                    onPressAction = async () =>
                    {
                        isShowingPrompt = false;
                    }
                }
            };

            EventBus.Publish
                (
                    new ShowPromptEvent
                        (
                            _sessionStringTable.GetEntry("savePromptText").GetLocalizedString(),
                            confirmCancelButtonList,
                            sessionLogPlacement
                        )
                );
        }

        /// <summary>
        /// Called via the UnityEditor.
        /// </summary>
        public void PromptSaveSessionCopy()
        {
            if (isShowingPrompt)
                return;

            isShowingPrompt = true;

            var confirmCancelButtonList = new List<ButtonPromptInfo>()
            {
                new ButtonPromptInfo()
                {
                    buttonText = _sessionStringTable.GetEntry("yesText").GetLocalizedString(),
                    onPressAction = async () =>
                    {
                        isShowingPrompt = false;

                        await SessionManager.SaveSessionCopyAsync();

                        changesMadeToSessionRequiresSaving = false;

                        EventBus.Publish
                            (
                                new ShowTimedPromptEvent
                                    (
                                        mainText: _sessionStringTable.GetEntry
                                                ("savedSessionText")
                                            .GetLocalizedString
                                                (
                                                    SessionManager.ActiveSession.SessionName +
                                                    " Copy"
                                                ),
                                        buttons: null,
                                        placementData: sessionLogPlacement
                                    )
                            );
                    }
                },
                new ButtonPromptInfo()
                {
                    buttonText = _sessionStringTable.GetEntry("noText").GetLocalizedString(),
                    onPressAction = () =>
                    {
                        isShowingPrompt = false;
                    }
                }
            };

            EventBus.Publish
                (
                    new ShowPromptEvent
                        (
                            _sessionStringTable.GetEntry("savedSessionAsCopyText").GetLocalizedString(),
                            confirmCancelButtonList,
                            sessionLogPlacement
                        )
                );
        }

        /// <summary>
        /// Locks or unlocks the active session.
        /// 
        /// Called via the UnityEditor.
        /// </summary>
        /// <param name="shouldLock"></param>
        public async void LockSession(bool shouldLock)
        {
            await SessionManager.ApiClient.SessionsApi.SetSessionLockAsync
                (
                    SessionManager.ActiveSession.SessionId,
                    new UpdateSessionLockRequest() { Locked = shouldLock }
                );
        }

        /// <summary>
        /// Called via the UnityEditor
        /// </summary>
        /// <param name="newSessionName"></param>
        public void RenameSession(string newSessionName)
        {
            // If the provided string is empty, the action was canceled.
            // Use the old name to fill the field again and don't call RenameSessionAsync. 
            if (newSessionName == "")
            {
                sessionNameInput.text = SessionManager.ActiveSession.SessionName;
            }
            else
            {
                // Update the name on the SessionManager, which will update it across GMS and the app
                SessionManager.RenameSessionAsync(newSessionName);
            }
        }

        public void SetSessionNameText(string newSessionName)
        {
            sessionNameInput.text = newSessionName;
        }

        public void SwitchDictationButtons(bool makeDictationStartable)
        {
            startDictationButton.SetActive(makeDictationStartable);
            stopDictationButton.SetActive(!makeDictationStartable);
        }

        /// <summary>
        /// Set Leave or Close session button as active depending on whether client or host
        /// </summary>
        public void SetSidebarButton(bool isHost, bool isCreator)
        {
            // Set the sidebar buttons for the owner of the session (the ability to save this sessions)
            // Note the owner only has the abilities if they are also the host
            ForEachCollection(ownerButtons, (gameObject) => gameObject.SetActive(isCreator && isHost));

            // TODO Need to redo this lock/unlock button situation
            if (isCreator && isHost)
            {
                unlockButton.SetActive(SessionManager.ActiveSession.Locked);
                lockButton.SetActive(!SessionManager.ActiveSession.Locked);
            }

            // Set the buttons for the client (the ability to leave the session)
            ForEachCollection(clientButtons, (gameObject) => gameObject.SetActive(!isHost));

            // Set the buttons for the host (the ability to close and lock the session)
            ForEachCollection(hostButtons, (gameObject) => gameObject.SetActive(isHost));

            // Set the buttons all users will need (the ability to save a copy of the session
            ForEachCollection(universalButtons, (gameObject) => gameObject.SetActive(true));

            sideBarGrid.UpdateCollection();
        }

        /// <summary>
        /// Helper method to enumerate over a collection and apply an action 
        /// to the items.
        /// </summary>
        /// <typeparam name="T">The type in the collection</typeparam>
        /// <param name="collection">Any collection holding a set of objects</param>
        /// <param name="action">The action that will be applied to each item in 
        /// the collection</param>
        private void ForEachCollection<T>(T[] collection, Action<T> action)
        {
            foreach (var g in collection)
            {
                action?.DynamicInvoke(g);
            }
        }

        /// <summary>
        /// Called via the UnityEditor.
        /// </summary>
        /// <param name="value"></param>
        public void SetDicationState(bool value)
        {
            // When starting dictation, we want the button to stop dictation to appear
            SwitchDictationButtons(!value);

            if (value)
            {
                StartDictation();
            }
            else
            {
                CancelDictation();
            }
        }

        /// <summary>
        /// Used to enable or disable the ability to rename the session through
        /// the UI and bring down the dictation button.
        /// </summary>
        /// <param name="state"></param>
        public void SetRenameSessionAbility(bool state)
        {
            // Set the dictation level for editing the session name
            dictationGroup.SetActive(state);

            // Don't let the button generate clickable events
            sessionTitleButtonObject.IsDisabled(state);

            // Changes whether the object 'acts' like a button when clicked on
            sessionTitleButtonObject.GetComponent<Interactable>().enabled = state;

            // Switch the Input field interactable state so a keyboard isn't generated
            sessionNameInput.interactable = state;

            // Set the box visuals so the title does/does not have the illusion of a collider to press
            sessionTitleButtonObject.SetCompressableVisuals(state);
        }

        #endregion

        #region EventHandler

        private void OnSessionRenamedEvent(SessionRenamedEvent @event)
        {
            SetSessionNameText(@event.NewSessionName);
        }

        private void OnHostTransferCompleteEvent(HostTransferCompleteEvent @event)
        {
            SetRenameSessionAbility(SessionManager.IsHost && SessionManager.IsSessionCreator);
        }

        private void OnReturnToSessionListEvent(ReturnToSessionListEvent @event)
        {
            // Whenever the user leaves the session, remove the ability to rename the session
            // as they can only do this if they are the creator of the session
            SetRenameSessionAbility(false);

            // Reset the text since we no longer need it
            SetSessionNameText("");

            // Clear the NetworkLog as they are no longer needed
            EventBus.Publish(new ClearNetworkLogEvent());

            // No changes are saved between sessions
            changesMadeToSessionRequiresSaving = false;
        }

        private void OnJoinedRoomNetworkEvent(JoinedRoomNetworkEvent @event)
        {
            SetRenameSessionAbility(SessionManager.IsHost && SessionManager.IsSessionCreator);
        }

        #endregion

        #region IScrollInput Implementation

        public void ScrollUp()
        {
            UserCard.ScrollUp(userCardScrollAmount);
        }

        public void ScrollDown()
        {
            UserCard.ScrollDown(userCardScrollAmount, userGrid.transform.childCount);
        }

        #endregion

        #region IDictationInput Implementation

        public async void StartDictation()
        {
            DictationResult response = await DictationManager.DictateAsync(true);

            if (response.Status != DictationResultStatus.Canceled)
            {
                Debug.Log($"[SessionScreen] DictationResult Status: {response.Status}, Text: {response.ReturnedText}");
            }

            bool validChange = response.Status == DictationResultStatus.Success &&
                               response.ReturnedText != SessionManager.ActiveSession.SessionName;

            if (validChange)
            {
                await SessionManager.RenameSessionAsync(response.ReturnedText);
            }

            SwitchDictationButtons(true);
        }

        public void CancelDictation()
        {
            DictationManager.CancelDictation();

            SwitchDictationButtons(true);
        }

        #endregion
    }
}