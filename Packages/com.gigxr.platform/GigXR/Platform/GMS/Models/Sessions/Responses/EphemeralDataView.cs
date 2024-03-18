namespace GIGXR.GMS.Models.Sessions.Responses
{
    using Newtonsoft.Json.Linq;
    using System;

    public class EphemeralDataView
    {
        public Guid SessionEphemeralDataId { get; set; }

        public JObject EphemeralData { get; set; }
    }
}