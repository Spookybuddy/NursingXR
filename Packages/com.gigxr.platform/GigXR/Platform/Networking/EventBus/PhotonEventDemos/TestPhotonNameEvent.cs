using ExitGames.Client.Photon;
using Photon.Realtime;

namespace GIGXR.Platform.Networking.EventBus.PhotonEventDemos
{
    public class TestPhotonNameEvent : ICustomNetworkEvent
    {
        public string FirstName { get; }
        public string LastName { get; }

        public TestPhotonNameEvent(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;

            RaiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.All,};
        }

        public override string ToString()
        {
            return $"FirstName: {FirstName}, LastName: {LastName}";
        }

        public RaiseEventOptions RaiseEventOptions { get; }
        public SendOptions SendOptions { get; } = SendOptions.SendUnreliable;
    }

    public class TestPhotonNameEventSerializer : ICustomNetworkEventSerializer<TestPhotonNameEvent>
    {
        public object[] Serialize(TestPhotonNameEvent @event)
        {
            return new object[] {@event.FirstName, @event.LastName};
        }

        public TestPhotonNameEvent Deserialize(object[] data)
        {
            return new TestPhotonNameEvent((string)data[0], (string)data[1]);
        }
    }
}