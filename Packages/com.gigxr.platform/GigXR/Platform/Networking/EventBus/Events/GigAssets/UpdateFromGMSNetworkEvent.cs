namespace GIGXR.Platform.Networking.EventBus.Events
{
    using ExitGames.Client.Photon;
    using GIGXR.Platform.Scenarios;
    using GIGXR.Platform.Scenarios.Data;
    using Photon.Pun;
    using Photon.Realtime;
    using System;

    public class UpdateFromGMSNetworkEvent : ICustomNetworkEvent
    {
        public int TimeStamp { get; }

        public Guid SessionId { get; }

        public Guid HostId { get; }

        public Guid StageId { get; }

        public int TargetActor { get; }

        public ScenarioSyncData ScenarioSyncData { get; }

        public UpdateFromGMSNetworkEvent(Guid sessionId, Guid hostId, Guid stageId, ScenarioSyncData scenarioTimerData)
        {
            TimeStamp = PhotonNetwork.ServerTimestamp;
            SessionId = sessionId;
            HostId = hostId;
            TargetActor = -1;
            StageId = stageId;
            ScenarioSyncData = scenarioTimerData;

            _raiseEventOptionss = new RaiseEventOptions()
            {
                Receivers = ReceiverGroup.Others
            };
        }

        public UpdateFromGMSNetworkEvent(Guid sessionId, int targetActor, Guid hostId, Guid stageId, ScenarioSyncData scenarioTimerData)
        {
            TimeStamp = PhotonNetwork.ServerTimestamp;
            SessionId = sessionId;
            HostId = hostId;
            TargetActor = targetActor;
            StageId = stageId;
            ScenarioSyncData = scenarioTimerData;

            // This makes sure only the new user who joined the session will get this message
            _raiseEventOptionss = new RaiseEventOptions()
            {
                Receivers = ReceiverGroup.Others,
                TargetActors = new[] { TargetActor }
            };
        }

        public UpdateFromGMSNetworkEvent(int timeStamp, Guid sessionId, int targetActor, Guid hostId, Guid stageId, ScenarioSyncData scenarioTimerData)
        {
            TimeStamp = timeStamp;
            SessionId = sessionId;
            HostId = hostId;
            TargetActor = targetActor;
            StageId = stageId;
            ScenarioSyncData = scenarioTimerData;

            // This makes sure only the new user who joined the session will get this message
            _raiseEventOptionss = new RaiseEventOptions()
            {
                Receivers = ReceiverGroup.Others,
                TargetActors = new[] { TargetActor }
            };
        }

        public SendOptions SendOptions => SendOptions.SendReliable;

        private RaiseEventOptions _raiseEventOptionss;

        public RaiseEventOptions RaiseEventOptions { get => _raiseEventOptionss; }

        public override string ToString()
        {
            return $"Actor {TargetActor} is updating {SessionId} from GMS";
        }
    }

    public class UpdateFromGMSNetworkEventSerializer : ICustomNetworkEventSerializer<UpdateFromGMSNetworkEvent>
    {
        public object[] Serialize(UpdateFromGMSNetworkEvent @event) => new object[] { @event.TimeStamp, @event.SessionId, @event.TargetActor, @event.HostId, @event.StageId, @event.ScenarioSyncData };

        public UpdateFromGMSNetworkEvent Deserialize(object[] data) => new UpdateFromGMSNetworkEvent((int)data[0], (Guid)data[1], (int)data[2], (Guid)data[3], (Guid)data[4], (ScenarioSyncData)data[5]);
    }
}