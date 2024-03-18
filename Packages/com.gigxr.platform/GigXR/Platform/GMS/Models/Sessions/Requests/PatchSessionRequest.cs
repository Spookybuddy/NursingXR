namespace GIGXR.GMS.Models.Sessions.Requests
{
    using Newtonsoft.Json.Linq;
    using System;

    /// <summary>
    /// A data holder class to help update only the session status of a status.
    /// </summary>
    [Serializable]
    public class PatchSessionRequest
    {
        public Guid SessionId { get; set; }

        public SessionStatus? SessionStatus { get; set; }
    }
}