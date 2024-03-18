using GIGXR.Platform.Core.EventBus;
using GIGXR.Platform.Interfaces;

namespace GIGXR.Platform.AppEvents.Events.Authentication
{
    public enum AuthenticationMethod
    {
        NONE = 0,
        MANUAL = 1,
        QRCODE = 2
    }
    
    /// <summary>
    /// Base event for a collection of update and request events regarding authentication.
    /// </summary>
    public abstract class BaseAuthenticationEvent : IGigEvent<AppEventBus>
    {
    }

    public class AuthenticatedUserEvent : BaseAuthenticationEvent
    {
        public IAuthenticationManager.AuthenticationStatus AuthenticationStatus { get; }

        public string Message { get; }

        public AuthenticationMethod Method { get; }
        
        /// <summary>
        /// Used to communicate information about user-initiated authentication results.
        /// </summary>
        /// <remarks>
        /// Upon success, the authenticating class should raise the LoggedInEvent.
        /// </remarks>
        /// <param name="authenticationResult">The result of the authentication attempt.</param>
        public AuthenticatedUserEvent(IAuthenticationManager.AuthenticationStatus authenticationResult, string message = null,
            AuthenticationMethod method = AuthenticationMethod.NONE)

        {
            AuthenticationStatus = authenticationResult;
            Message = message;
            Method = method;
        }
    }

    public class BeginAuthenticatingEvent : BaseAuthenticationEvent
    {
        public readonly string email;
        public readonly string password;

        /// <summary>
        /// Sends a request to begin authenticating using the retrieved credentials. 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        public BeginAuthenticatingEvent(string email, string password)
        {
            this.email = email;
            this.password = password;
        }
    }

    public class StartLogOutEvent : BaseAuthenticationEvent
    {
        public string LogoutMessage { get; }

        /// <summary>
        /// Sends a request to begin the logout process. 
        /// </summary>
        /// <param name="fromConnectionLost">Logged out due to disconnect.</param>
        public StartLogOutEvent(string logoutMessage = null)
        {
            LogoutMessage = logoutMessage;
        }
    }

    public class FinishedLogOutEvent : BaseAuthenticationEvent
    {
        /// <summary>
        /// Sends an update saying that the user has been logged out.  
        /// </summary>
        public FinishedLogOutEvent()
        {
        }
    }
}