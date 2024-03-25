using GIGXR.Platform.Scenarios.GigAssets.Data;
using System;

[Serializable]
public class IncrementableNumberFieldAssetData : BaseAssetData
{
    //Value
    public AssetPropertyDefinition<int> currentValue;

    //Constraints
    public AssetPropertyDefinition<int> minValue;
    public AssetPropertyDefinition<int> maxValue;
}