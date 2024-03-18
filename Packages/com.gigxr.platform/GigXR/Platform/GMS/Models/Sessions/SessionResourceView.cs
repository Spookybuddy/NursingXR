using System;

namespace GIGXR.GMS.Models.Sessions
{
    public class SessionResourceView
    {
        public SessionResourceView(Guid resourceId)
        {
            ResourceId = resourceId;
        }

        /// <summary>
        /// The resourceId of a resource.
        /// </summary>
        public Guid ResourceId { get; }
    }
}