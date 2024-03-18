namespace GIGXR.Platform.AppEvents.Events.Session
{
    public class EnteredWaitingRoomLobbyEvent : BaseSessionEvent
    {
        public bool FromHostDisconnection { get; }

        public EnteredWaitingRoomLobbyEvent(bool fromHostDisconnect) 
        {
            FromHostDisconnection = fromHostDisconnect;
        }
    }
}