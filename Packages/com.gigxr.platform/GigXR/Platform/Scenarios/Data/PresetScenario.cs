namespace GIGXR.Platform.ScenarioBuilder.Data
{
    using GIGXR.Platform.Scenarios.Data;
    using GIGXR.Platform.Scenarios.GigAssets;
    using GIGXR.Platform.Scenarios.GigAssets.Data;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cysharp.Threading.Tasks;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;
    using GIGXR.Platform.Scenarios;
    using Newtonsoft.Json.Linq;

    public abstract class HmdJsonProvider : ScriptableObject
    {
        public abstract string GetJson();

        public abstract JObject GetDictionary();
    }

    /// <summary>
    /// A <c>PresetScenario</c> is a GIGXR provided Scenario with the entire experience pre-scripted.
    /// </summary>
    /// <remarks>
    /// <c>PresetScenario</c> have limited end-user editing capabilities.
    /// </remarks>
    [CreateAssetMenu(fileName = "New Preset Scenario", menuName = "GIGXR/Scenarios/New Preset Scenario")]
    public class PresetScenario : HmdJsonProvider
    {
        /// <summary>
        /// The name of the Scenario.
        /// </summary>
        public string presetScenarioName;

        /// <summary>
        /// The Stages in the Scenario.
        /// </summary>
        public List<PresetStage> presetStages;

        /// <summary>
        /// Individual preset assets in the Scenario.
        /// These will be instantiated while loading the scenario.
        /// </summary>
        public List<PresetAsset> presetAssets;

        [HideInInspector]
        [JsonIgnore]
        public AssetPrefabDataScriptableObject assetDataPrefabReferences;

        /// <summary>
        /// Groups of assets types to load when loading the scenario.
        /// Overlapping types with presetAssets will not be re-loaded.
        /// Instantiations will not happen at load time. Instances
        /// of specified assets can be instantiated at run time.
        /// </summary>
        public List<PresetAssetTypeLoadBundle> presetAssetTypeLoadBundles;

        public List<string> presetRuleIds;

        /// <summary>
        /// Optional. Allows stages to diverge along differently named pathways.
        /// </summary>
        public List<PathwayData> pathwayData;

        public bool TryGetStage(string stageId, out PresetStage stage)
        {
            stage = presetStages.Where(presetStage => presetStage.stage.stageId == stageId).FirstOrDefault();

            return stage != null;
        }

        public void ResetRuntimeSharedValue(string presetAssetId)
        {
#if UNITY_EDITOR
            OnAllAssetTypeComponentsEditor(presetAssetId, atc => atc.ResetAllAssetPropertyDefintionSharedValueToDefault());
#endif
        }

        public void AddStageData(string presetAssetId, string stageId, IAssetPropertyDefinition? assetProperty = null, RuntimeStageInput stageData = null)
        {
#if UNITY_EDITOR
            if (stageData != null)
            {
                OnAllAssetTypeComponentsEditor(presetAssetId, atc => atc.AddKnownStageData(stageId, assetProperty.PropertyName, stageData), assetProperty);
            }
            else
            {
                OnAllAssetTypeComponentsEditor(presetAssetId, atc => atc.AddStageData(stageId, assetProperty?.PropertyName), assetProperty);
            }
#endif
        }

#if UNITY_EDITOR
        private void OnAllAssetTypeComponentsEditor(string presetAssetId, Action<IAssetTypeComponent> action, IAssetPropertyDefinition? assetProperty = null)
        {
            var asset = assetDataPrefabReferences.GetAsset(presetAssetId);

            var assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(asset);

            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(assetPath))
            {
                var assetTypeComponents = editingScope.prefabContentsRoot.GetComponents<IAssetTypeComponent>();

                if (assetProperty == null)
                {
                    foreach (var atc in assetTypeComponents)
                    {
                        action?.Invoke(atc);
                    }
                }
                else
                {
                    action?.Invoke(assetProperty.AttachedAssetTypeComponent);
                }
            }
        }
#endif

        public void RemoveStageData(string presetAssetId, string stageId)
        {
#if UNITY_EDITOR
            var asset = assetDataPrefabReferences.GetAsset(presetAssetId);

            var assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(asset);

            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(assetPath))
            {
                var assetTypeComponents = editingScope.prefabContentsRoot.GetComponents<IAssetTypeComponent>();

                foreach (var atc in assetTypeComponents)
                {
                    atc.RemoveStageData(stageId);
                }
            }
#endif
        }

        public void RemoveStageData(string presetAssetId, int stageIndex)
        {
#if UNITY_EDITOR
            var asset = assetDataPrefabReferences.GetAsset(presetAssetId);

            var assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(asset);

            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(assetPath))
            {
                var assetTypeComponents = editingScope.prefabContentsRoot.GetComponents<IAssetTypeComponent>();

                foreach (var atc in assetTypeComponents)
                {
                    atc.RemoveStageData(stageIndex);
                }
            }
