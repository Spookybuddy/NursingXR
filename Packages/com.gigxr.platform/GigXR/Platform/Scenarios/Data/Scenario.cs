namespace GIGXR.Platform.Scenarios.Data
{
    using GigAssets.Data;
    using Newtonsoft.Json;
    using Stages.Data;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// No common methods or data for any scenario data, just essentially acts as a compile-time marker.
    /// </summary>
    public interface IScenarioData
    {
    }

    [Serializable]
    public class Scenario : IScenarioData
    {
        public string scenarioName;
        public List<Stage> stages;
        public List<Asset> assets;
        public List<string> loadedAssetTypes;
        public List<PathwayData> pathways;
        
        [Obsolete("Replaced by data-driven Rules.")]
        public List<PresetStageMapping> presetStageMappings;
        [Obsolete("Replaced by data-driven Rules.")]
        public List<PresetAssetMapping> presetAssetMappings;
    }

    [Serializable]
    public class PathwayData
    {
        public string pathwayDisplayName;

        // This should be changed only under extreme circumstances. Changing it will necessitate changing corresponding rules.
        public string pathwayPermanentID;

        public static PathwayData Create(string pathwayJson)
        {
            if(string.IsNullOrEmpty(pathwayJson))
                return null;
            return JsonConvert.DeserializeObject<PathwayData>(pathwayJson);
        }

        public static PathwayData DefaultPathway()
        {
            // TODO Do we want to externalize this name at some point?
            return new PathwayData()
            {
                pathwayDisplayName = "Default"
            };
        }
    }
}