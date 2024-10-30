using GIGXR.Platform.Scenarios.GigAssets.Data;
using System;
using UnityEngine;

[Serializable]
public class AutopsyOrganizerAssetData : BaseAssetData
{
    public AssetPropertyDefinition<BodySystem> curSystem;
    public AssetPropertyDefinition<BodySection> curSection;
    public AssetPropertyDefinition<int> outerLayer, inner1Layer, inner2Layer;
    public AssetPropertyDefinition<bool> notSetLayers, toggleResetAll, toggleResetActive;
}
