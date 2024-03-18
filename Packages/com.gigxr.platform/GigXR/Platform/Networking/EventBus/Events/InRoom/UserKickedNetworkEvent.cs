namespace GIGXR.Platform.Sessions
{
    using ExitGames.Client.Photon;
    using GIGXR.Platform.Networking.EventBus;
    using Photon.Realtime;
    using System;

    public class UserKickedNetworkEvent : ICustomNetworkEvent
    {
        public Guid UserID { get; }

        public UserKickedNetworkEvent(Guid userID)
        {
            UserID = userID;
        }

        public RaiseEventOptions RaiseEventOptions => new RaiseEventOptions
        {
            Receivers = ReceiverGroup.Others
        };

        public SendOptions SendOptions => SendOptions.SendReliable;
    }

    public class UserKickedNetworkEventSerializer : ICustomNetworkEventSerializer<UserKickedNetworkEvent>
    {
        public object[] Serialize(UserKickedNetworkEvent @event) => new object[] { @event.UserID };

        public UserKickedNetworkEvent Deserialize(object[] data) => new UserKickedNetworkEvent((Guid)data[0]);
    }
}