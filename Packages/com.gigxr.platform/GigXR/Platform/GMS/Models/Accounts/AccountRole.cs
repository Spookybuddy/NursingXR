namespace GIGXR.GMS.Models.Accounts
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum AccountRole
    {
        Invalid = 0,
        Student = 1,
        Instructor = 2,
        DepartmentAdmin = 3,
        InstitutionAdmin = 4,
        GigXrAdmin = 5
    }
}