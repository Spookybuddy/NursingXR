namespace GIGXR.GMS.Models.Sessions.Requests
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class CreateEphemeralDataRequest
    {
        public JObject EphemeralData { get; set; } = null!;

        public CreateEphemeralDataRequest(JObject ephemeralData)
        {
            EphemeralData = ephemeralData;
        }
    }
}