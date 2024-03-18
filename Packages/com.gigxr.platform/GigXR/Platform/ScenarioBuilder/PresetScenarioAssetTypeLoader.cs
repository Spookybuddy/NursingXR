namespace GIGXR.Platform.ScenarioBuilder
{
    using Cysharp.Threading.Tasks;
    using Data;
    using Scenarios.GigAssets.Loader;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// An implementation of <c>IAssetTypeLoader</c> that loads asset types from a <c>PresetScenario</c>.
    /// </summary>
    public class PresetScenarioAssetTypeLoader : IAssetTypeLoader
    {
        private readonly PresetScenario presetScenario;
        private readonly Dictionary<string, GameObject> loadedAssetTypes = new Dictionary<string, GameObject>();

        public PresetScenarioAssetTypeLoader(PresetScenario presetScenario)
        {
            this.presetScenario = presetScenario;
        }

        public IReadOnlyDictionary<string, GameObject> LoadedAssetTypes => loadedAssetTypes;

        public UniTask LoadAssetTypesAsync(ISet<string> assetTypeIds)
        {
            foreach (string assetTypeId in assetTypeIds)
            {
                if (string.IsNullOrWhiteSpace(assetTypeId))
                {
                    continue;
                }

                foreach (PresetAsset presetAsset in presetScenario.presetAssets)
                {
                    if (presetAsset.assetTypePrefab.AssetTypeId == assetTypeId)
                    {
                        // Debug.Log($"{nameof(PresetScenarioAssetTypeLoader)} Loaded: {assetTypeId}");
                        loadedAssetTypes.Add(assetTypeId, presetAsset.assetTypePrefab.gameObject);
                        break;
                    }
                }
            }

            return UniTask.CompletedTask;
        }

        public UniTask UnloadAllAssetTypesAsync()
        {
            loadedAssetTypes.Clear();

            return UniTask.CompletedTask;
        }
    }
}