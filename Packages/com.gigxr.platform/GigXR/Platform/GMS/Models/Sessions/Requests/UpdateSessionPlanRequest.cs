namespace GIGXR.GMS.Models.Sessions.Requests
{
    using Newtonsoft.Json.Linq;
    using System;

    public class UpdateSessionPlanRequest
    {
        public string SessionName { get; set; } = null!;

        public string Description { get; set; } = "";

        public Guid? ClassId { get; set; }

        public Guid? InstructorId { get; set; }

        public SessionStatus SessionStatus { get; set; }

        public SessionPermission SessionPermission { get; set; }

        public JObject HmdJson { get; set; } = null!;
    }
}