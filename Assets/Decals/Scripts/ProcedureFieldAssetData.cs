using GIGXR.Platform.Scenarios.GigAssets.Data;
using System;

[Serializable]
public class ProcedureFieldAssetData : BaseAssetData
{
    //The step number of the treatment plan
    public AssetPropertyDefinition<int> step;
    public AssetPropertyDefinition<int> usedOnStep;
}