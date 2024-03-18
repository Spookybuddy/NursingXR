namespace GIGXR.Platform.AppEvents.Events.Session
{
    using GIGXR.Platform.Data;

    public class SetUserLocationLocalEvent : BaseSessionEvent
    {
        public SessionParticipantStatus JoinSessionStatus { get; }

        public SetUserLocationLocalEvent(SessionParticipantStatus joinSessionStatus)
        {
            JoinSessionStatus = joinSessionStatus;
        }
    }
}