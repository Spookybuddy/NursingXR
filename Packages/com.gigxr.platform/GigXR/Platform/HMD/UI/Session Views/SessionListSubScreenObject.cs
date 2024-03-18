namespace GIGXR.Platform.HMD.UI
{
    using GIGXR.Platform.Core.DependencyInjection;
    using GIGXR.Platform.Managers;
    using GIGXR.Platform.Networking;
    using GIGXR.Platform.Networking.EventBus.Events.Connection;
    using GIGXR.Platform.AppEvents.Events.Authentication;
    using GIGXR.Platform.AppEvents.Events.UI;
    using GIGXR.Platform.HMD.AppEvents.Events.UI;
    using GIGXR.Platform.Interfaces;
    using GIGXR.Platform.UI;
    using UnityEngine;
    using System;
    using UnityEngine.UI;
    using GIGXR.Platform.Sessions;

    /// <summary>
    /// Not an actual SubScreenObject, but an accessory component to help with the SessionList.
    /// </summary>
    public class SessionListSubScreenObject : BaseUiObject, IScrollInput, ITabInput
    {
        [SerializeField]
        private ToggleGroup sessionTabToggleGroup;

        [SerializeField]
        private VerticalLayoutGroup verticalSessionGroup;

        private SessionList sessionList;

        // By default, we will only show compatible sessions to start
        private bool showAllSessions = false;

        private SessionListTypes currentSessionListType = SessionListTypes.ActiveSessions;

        private INetworkManager NetworkManager;
        protected ISessionManager SessionManager;
        private IAuthenticationManager AuthenticationManager;
        private IBuilderManager BuilderManager;
        
        public void StartAdHocSession()
        {
            GetComponentInParent<SessionScreen>().StartAdHocSession();
        }

        protected void Awake()
        {
            sessionList = GetComponentInChildren<SessionList>(true);
        }

        protected void OnDestroy()
        {
            NetworkManager.Unsubscribe<DisconnectedNetworkEvent>(OnDisconnectedNetworkEvent);

            EventBus.Unsubscribe<StartLogOutEvent>(OnStartLogOutEvent);
        }

        [InjectDependencies]
        public void Construct(ISessionManager sessionManager, INetworkManager networkManager, IAuthenticationManager authenticationManager, IBuilderManager builderManager, ProfileManager profileManager)
        {
            SessionManager = sessionManager;
            NetworkManager = networkManager;
            AuthenticationManager = authenticationManager;
            BuilderManager = builderManager;

            NetworkManager.Subscribe<DisconnectedNetworkEvent>(OnDisconnectedNetworkEvent);

            // If a GMS version is not defined, show all sessions
            if(string.IsNullOrEmpty(profileManager.authenticationProfile.TargetEnvironmentalDetails.GmsVersion) ||
               profileManager.authenticationProfile.TargetEnvironmentalDetails.GmsVersion == "1.0")
            {
                showAllSessions = true;
            }
        }

        private void OnDisconnectedNetworkEvent(DisconnectedNetworkEvent @event)
        {
            if (@event.FromConnectionLost)
            {
                // put up a prompt telling the user they have lost connection
                // until they reconnect.
                EventBus.Publish
                    (
                        // TODO utilize Localization for text prompts
                        new ShowPredicatePromptEvent
                            (
                                () => NetworkManager.IsConnected || !AuthenticationManager.GMSApiClient.AccountsApi.IsLoggedIn,
                                "Lost connection to server",
                                null,
                                PromptManager.WindowStates.Narrow
                            )
                    );
            }
        }

        private void OnStartLogOutEvent(StartLogOutEvent @event)
        {
            currentSessionListType = SessionListTypes.ActiveSessions;
        }

        #region PublicAPI

        public SessionListTypes CurrentSessionListType { get { return currentSessionListType; } }

        protected override void SubscribeToEventBuses()
        {
            sessionTabToggleGroup.Construct(uiEventBus);

            EventBus.Subscribe<StartLogOutEvent>(OnStartLogOutEvent);
        }

        public void Clear()
        {
            sessionList.Clear();
        }

        private void UpdateSession
        (
            SessionListTypes newCurrentSessionListType
        )
        {
            sessionList.UpdateSessions(newCurrentSessionListType, showAllSessions);
        }

        /// <summary>
        /// Instructs the authentication manager to begin log out procedure.
        /// 
        /// Called via the Unity Editor in the Logout button on the SessionList subscreen
        /// </summary>
        public void LogOut()
        {
            // Inform the app that the user has logged out
            EventBus.Publish(new StartLogOutEvent());
        }

        /// <summary>
        /// Refreshes the list of session buttons.
        /// 
        /// Called via the Unity Editor
        /// </summary>
        public void RefreshSessions()
        {
            UpdateSession(currentSessionListType);
        }

        /// <summary>
        /// Grabs either all sessions, or only compatible sessions, depending
        /// on the toggle button.
        /// 
        /// Called via the Unity Editor.
        /// </summary>
        public void SwitchSessionCompatibility()
        {
            showAllSessions = !showAllSessions;

            RefreshSessions();
        }

        private void SetSessionListType(SessionListTypes newSessionListType)
        {
            // Disable the toggle buttons while the session list loads
            uiEventBus.Publish(new SettingToggleButtonStateEvent(false));

            currentSessionListType = newSessionListType;

            RefreshSessions();
        }

        #endregion

        #region IScrollInput Implementation

        public void ScrollUp()
        {
            sessionList.ScrollUp();
        }

        public void ScrollDown()
        {
            sessionList.ScrollDown();
        }

        #endregion

        #region ITabInput Implementation

        public void TabSelected(int tabIndex)
        {
            // TODO These tabs are based on indices and this is brittle, think of better way to reference them
            switch (tabIndex)
            {
                case 0:
                    SetSessionListType(SessionListTypes.ActiveSessions);
                    break;
                case 1:
                    SetSessionListType(SessionListTypes.SavedSessions);
                    break;
                case 2:
                    SetSessionListType(SessionListTypes.SessionPlans);
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}