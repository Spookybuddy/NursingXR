namespace GIGXR.Platform.Networking.EventBus.Events.Sessions
{
    using ExitGames.Client.Photon;
    using Photon.Realtime;
    using UnityEngine;

    /// <summary>
    /// A networked event that is sent from the MasterClient to all other users
    /// when a session is renamed
    /// </summary>
    public class SessionRenamedNetworkEvent : ICustomNetworkEvent
    {
        public string NewName { get; private set; }

        public RaiseEventOptions RaiseEventOptions => new RaiseEventOptions()
        {
            Receivers = ReceiverGroup.Others
        };

        public SendOptions SendOptions => SendOptions.SendReliable;

        public SessionRenamedNetworkEvent(string newName)
        {
            NewName = newName;
        }

        public override string ToString()
        {
            return $"Session Renamed: {NewName}";
        }
    }

    public class SessionRenamedNetworkEventSerializer : ICustomNetworkEventSerializer<SessionRenamedNetworkEvent>
    {
        public object[] Serialize(SessionRenamedNetworkEvent @event) => new object[] { @event.NewName };

        public SessionRenamedNetworkEvent Deserialize(object[] data) => new SessionRenamedNetworkEvent((string)data[0]);
    }
}