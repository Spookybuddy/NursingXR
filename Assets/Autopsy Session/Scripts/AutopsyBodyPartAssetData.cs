using GIGXR.Platform.Scenarios.GigAssets.Data;
using System;
using UnityEngine;

[Serializable]
public class AutopsyBodyPartAssetData : BaseAssetData
{
    public AssetPropertyDefinition<BodySection> bodySection;
    public AssetPropertyDefinition<BodySystem> bodySystem;
    public AssetPropertyDefinition<int> systemLayer;
    public AssetPropertyDefinition<float> weight;
    public AssetPropertyDefinition<bool> inGravityZone;
}
