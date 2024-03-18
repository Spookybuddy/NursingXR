namespace GIGXR.GMS.Models.Sessions.Responses
{
    using Accounts;
    using Classes.Resposnes;
    using ClientApps.Responses;
    using Newtonsoft.Json.Linq;
    using Platform.Data;
    using System;

    /// <summary>
    /// Contains all the data needed to define a Session
    /// </summary>
    public class SessionDetailedView
    {
        public Guid SessionId { get; set; }

        public string SessionName { get; set; } = null!;

        public string Description { get; set; } = null!;

        public bool SessionPlan { get; set; }

        public DateTime? LessonDate { get; set; }

        public SessionStatus SessionStatus { get; set; }

        public SessionPermission SessionPermission { get; set; }

        public bool Locked { get; set; }

        public Guid? InstructorId { get; set; }

        public AccountLeafView Instructor { get; set; } = null!;

        public Guid InstitutionId { get; set; }

        public Guid ClientAppId { get; set; }

        public ClientAppView ClientApp { get; set; } = null!;

        public Guid? ClassId { get; set; }

        public ClassLeafView ClassEntity { get; set; } = null!;

        public JObject HmdJson { get; set; } = null!;

        public bool Saved { get; set; }

        public string SessionNote { get; set; } = null!;

        public bool SessionNoteVisible { get; set; }

        public DateTime LastActivity { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public AccountBasicView CreatedBy { get; set; } = null!;

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public AccountBasicView ModifiedBy { get; set; } = null!;

        // Added for version 1.1
        public string? ClientAppVersion { get; set; }
    }
}