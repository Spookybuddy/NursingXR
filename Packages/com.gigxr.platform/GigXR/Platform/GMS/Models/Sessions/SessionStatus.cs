namespace GIGXR.GMS.Models.Sessions
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SessionStatus
    {
        Invalid = 0,
        Ended = 1,
        InProgress = 2,
        Archived = 3,
        New = 4
    }
}