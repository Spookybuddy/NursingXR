namespace GIGXR.GMS.Models.Sessions
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SessionPermission
    {
        Private = 0,
        OpenToInvitedParticipants = 1,
        OpenToClass = 2,
        OpenToDepartment = 3,
        OpenToInstitution = 4,
        OpenToInstitutionNonStudents = 5
    }
}