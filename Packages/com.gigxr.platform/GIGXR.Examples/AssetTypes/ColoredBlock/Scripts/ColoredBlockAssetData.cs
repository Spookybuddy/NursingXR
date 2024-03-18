namespace GIGXR.Examples.AssetTypes.ColoredBlock.Scripts
{
    using GIGXR.Platform.Scenarios.GigAssets.Data;
    using System;
    using UnityEngine;

    [Serializable]
    public class ColoredBlockAssetData : BaseAssetData
    {
        public AssetPropertyDefinition<Color> color;
    }
}