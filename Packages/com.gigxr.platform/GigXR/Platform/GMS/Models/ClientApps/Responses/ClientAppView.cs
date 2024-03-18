namespace GIGXR.GMS.Models.ClientApps.Responses
{
    using System;

    public class ClientAppView
    {
        public Guid ClientAppId { get; set; }

        public string ClientAppName { get; set; } = null!;
    }
}