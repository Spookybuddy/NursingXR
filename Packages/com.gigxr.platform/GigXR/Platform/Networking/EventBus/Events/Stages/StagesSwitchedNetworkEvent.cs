namespace GIGXR.Platform.Networking.EventBus.Events.Stages
{
    using ExitGames.Client.Photon;
    using GIGXR.Platform.Scenarios;
    using Photon.Realtime;
    using System;

    /// <summary>
    /// Network event that is sent from the Host to all other users when the session moves to another stage
    /// </summary>
    public class StagesSwitchedNetworkEvent : ICustomNetworkEvent
    {
        public Guid StageID { get; }

        public ScenarioSyncData SyncData { get; }
        
        public StagesSwitchedNetworkEvent(Guid id, ScenarioSyncData syncData)
        {
            StageID = id;
            SyncData = syncData;
        }

        public RaiseEventOptions RaiseEventOptions => new RaiseEventOptions()
        {
            Receivers = ReceiverGroup.Others
        };

        public SendOptions SendOptions => SendOptions.SendReliable;

        public override string ToString()
        {
            return $"Stages Switched";
        }
    }

    public class StagesSwitchedNetworkEventSerializer : ICustomNetworkEventSerializer<StagesSwitchedNetworkEvent>
    {
        public object[] Serialize(StagesSwitchedNetworkEvent @event) => new object[] { @event.StageID, @event.SyncData };

        public StagesSwitchedNetworkEvent Deserialize(object[] data) => new StagesSwitchedNetworkEvent((Guid)data[0], (ScenarioSyncData)data[1]);
    }
}