using GIGXR.Platform.Scenarios.GigAssets.Data;
using System;
using UnityEngine;

[Serializable]
public class SprayAssetData : BaseAssetData
{
    // Properties of type in that can have callbacks defined for it when it changes for networking
    public AssetPropertyDefinition<bool> activateSpray;
}
