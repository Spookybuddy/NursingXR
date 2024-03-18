using Cysharp.Threading.Tasks;
using GIGXR.GMS.Models.Sessions.Responses;
using GIGXR.Platform.AppEvents.Events.Session;
using GIGXR.Platform.AppEvents.Events.UI.ButtonEvents;
using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.HMD.AppEvents.Events.UI;
using GIGXR.Platform.Interfaces;
using GIGXR.Platform.Sessions;
using GIGXR.Platform.UI;
using GIGXR.Platform.Utilities;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GIGXR.Platform.HMD.UI
{
    /// <summary>
    /// Holds a list of session data and a GameObject pool to manage groups of session data in buttons.
    /// </summary>
    public class SessionList : BaseUiObject, IScrollInput
    {
        // --- Configured Values
        [SerializeField]
        private int visibleButtonCount = 4;

        [SerializeField]
        private float buttonFontSize = 0.004f;

        [SerializeField]
        GameObject sessionButtonPrefab;

        // --- Private Variables

        private VerticalLayoutGroup sessionGrid;

        private GameObject[] userGridSessionButtonPool;

        private int sessionListScrollIndex;

        private List<SessionListView> activeSessions;
        private List<SessionListView> savedSessions;
        private List<SessionPlanListView> sessionPlanSessions;

        private IList CurrentSessionPlan 
        { 
            get 
            {
                if(_currentSessionPlan == null)
                {
                    return Array.Empty<SessionListView>();
                }

                return _currentSessionPlan; 
            }
            set
            {
                _currentSessionPlan = value;
            }
        }

        private IList _currentSessionPlan;

        private bool showingSessionPlans = false;

        // --- Dependencies

        protected ISessionManager SessionManager { get; set; }

        private ProfileManager ProfileManager { get; set; }

        private IBuilderManager BuilderManager { get; set; }

        [InjectDependencies]
        public void Construct(ISessionManager sessionManager, IBuilderManager builderManager, ProfileManager profileManager)
        {
            SessionManager = sessionManager;
            BuilderManager = builderManager;
            ProfileManager = profileManager;

            userGridSessionButtonPool = new GameObject[visibleButtonCount];

            for (int n = 0; n < visibleButtonCount; n++)
            {
                userGridSessionButtonPool[n] = CreateSessionButton(sessionButtonPrefab, uiEventBus, sessionGrid.transform);
            }
        }

        protected void Awake()
        {
            sessionGrid = GetComponent<VerticalLayoutGroup>();

            sessionListScrollIndex = 0;
        }

        private bool IsSessionCompatible(SessionListView session)
        {
            return ProfileManager.appDetailsProfile.appDetailsScriptableObject.IsVersionCompatible(session.ClientAppVersion);
        }

        private bool IsSessionCompatible(SessionPlanListView session)
        {
            return ProfileManager.appDetailsProfile.appDetailsScriptableObject.IsVersionCompatible(session.ClientAppVersion);
        }

        #region PublicAPI

        /// <summary>
        /// Return the sessions available to the user, dependent on the current subscreen state.
        /// </summary>
        /// <returns></returns>
        public async void UpdateSessions
        (
            SessionListTypes currentSessionListType, bool showAllSessions
        )
        {
            switch (currentSessionListType)
            {
                case SessionListTypes.ActiveSessions:
                    List<SessionListView> activeSessions = await SessionManager.ApiClient.SessionsApi.GetActiveSessionListAsync();

                    // If filtering is set, only show sessions that are compatible with the current version
                    if(!showAllSessions)
                    {
                        // If a session has no app version, assume it's 
                        activeSessions = activeSessions.Where(session => session != null)
                                                       .Where(session => session.ClientAppVersion != null)
                                                       .Where(session => IsSessionCompatible(session))
                                                       .ToList();
                    }

                    // Update the session button grid to 
                    UpdateActiveSessionList(activeSessions);

                    break;
                case SessionListTypes.SavedSessions:
                    List<SessionListView> savedSessions = await SessionManager.ApiClient.SessionsApi.GetSavedSessionsAsync();

                    // If filtering is set, only show sessions that are compatible with the current version
                    if (!showAllSessions)
                    {
                        // If a session has no app version, assume it's 
                        savedSessions = savedSessions.Where(session => session != null)
                                                     .Where(session => session.ClientAppVersion != null)
                                                     .Where(session => IsSessionCompatible(session))
                                                     .ToList();
                    }

                    UpdateSavedSessionList(savedSessions);

                    break;
                case SessionListTypes.SessionPlans:
                    List<SessionPlanListView> sessionPlans = await SessionManager.ApiClient.SessionsApi.GetSessionPlansAsync();

                    // If filtering is set, only show sessions that are compatible with the current version
                    if (!showAllSessions)
                    {
                        // If a session has no app version, assume it's not compatible
                        sessionPlans = sessionPlans.Where(session => session != null)
                                                   .Where(session => session.ClientAppVersion != null)
                                                   .Where(session => IsSessionCompatible(session))
                                                   .ToList();
                    }

                    // Update the session button grid to 
                    UpdateSessionPlanList(sessionPlans);

                    break;
                default:
                    return;
            }
        }

        private async UniTask UpdateCurrentSession(IList sessions)
        {
            sessionListScrollIndex = 0;

            CurrentSessionPlan = sessions;

            await UpdateSessionButtons(sessionListScrollIndex);

            // By this point, the session list is up to date and can be clicked on again
            uiEventBus.Publish(new SettingToggleButtonStateEvent(true));
        }

        public async void UpdateActiveSessionList
        (
            List<SessionListView> newSessions
        )
        {
            activeSessions = newSessions;

            showingSessionPlans = false;

            await UpdateCurrentSession(activeSessions);
        }

        public async void UpdateSavedSessionList
        (
            List<SessionListView> newSavedSessions
        )
        {
            savedSessions = newSavedSessions;

            showingSessionPlans = false;

            await UpdateCurrentSession(savedSessions);
        }

        public async void UpdateSessionPlanList
        (
            List<SessionPlanListView> newSessionPlans
        )
        {
            sessionPlanSessions = newSessionPlans;

            showingSessionPlans = true;

            await UpdateCurrentSession(sessionPlanSessions);
        }

        public async void ScrollUp()
        {
            if ((sessionListScrollIndex - visibleButtonCount) >= 0)
                sessionListScrollIndex -= visibleButtonCount;
            else
                sessionListScrollIndex = 0;

            await UpdateSessionButtons(sessionListScrollIndex);
        }

        public async void ScrollDown()
        {
            if (sessionListScrollIndex < CurrentSessionPlan.Count - visibleButtonCount)
                sessionListScrollIndex += visibleButtonCount;

            await UpdateSessionButtons(sessionListScrollIndex);
        }

        public async void Clear()
        {
            sessionListScrollIndex = 0;

            activeSessions?.Clear();
            savedSessions?.Clear();
            sessionPlanSessions?.Clear();

            await UpdateSessionButtons(sessionListScrollIndex);
        }

        #endregion

        #region ButtonManagement

        private GameObject CreateSessionButton
        (
            GameObject sessionPrefab,
            UiEventBus uiEventBusInstance,
            Transform  sessionButtonParentTransform
        )
        {
            GameObject buttonGameObject = Instantiate(sessionPrefab);

            buttonGameObject.transform.SetParent(sessionButtonParentTransform, false);
            buttonGameObject.transform.localRotation = Quaternion.identity;

            SetGameObjectDetails(buttonGameObject, "No Session", false);

            return buttonGameObject;
        }

        private UniTask UpdateSessionButtons(int currentIndex)
        {
            for (int i = 0; i < userGridSessionButtonPool.Length; i++)
            {
                if(showingSessionPlans)
                {
                    SetSessionDataWindow(userGridSessionButtonPool[i],
                                         currentIndex + i < CurrentSessionPlan.Count
                                            ? (SessionPlanListView)CurrentSessionPlan[currentIndex + i]
                                            : null);
                }
                else
                {
                    SetSessionDataWindow(userGridSessionButtonPool[i],
                                         currentIndex + i < CurrentSessionPlan.Count
                                            ? (SessionListView)CurrentSessionPlan[currentIndex + i]
                                            : null);
                }
            }

            return UniTask.CompletedTask;
        }

        private void SetSessionDataWindow
        (
            GameObject      sessionButton,
            SessionListView session
        )
        {
            if (session != null)
            {
                SetGameObjectDetails(sessionButton, $"Session {session.SessionName}", true);

                var sessionListComponent = sessionButton.GetComponent<SessionListViewComponent>();

                // HACK brittle improve this looking, find the location for the button and place it here
                var buttonHolder = sessionButton.transform.Find("Button Location");

                // A little gross, but we recreate a new button for each time, so we need to make sure the button holder has no children
                foreach (Transform child in buttonHolder.transform)
                {
                    Destroy(child.gameObject);
                }

                string buttonText;

                if (session.Saved)
                {
                    sessionListComponent.SetSavedSessionListView(SessionManager.ApiClient, session);

                    if(session.CreatedById == SessionManager.ApiClient.AccountsApi.AuthenticatedAccount.AccountId)
                        buttonText = "Start";
                    else if(session.SessionStatus == GIGXR.GMS.Models.Sessions.SessionStatus.InProgress)
                        buttonText = "Join";
                    else
                        buttonText = "Join Copy";
                }
                else
                {
                    sessionListComponent.SetActiveSessionListView(SessionManager.ApiClient, session);

                    if (session.CreatedById == SessionManager.ApiClient.AccountsApi.AuthenticatedAccount.AccountId && 
                        session.SessionStatus == GIGXR.GMS.Models.Sessions.SessionStatus.New)
                        buttonText = "Start";
                    else
                        buttonText = "Join";
                }

                // If GMS Versioning is enabled, then don't allow buttons for incompatible sessions
                if (string.IsNullOrEmpty(ProfileManager.authenticationProfile.TargetEnvironmentalDetails.GmsVersion) ||
                    ProfileManager.authenticationProfile.TargetEnvironmentalDetails.GmsVersion == "1.0")
                {
                    sessionListComponent.SetCompatibility(true);

                    CreateButton(sessionListComponent, buttonText, buttonHolder);
                }
                else
                {
                    // Only create a button if the versions match
                    if (IsSessionCompatible(session))
                    {
                        sessionListComponent.SetCompatibility(true);

                        CreateButton(sessionListComponent, buttonText, buttonHolder);
                    }
                    else
                    {
                        // Set the session data to show incompatible markings like a light gray background to convey its state
                        sessionListComponent.SetCompatibility(false);
                    }
                }
            }
            else
            {
                SetGameObjectDetails(sessionButton.gameObject, "No Session", false);
            }
        }

        private void SetSessionDataWindow
        (
            GameObject sessionButton,
            SessionPlanListView session
        )
        {
            if (session != null)
            {
                SetGameObjectDetails(sessionButton, $"Session {session.SessionName}", true);

                var sessionListComponent = sessionButton.GetComponent<SessionListViewComponent>();

                // HACK brittle improve this looking, find the location for the button and place it here
                var buttonHolder = sessionButton.transform.Find("Button Location");

                // A little gross, but we recreate a new button for each time, so we need to make sure the button holder has no children
                foreach (Transform child in buttonHolder.transform)
                {
                    Destroy(child.gameObject);
                }

                // TODO Externalize these text fields
                string buttonText = "Create Session";
                sessionListComponent.SetSessionPlanListView(SessionManager.ApiClient, session);

                // If GMS Versioning is enabled, then don't allow buttons for incompatible sessions
                if (string.IsNullOrEmpty(ProfileManager.authenticationProfile.TargetEnvironmentalDetails.GmsVersion) ||
                    ProfileManager.authenticationProfile.TargetEnvironmentalDetails.GmsVersion == "1.0")
                {
                    sessionListComponent.SetCompatibility(true);

                    CreateButton(sessionListComponent, buttonText, buttonHolder);
                }
                else
                {
                    // Only create a button if the versions match
                    if (IsSessionCompatible(session))
                    {
                        sessionListComponent.SetCompatibility(true);

                        CreateButton(sessionListComponent, buttonText, buttonHolder);
                    }
                    else
                    {
                        // Set the session data to show incompatible markings like a light gray background to convey its state
                        sessionListComponent.SetCompatibility(false);
                    }
                }
            }
            else
            {
                SetGameObjectDetails(sessionButton.gameObject, "No Session", false);
            }
        }

        private void CreateButton(SessionListViewComponent sessionListComponent, string buttonText, Transform buttonHolder)
        {
            // Hook into the SessionButton's event when created, so it only occurs once
            var button = BuilderManager.BuildMRTKButton(() =>
            {
                OnSessionButtonClick(sessionListComponent);
            },
                                                    buttonText: buttonText,
                                                    buttonSize: new Vector3(0.025f, 0.0125f, 0.01f),
                                                    buttonLocation: Vector3.zero,
                                                    fontSize: buttonFontSize);

            button.transform.SetParent(buttonHolder, false);
        }

        private void SetGameObjectDetails
        (
            GameObject buttonGameObject,
            string     gameObjectName,
            bool       state
        )
        {
            buttonGameObject.name = gameObjectName;

            for (int n = 0; n < buttonGameObject.transform.childCount; n++)
            {
                var child = buttonGameObject.transform.GetChild(n);

                if (child != null)
                {
                    child.gameObject.SetActive(state);
                }
            }
        }

        /// <summary>
        /// On click join the session via the button properties
        /// </summary>
        /// <param name="sessionId"></param>
        private void OnSessionButtonClick(SessionListViewComponent sender)
        {
            // Prevent the user from trying to load another session right after clicking on a session button
            uiEventBus.Publish(new SettingGlobalButtonStateEvent(true, true));

            // Tell the app through the EventBus that a new session should be join and provide the ID so it can handle it
            EventBus.Publish(new AttemptStartSessionEvent(sender.GetSessionId()));
        }

        #endregion

        #region IDisposableImplementation

        protected override void SubscribeToEventBuses()
        {
            // Not need yet
        }

        #endregion
    }
}