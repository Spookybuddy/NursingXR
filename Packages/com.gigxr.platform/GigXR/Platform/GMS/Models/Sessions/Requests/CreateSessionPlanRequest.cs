namespace GIGXR.GMS.Models.Sessions.Requests
{
    using Newtonsoft.Json.Linq;
    using System;

    /// <summary>
    /// A data holder class to help create a session plan in GMS
    /// </summary>
    public class CreateSessionPlanRequest
    {
        public string SessionName { get; set; } = null!;

        [Obsolete(
            "No longer used since v2.12.0 of the GMS API. Session plans will always be created under the institution of the requesting user.")]
        public Guid InstitutionId { get; set; }

        public Guid ClientAppId { get; set; }

        public Guid? ClassId { get; set; }

        public Guid? InstructorId { get; set; }

        public string Description { get; set; } = "";

        public SessionPermission SessionPermission { get; set; }

        public JObject HmdJson { get; set; } = new JObject();

        // Added for version 1.1
        public string? ClientAppVersion { get; set; }
    }
}