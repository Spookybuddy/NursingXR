namespace GIGXR.Platform.AppEvents.Events.Session
{
    using GIGXR.Platform.Core.User;
    using System;

    /// <summary>
    /// Event that is sent out when a user's profile should be opened up.
    /// </summary>
    public class OpenUserProfileEvent : BaseSessionStatusChangeEvent
    {
        public Guid UserId { get; }

        public UserCard UserCardReference { get; }

        public OpenUserProfileEvent(Guid userId, UserCard userCard)
        {
            UserId = userId;
            UserCardReference = userCard;
        }
    }
}