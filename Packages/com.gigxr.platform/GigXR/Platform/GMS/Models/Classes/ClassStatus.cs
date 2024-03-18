namespace GIGXR.GMS.Models.Classes
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ClassStatus
    {
        Inactive = 0,
        Active = 1
    }
}