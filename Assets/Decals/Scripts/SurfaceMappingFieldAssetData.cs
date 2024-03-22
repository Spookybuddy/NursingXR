using GIGXR.Platform.Scenarios.GigAssets.Data;
using UnityEngine;
using System;

[Serializable]
public class SurfaceMappingFieldAssetData : BaseAssetData
{
    //Mesh data networked
    public AssetPropertyDefinition<Mesh> decal;
}