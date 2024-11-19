using GIGXR.Platform.Scenarios.GigAssets.Data;
using System;
using UnityEngine;

[Serializable]
public class CatheterSiteAssetData : BaseAssetData
{
    public AssetPropertyDefinition<float> oldTegadermSliderValue;
    public AssetPropertyDefinition<bool> notHoldingDownCatheter;
}
