namespace GIGXR.Platform.Networking.EventBus.Events
{
    using ExitGames.Client.Photon;
    using Photon.Realtime;
    using System;
    using UnityEngine;

    public class InstantiateAssetNetworkEvent : ICustomNetworkEvent
    {
        public string AssetTypeId { get; }

        public Guid AssetId { get; }

        public string PresetAssetTypeId { get; }

        public Vector3 Position { get; }

        public Quaternion Rotation { get; }

        // true if the asset was instantiated after loading the scenario
        public bool IsRuntimeInstantiation { get; }

        // true if the asset should be omitted from saves
        public bool RuntimeOnly { get; }
        public string AssetData { get; }

        public InstantiateAssetNetworkEvent(string assetTypeId, Guid id, string presetAssetTypeId, Vector3 position, Quaternion rotation, bool isRuntimeInstantiation, bool runtimeOnly, string assetData)
        {
            AssetTypeId = assetTypeId;
            AssetId = id;
            PresetAssetTypeId = presetAssetTypeId;
            Position = position;
            Rotation = rotation;
            IsRuntimeInstantiation = isRuntimeInstantiation;
            RuntimeOnly = runtimeOnly;
            AssetData = assetData;
        }

        public RaiseEventOptions RaiseEventOptions => new RaiseEventOptions()
        {
            Receivers = ReceiverGroup.Others
        };

        public SendOptions SendOptions => SendOptions.SendReliable;

        public override string ToString()
        {
            return $"Instantiate {AssetTypeId} with ID {AssetId} at Position: {Position} Rotation: {Rotation}";
        }
    }

    public class InstantiateAssetNetworkEventSerializer : ICustomNetworkEventSerializer<InstantiateAssetNetworkEvent>
    {
        public object[] Serialize(InstantiateAssetNetworkEvent @event) => new object[] { 
            @event.AssetTypeId, @event.AssetId, @event.PresetAssetTypeId, 
            @event.Position, @event.Rotation, @event.IsRuntimeInstantiation, 
            @event.RuntimeOnly, @event.AssetData
        };

        public InstantiateAssetNetworkEvent Deserialize(object[] data) => new InstantiateAssetNetworkEvent(
            (string)data[0], (Guid)data[1], (string)data[2], 
            (Vector3)data[3], (Quaternion)data[4], (bool)data[5],
            (bool)data[6], (string)data[7]
        );
    }
}