#endif
        }

        /// <summary>
        /// Converts a <c>PresetScenario</c> into a runtime <c>Scenario</c> capable of being started.
        /// </summary>
        /// <returns>A <c>Scenario</c> ready to be started.</returns>
        public Scenario BuildScenario()
        {
            List<Asset> assetList;

            if (assetDataPrefabReferences == null)
            {
                assetList = presetAssets.Select(preset => new Asset
                {
                    assetTypeId = preset.AssetTypeId,
                    assetId = preset.assetId,
                    presetAssetId = preset.presetAssetId,
                    assetData = preset.assetData,
                }).ToList();
            }
            else
            {
                assetList = new List<Asset>();

                foreach (var asset in presetAssets)
                {
                    var presetAsset = assetDataPrefabReferences.GetAsset(asset.presetAssetId);

                    var assetMediator = presetAsset.GetComponent<IAssetMediator>();

                    IAssetTypeComponent[] assetTypeComponents = presetAsset.GetComponents<IAssetTypeComponent>();

                    // Go through all attached asset type components and map their data.
                    for (int i = 0; i < assetTypeComponents.Length; i++)
                    {
                        IAssetTypeComponent assetTypeComponent = assetTypeComponents[i];
                        assetTypeComponent.SendAssetData(assetMediator);
                    }

                    assetList.Add(new Asset
                    {
                        assetTypeId = asset.AssetTypeId,
                        assetId = asset.assetId,
                        presetAssetId = asset.presetAssetId,
                        assetData = assetMediator.SerializeToJson()
                    });
                }
            }

            var scenario = new Scenario
            {
                scenarioName = presetScenarioName,
                stages = presetStages.Select(preset => preset.stage).ToList(),
                assets = assetList,
                loadedAssetTypes = new List<string>(PresetAssetTypeLoadBundle.GetUniqueAssetReferences(presetAssetTypeLoadBundles)),
                presetStageMappings = presetStages.Select(presetStage => new PresetStageMapping
                {
                    presetStageId = presetStage.presetStageId,
                    stageId = presetStage.stage.stageId,
                }).ToList(),
                presetAssetMappings = presetAssets.Select(presetAsset => new PresetAssetMapping
                {
                    presetAssetId = presetAsset.presetAssetId,
                    assetId = presetAsset.assetId,
                }).ToList(),
                pathways = pathwayData
            };

            return scenario;
        }

        protected Guid NewGuid() => Guid.NewGuid();

        private void OnValidate()
        {
            SeedEmptyOrInvalidStageIds();
            SeedEmptyOrInvalidAssetIds();
        }

        private void SeedEmptyOrInvalidStageIds()
        {
            var stageIds = new HashSet<string>();
            foreach (PresetStage presetStage in presetStages)
            {
                if (presetStage.stage.stageId != ""
                    && !stageIds.Contains(presetStage.stage.stageId) &&
                    Guid.TryParse(presetStage.stage.stageId, out Guid id) && 
                    id != Guid.Empty)
                {
                    // Valid stageId.
                    stageIds.Add(presetStage.stage.stageId);
                    continue;
                }

                // Empty, duplicate, or invalid. Replace it.
                var stageId = NewGuid();
                presetStage.stage.StageId = stageId;
                stageIds.Add(presetStage.stage.stageId);
            }
        }

        private void SeedEmptyOrInvalidAssetIds()
        {
            var assetIds = new HashSet<string>();
            foreach (PresetAsset presetAsset in presetAssets)
            {
                if (presetAsset.assetId != "" &&
                    !assetIds.Contains(presetAsset.assetId) &&
                    Guid.TryParse(presetAsset.assetId, out _))
                {
                    // Valid assetId.
                    assetIds.Add(presetAsset.assetId);
                    continue;
                }

                // Empty, duplicate, or invalid. Replace it.
                var assetId = NewGuid();
                presetAsset.AssetId = assetId;
                assetIds.Add(presetAsset.assetId);
            }
        }

        public override string GetJson()
        {
            var scenario = BuildScenario();

            return JsonConvert.SerializeObject(scenario, Formatting.Indented);
        }

        public override JObject GetDictionary()
        {
            var scenario = BuildScenario();

            return JObject.FromObject(scenario);
        }
    }

    public class PresetStageComparer : IEqualityComparer<PresetStage>
    {
        public int GetHashCode(PresetStage stage)
        {
            if (stage == null)
            {
                return 0;
            }

            return stage.GetHashCode();
        }

        public bool Equals(PresetStage stage, PresetStage otherStage)
        {
            if (object.ReferenceEquals(stage, otherStage))
            {
                return true;
            }

            if (object.ReferenceEquals(stage, null) || object.ReferenceEquals(otherStage, null))
            {
                return false;
            }

            return stage.stage.stageId == otherStage.stage.stageId;
        }
    }
}