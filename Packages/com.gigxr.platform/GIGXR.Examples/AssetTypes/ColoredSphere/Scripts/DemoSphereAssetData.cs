using GIGXR.Platform.Scenarios.GigAssets.Data;
using System;
using UnityEngine;

[Serializable]
public class DemoSphereAssetData : BaseAssetData
{
    public AssetPropertyDefinition<Color> color;

    public AssetPropertyDefinition<Vector3> validatedVector;
    
    public AssetPropertyDefinition<int> randomIntToDemoConditions;
}
