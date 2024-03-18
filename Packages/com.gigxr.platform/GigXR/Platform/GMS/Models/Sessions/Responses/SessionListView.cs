namespace GIGXR.GMS.Models.Sessions.Responses
{
    using Newtonsoft.Json.Linq;
    using Platform.Data;
    using System;

    public class SessionListView
    {
        public Guid SessionId { get; set; }

        public string SessionName { get; set; } = null!;

        public string Description { get; set; } = null!;

        public bool SessionPlan { get; set; }

        public DateTime? LessonDate { get; set; }

        public SessionStatus SessionStatus { get; set; }

        // public SessionPermission SessionPermission { get; set; }

        public bool Locked { get; set; }

        public Guid? InstructorId { get; set; }

        public AccountLeafView Instructor { get; set; } = null!;

        public Guid InstitutionId { get; set; }

        public Guid ClientAppId { get; set; }

        public Guid? ClassId { get; set; }

        // public ClassLeafView ClassEntity { get; set; } = null!;

        public DateTime LastActivity { get; set; }

        public JObject HmdJson { get; set; } = null!;

        public bool Saved { get; set; }

        public bool Invited { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        // Added for version 1.1
        public string? ClientAppVersion { get; set; }
    }
}