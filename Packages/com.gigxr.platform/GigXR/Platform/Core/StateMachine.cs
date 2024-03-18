using System.Collections.Generic;
using GIGXR.Platform.Managers;
using GIGXR.Platform.Interfaces;
using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Sessions;

namespace GIGXR.Platform
{
    // public enum UXStates
    // {
    //     Login, // logged out
    //     AuthFail, // not used
    //     PreSession, // logged in but not in session
    //     InSession, // logged in and in a session
    //     QrScanning, // scanning a qr code
    //     AuthFailTimeout // not used
    // }

    public static class StateMachine
    {
        // --- Private Variables:

        private static List<IStateUpdated> handlers;

        // --- Public Properties:

        // public static UXStates UxState { get; private set; }

        private static ISessionManager SessionManager { get; set; }

        private static IAuthenticationManager AuthenticationManager { get; set; }

        // --- Public Methods:

        [InjectDependencies]
        public static void Construct
        (
            ISessionManager sessionManager,
            IAuthenticationManager authManager
        )
        {
            SessionManager = sessionManager;
            AuthenticationManager = authManager;

            // AuthenticationManager.OnTimedOut += Instance_OnTimedOut;
            // AuthenticationManager.OnFailed += Instance_OnFailed;
            // AuthenticationManager.OnForbidden += Instance_OnForbidden;
            //
            // AuthenticationManager.OnLogout += Instance_OnLogout;

            if (SessionManager != null)
            {
                // TODO
                /*SessionManager.LostConnection += OnLostConnection;
                SessionManager.LeftSession += OnLeftSession;
                SessionManager.JoinedSession += OnJoinedSession;*/
            }
        }

        public static void Dispose()
        {
            if (SessionManager != null)
            {
                /*SessionManager.LostConnection -= OnLostConnection;
                SessionManager.LeftSession -= OnLeftSession;
                SessionManager.JoinedSession -= OnJoinedSession;*/
            }

            // AuthenticationManager.OnTimedOut -= Instance_OnTimedOut;
            // AuthenticationManager.OnFailed -= Instance_OnFailed;
            // AuthenticationManager.OnForbidden -= Instance_OnForbidden;
            //
            // AuthenticationManager.OnLogout -= Instance_OnLogout;
        }

        // private static void Instance_OnTimedOut
        // (
        //     UXStates value,
        //     string message
        // ) => UpdateState(value);
        //
        // private static void Instance_OnForbidden
        // (
        //     UXStates value,
        //     string message
        // ) => UpdateState(value);
        //
        // private static void Instance_OnFailed
        // (
        //     UXStates value,
        //     string message
        // ) => UpdateState(value);
        //
        // private static void Instance_OnLogout
        // (
        //     UXStates value
        // ) => UpdateState(value);


        /// <summary>
        /// Registers an IStateUpdated handler.
        /// </summary>
        /// <param name="newHandler"></param>
        public static void RegisterHandler
        (
            IStateUpdated newHandler
        )
        {
            if (handlers == null) handlers = new List<IStateUpdated>();
            handlers.Add(newHandler);
        }

        /// <summary>
        /// Unregisters an IStateUpdated handler.
        /// </summary>
        /// <param name="handler"></param>
        public static void UnregisterHandler
        (
            IStateUpdated handler
        )
        {
            handlers.Remove(handler);
        }

        /// <summary>
        /// Updates the global UX state to the input state.
        /// </summary>
        /// <param name="newState"></param>
        // public static void UpdateState
        // (
        //     UXStates newState
        // )
        // {
        //     UnityEngine.MonoBehaviour.print("Update State");
        //     if (newState == UxState) return;
        //     UnityEngine.MonoBehaviour.print("Updating State to " + newState);
        //     UxState = newState;
        //
        //     // copy the list over, we cant lock as we actually DO want to add to it.
        //     //List<IStateUpdated> handlersCopy = handlers;
        //     lock (handlers)
        //     {
        //         foreach (IStateUpdated handler in handlers)
        //         {
        //             handler.StateUpdated(newState);
        //         }
        //     }
        // }

        // --- Private Methods:
        
        // TODO replace with event bus events - see SessionEvents 

        // private static void OnLostConnection() => UpdateState(UXStates.Login);
        //
        // private static void OnLeftSession() => UpdateState(UXStates.PreSession);
        //
        // private static void OnJoinedSession() => UpdateState(UXStates.InSession);
    }
}