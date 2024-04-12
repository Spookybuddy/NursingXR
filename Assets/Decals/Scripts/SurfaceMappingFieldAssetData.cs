using GIGXR.Platform.Scenarios.GigAssets.Data;
using System;
using UnityEngine;

[Serializable]
public class SurfaceMappingFieldAssetData : BaseAssetData
{
    //Mesh does not seem to like to network
    [Tooltip("Network a variable and update mesh when it changes")]
    public AssetPropertyDefinition<bool> meshVersion;
}