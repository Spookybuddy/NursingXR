namespace GIGXR.GMS.Models.Sessions.Requests
{
    using Newtonsoft.Json.Linq;
    using System;

    /// <summary>
    /// A data holder class to help create a session in GMS
    /// </summary>
    public class CreateSessionRequest
    {
        public string SessionName { get; set; } = null!;

        public Guid InstitutionId { get; set; }

        public Guid ClientAppId { get; set; }

        public Guid? ClassId { get; set; }

        public Guid? InstructorId { get; set; }

        public DateTime? LessonDate { get; set; }

        public string Description { get; set; } = "";

        public SessionStatus SessionStatus { get; set; }

        public SessionPermission SessionPermission { get; set; }

        public bool Locked { get; set; } = false;

        public JObject HmdJson { get; set; } = new JObject();

        public bool Saved { get; set; }

        public string SessionNote { get; set; } = "";

        public bool SessionNoteVisible { get; set; }

        // Added for version 1.1
        public string? ClientAppVersion { get; set; }
    }
}