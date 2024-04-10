using GIGXR.Platform.Scenarios.GigAssets.Data;
using System;

[Serializable]
public class SurfaceMappingFieldAssetData : BaseAssetData
{
    //Mesh does not seem to like to network
    //Network a variable and update mesh individually when it changes
    public AssetPropertyDefinition<int> updateMesh; 
}