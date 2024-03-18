using System;
using GIGXR.GMS.Models.Accounts;
using Newtonsoft.Json.Linq;

namespace GIGXR.GMS.Models.Sessions
{
    public class ClientAppManifestDetailedView
    {
        public ClientAppManifestDetailedView(
            Guid clientAppId,
            JObject manifest,
            DateTime createdOn,
            Guid createdById,
            AccountBasicView createdBy,
            DateTime modifiedOn,
            Guid modifiedById,
            AccountBasicView modifiedBy)
        {
            ClientAppId = clientAppId;
            Manifest = manifest;
            CreatedOn = createdOn;
            CreatedById = createdById;
            CreatedBy = createdBy;
            ModifiedOn = modifiedOn;
            ModifiedById = modifiedById;
            ModifiedBy = modifiedBy;
        }

        public Guid ClientAppId { get; }

        public JObject Manifest { get; }

        public DateTime CreatedOn { get; }

        public Guid CreatedById { get; }

        public AccountBasicView CreatedBy { get; }

        public DateTime ModifiedOn { get; }

        public Guid ModifiedById { get; }

        public AccountBasicView ModifiedBy { get; }
    }
}