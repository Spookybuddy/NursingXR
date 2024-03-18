namespace GIGXR.GMS.Models.Sessions.Requests
{
    using Newtonsoft.Json;
    using Platform.Data;

    public class UpdateSessionParticipantRequest
    {
        [JsonRequired] public SessionParticipantStatus SessionParticipantStatus { get; set; }
    }
}