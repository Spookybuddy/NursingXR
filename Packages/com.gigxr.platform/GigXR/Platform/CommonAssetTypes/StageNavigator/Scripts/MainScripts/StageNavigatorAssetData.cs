namespace GIGXR.Platform.CommonAssetTypes.StageNavigator.Scripts.MainScripts
{
    using GIGXR.Platform.Scenarios.GigAssets.Data;
    using GIGXR.Platform.Scenarios.Stages.Data;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// AssetData for the StageNavigator that sets up the visuals and text needed
    /// to navigate stages
    /// </summary>
    [Serializable]
    public class StageNavigatorAssetData : BaseAssetData
    {
        public AssetPropertyDefinition<string> headerText;
        public AssetPropertyDefinition<string> contentText;
        public AssetPropertyDefinition<string> leftButtonText;
        public AssetPropertyDefinition<string> rightButtonText;
        public AssetPropertyDefinition<bool> showStages;
        public AssetPropertyDefinition<int> stageIndex;
        public AssetPropertyDefinition<List<Stage>> stageList;
        public AssetPropertyDefinition<int> maxStageIndex; 
    }
}