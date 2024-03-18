using System;

namespace GIGXR.Platform.Data
{
    /// <summary>
    /// Specifies the status of whether a user has participated in a session or not
    /// </summary>
    public enum SessionParticipantStatus
    {
        /// <summary>
        /// GMS: Once was added or invited to the session, no longer so.
        /// </summary>
        Removed = 0,
        /// <summary>
        /// GMS: The user explicitly has access to this session.
        /// </summary>
        Added = 1,
        /// <summary>
        /// GMS: "Added" and they have also been notified about it via email.
        /// </summary>
        Invited = 2,
        /// <summary>
        /// Actively in the session (colocated).
        /// </summary>
        InSessionColocated = 3,
        /// <summary>
        /// Actively in the session (remote).
        /// </summary>
        InSessionRemote = 4,
        /// <summary>
        /// Previously in the session, no longer so.
        /// </summary>
        Attended = 5
    }
}