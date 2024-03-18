
namespace GIGXR.Platform.Mobile.DevelopementTools
{
    using System;

    using UnityEngine;
    using UnityEditor;

    using GIGXR.Platform.Core;
    using GIGXR.Platform.Interfaces;
    using GIGXR.Platform.AppEvents;
    using GIGXR.Platform.AppEvents.Events.Authentication;
    using GIGXR.Platform.Mobile.AppEvents.Events.AR;
    using Sessions;

    /// <summary>
    /// Editor tool to login and join sessions in the editor.
    /// This class will be completely pointless if UniWebView
    /// ever adds Unity Windows support.
    /// </summary>
    public class MobileEventInjection : EditorWindow
    {
        private const string LOGTAG = "[MobileEventInjector]";

        private string username;
        private string password;

        private string sessionId;

        GIGXRCore core;
        private AppEventBus eventBus;

        [MenuItem("GIGXR/Mobile Event Injection")]
        public static void ShowWindow()
        {
            GetWindow<MobileEventInjection>().Show();
        }

        void OnGUI()
        {
            #region Authentication GUI

            GUILayout.Label("Login", EditorStyles.boldLabel);

            username = EditorGUILayout.TextField("Username", username);
            password = EditorGUILayout.TextField("Password", password);

            if (GUILayout.Button("login"))
            {
                AttemptLogin();
            }

            #endregion


            #region Session GUI

            GUILayout.Label("Session", EditorStyles.boldLabel);

            sessionId = EditorGUILayout.TextField("Session ID", sessionId);

            if (GUILayout.Button("join"))
            {
                AttemptJoinSession();
            }

            if (GUILayout.Button("leave"))
            {
                AttemptLeaveSession();
            }

            #endregion
        }

        #region Authentication

        private void AttemptLogin()
        {
            Log($"Login button pressed with credentials {username} and {password}.");

            core = FindObjectOfType<GIGXRCore>();
            eventBus = core.DependencyProvider.GetDependency<AppEventBus>();

            if (eventBus == null)
            {
                Log("Unable to find AppEventBus for login");
                return;
            }

            Log("Attempting login...");
            eventBus.Subscribe<AuthenticatedUserEvent>(OnAuthenticatedUserEvent);
            eventBus.Publish(new BeginAuthenticatingEvent(username, password));
        }

        private void OnAuthenticatedUserEvent(AuthenticatedUserEvent @event)
        {
            eventBus?.Unsubscribe<AuthenticatedUserEvent>(OnAuthenticatedUserEvent);

            if (@event.AuthenticationStatus != IAuthenticationManager.AuthenticationStatus.Success)
            {
                Log($"Authentication failed with result of {@event.AuthenticationStatus}");
                eventBus = null;
                return;
            }

            Log("Login Successful!");
        }

        #endregion


        #region Session

        private void AttemptJoinSession()
        {
            Log($"Attempting to join session with id {sessionId}");

            eventBus.Publish<ArStartScanningEvent>(new ArStartScanningEvent());
            core.DependencyProvider.GetDependency<ISessionManager>().JoinSessionAsync(Guid.Parse(sessionId));
        }

        private async void AttemptLeaveSession()
        {
            Log($"Attempting to leave current session.");
            await core.DependencyProvider.GetDependency<ISessionManager>().LeaveSessionAsync();
        }

        #endregion


        static void Log(string message)
        {
            Debug.Log($"{LOGTAG} -- {message}");
        }
    }
}
