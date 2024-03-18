using System;

namespace GIGXR.GMS.Models.Sessions
{
    public class CreateClientAppVersionRequest
    {
        public Guid ClientAppId { get; set; }

        public string Version { get; set; }

        public CreateClientAppVersionRequest(Guid clientAppId, string version)
        {
            ClientAppId = clientAppId;
            Version = version;
        }
    }
}