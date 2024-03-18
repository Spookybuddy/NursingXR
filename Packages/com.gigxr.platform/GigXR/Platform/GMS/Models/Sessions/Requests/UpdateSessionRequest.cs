namespace GIGXR.GMS.Models.Sessions.Requests
{
    using GIGXR.GMS.Models.Sessions.Responses;
    using Newtonsoft.Json.Linq;
    using System;

    public class UpdateSessionRequest
    {
        public UpdateSessionRequest(SessionDetailedView session)
        {
            SessionName = session.SessionName;
            Description = session.Description;
            ClassId =  session.ClassId;
            InstructorId = session.InstructorId;
            LessonDate = session.LessonDate;
            SessionStatus = session.SessionStatus;
            SessionPermission = session.SessionPermission;
            Locked = session.Locked;
            HmdJson = session.HmdJson;
            Saved = session.Saved;
            SessionNote = session.SessionNote != null ? session.SessionNote : "";
            SessionNoteVisible = session.SessionNoteVisible;
        }

        public string SessionName { get; set; } = null!;

        public string Description { get; set; } = "";

        public Guid? ClassId { get; set; }

        public Guid? InstructorId { get; set; }

        public DateTime? LessonDate { get; set; }

        public SessionStatus SessionStatus { get; set; }

        public SessionPermission SessionPermission { get; set; }

        public bool Locked { get; set; }

        public JObject HmdJson { get; set; } = null!;

        public bool Saved { get; set; }

        public string SessionNote { get; set; } = "";

        public bool SessionNoteVisible { get; set; }
    }
}