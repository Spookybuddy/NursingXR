namespace GIGXR.Platform.Scenarios.GigAssets.EventArgs
{
    using System;
    using UnityEngine;
    using Photon.Pun;


    public class AssetInstantiatedEventArgs : EventArgs
    {
        public string AssetTypeId { get; }

        public Guid AssetId { get; }

        public string PresetAssetId { get; }

        public Vector3 Position { get; }

        public Quaternion Rotation { get; }

        // true if the asset was instantiated after loading a scenario
        public bool IsRuntimeInstantiation { get; }

        // true if the asset should not be saved with the session
        public bool RuntimeOnly { get; }

        public string AssetData;

        public PhotonView AssetPhotonView { get; }

        // true if IsRuntimeInstantiation is true and the local player created the asset
        // false if the asset was instantiated by another player
        public bool RuntimeInstantiationOriginateLocally { get; }

        public AssetInstantiatedEventArgs(
            string assetTypeId, Guid id, string presetAssetId, Vector3 position, Quaternion rotation, bool isRuntimeInstantiation, 
            bool runtimeOnly, string assetData, PhotonView assetPhotonView, bool runtimeInstantiationOriginatedLocally)
        {
            AssetTypeId = assetTypeId;
            AssetId = id;
            PresetAssetId = presetAssetId;
            Position = position;
            Rotation = rotation;
            IsRuntimeInstantiation = isRuntimeInstantiation;
            RuntimeOnly = runtimeOnly;
            AssetData = assetData;
            AssetPhotonView = assetPhotonView;
            RuntimeInstantiationOriginateLocally = runtimeInstantiationOriginatedLocally;
        }
    }
}