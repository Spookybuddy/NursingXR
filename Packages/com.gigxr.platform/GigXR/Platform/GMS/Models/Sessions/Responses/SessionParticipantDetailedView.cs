namespace GIGXR.GMS.Models.Sessions.Responses
{
    using GIGXR.Platform.Data;
    using System;

    public class SessionParticipantDetailedView
    {
        public Guid AccountId { get; set; }

        public AccountLeafView Account { get; set; }

        public Guid SessionId { get; set; }

        public SessionParticipantStatus SessionParticipantStatus { get; set; }
    }
}