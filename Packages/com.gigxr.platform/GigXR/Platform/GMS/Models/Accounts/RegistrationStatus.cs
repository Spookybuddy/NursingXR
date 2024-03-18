namespace GIGXR.GMS.Models.Accounts
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum RegistrationStatus
    {
        Invalid = 0,
        Registered = 1,
        Invited = 2,
        Added = 3
    }
}