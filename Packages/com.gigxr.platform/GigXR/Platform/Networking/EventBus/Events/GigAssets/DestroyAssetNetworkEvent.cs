namespace GIGXR.Platform.Networking.EventBus.Events
{
    using ExitGames.Client.Photon;
    using Photon.Realtime;
    using System;

    public class DestroyAssetNetworkEvent : ICustomNetworkEvent
    {
        public Guid AssetId { get; }

        public DestroyAssetNetworkEvent(Guid assetId)
        {
            AssetId = assetId;
        }

        public RaiseEventOptions RaiseEventOptions => new RaiseEventOptions()
        {
            Receivers = ReceiverGroup.Others
        };

        public SendOptions SendOptions => SendOptions.SendReliable;

        public override string ToString()
        {
            return $"Destroy {AssetId}";
        }
    }

    public class DestroyAssetNetworkEventSerializer : ICustomNetworkEventSerializer<DestroyAssetNetworkEvent>
    {
        public object[] Serialize(DestroyAssetNetworkEvent @event) => new object[] {
            @event.AssetId.ToString(),
        };

        public DestroyAssetNetworkEvent Deserialize(object[] data) => new DestroyAssetNetworkEvent(
            Guid.Parse((string)data[0])
        );
    }
}