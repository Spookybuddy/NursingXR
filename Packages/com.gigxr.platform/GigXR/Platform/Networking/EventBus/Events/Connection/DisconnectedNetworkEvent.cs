namespace GIGXR.Platform.Networking.EventBus.Events.Connection
{
    public class DisconnectedNetworkEvent : BaseNetworkEvent
    {
        public DisconnectionCause Cause { get; }

        public bool FromConnectionLost
        {
            get
            {
                return Cause == DisconnectionCause.ClientTimeout || 
                       Cause == DisconnectionCause.Exception;
            }
        }

        public DisconnectedNetworkEvent(DisconnectionCause cause)
        {
            Cause = cause;
        }

        public override string ToString()
        {
            return $"Cause: {Cause}";
        }
    }

    // This is just a replication of Photon's DisconnectCause, allowing us to pass an enum without subscribings needing to know Photon
    public enum DisconnectionCause
    {
        /// <summary>No error was tracked.</summary>
        None,
        /// <summary>OnStatusChanged: The server is not available or the address is wrong. Make sure the port is provided and the server is up.</summary>
        ExceptionOnConnect,
        /// <summary>OnStatusChanged: Some internal exception caused the socket code to fail. This may happen if you attempt to connect locally but the server is not available. In doubt: Contact Exit Games.</summary>
        Exception,

        /// <summary>OnStatusChanged: The server disconnected this client due to timing out (missing acknowledgement from the client).</summary>
        ServerTimeout,

        /// <summary>OnStatusChanged: This client detected that the server's responses are not received in due time.</summary>
        ClientTimeout,

        /// <summary>OnStatusChanged: The server disconnected this client from within the room's logic (the C# code).</summary>
        DisconnectByServerLogic,
        /// <summary>OnStatusChanged: The server disconnected this client for unknown reasons.</summary>
        DisconnectByServerReasonUnknown,

        /// <summary>OnOperationResponse: Authenticate in the Photon Cloud with invalid AppId. Update your subscription or contact Exit Games.</summary>
        InvalidAuthentication,
        /// <summary>OnOperationResponse: Authenticate in the Photon Cloud with invalid client values or custom authentication setup in Cloud Dashboard.</summary>
        CustomAuthenticationFailed,
        /// <summary>The authentication ticket should provide access to any Photon Cloud server without doing another authentication-service call. However, the ticket expired.</summary>
        AuthenticationTicketExpired,
        /// <summary>OnOperationResponse: Authenticate (temporarily) failed when using a Photon Cloud subscription without CCU Burst. Update your subscription.</summary>
        MaxCcuReached,

        /// <summary>OnOperationResponse: Authenticate when the app's Photon Cloud subscription is locked to some (other) region(s). Update your subscription or master server address.</summary>
        InvalidRegion,

        /// <summary>OnOperationResponse: Operation that's (currently) not available for this client (not authorized usually). Only tracked for op Authenticate.</summary>
        OperationNotAllowedInCurrentState,
        /// <summary>OnStatusChanged: The client disconnected from within the logic (the C# code).</summary>
        DisconnectByClientLogic,

        /// <summary>The client called an operation too frequently and got disconnected due to hitting the OperationLimit. This triggers a client-side disconnect, too.</summary>
        /// <summary>To protect the server, some operations have a limit. When an OperationResponse fails with ErrorCode.OperationLimitReached, the client disconnects.</summary>
        DisconnectByOperationLimit,

        /// <summary>The client received a "Disconnect Message" from the server. Check the debug logs for details.</summary>
        DisconnectByDisconnectMessage
    }
}