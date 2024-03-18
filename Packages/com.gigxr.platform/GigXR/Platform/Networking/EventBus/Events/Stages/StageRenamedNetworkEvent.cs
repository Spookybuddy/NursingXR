namespace GIGXR.Platform.Networking.EventBus.Events.Stages
{
    using ExitGames.Client.Photon;
    using Photon.Realtime;
    using System;

    /// <summary>
    /// Network event that is sent from the Host to all other users when a stage is renamed
    /// </summary>
    public class StageRenamedNetworkEvent : ICustomNetworkEvent
    {
        public Guid StageId { get; }
        public string NewName { get; }

        public RaiseEventOptions RaiseEventOptions => new RaiseEventOptions()
        {
            Receivers = ReceiverGroup.Others
        };

        public SendOptions SendOptions => SendOptions.SendReliable;

        public StageRenamedNetworkEvent(Guid id, string newName)
        {
            StageId = id;
            NewName = newName;
        }

        public override string ToString()
        {
            return $"{StageId}'s New Name: {NewName}";
        }
    }

    public class StageRenamedNetworkEventSerializer : ICustomNetworkEventSerializer<StageRenamedNetworkEvent>
    {
        public object[] Serialize(StageRenamedNetworkEvent @event) => new object[] { @event.StageId, @event.NewName };

        public StageRenamedNetworkEvent Deserialize(object[] data) => new StageRenamedNetworkEvent((Guid)data[0], (string)data[1]);
    }
}