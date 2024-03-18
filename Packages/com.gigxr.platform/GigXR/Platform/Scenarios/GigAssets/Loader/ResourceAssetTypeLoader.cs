namespace GIGXR.Platform
{
    using Cysharp.Threading.Tasks;
    using Scenarios.GigAssets.Loader;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// An implementation of <c>IAssetTypeLoader</c> that loads asset types from a <c>PresetScenario</c>.
    /// </summary>
    public class ResourceAssetTypeLoader : IAssetTypeLoader
    {
        private readonly List<string> prefabPathList = new List<string>();
        private readonly Dictionary<string, GameObject> loadedAssetTypes = new Dictionary<string, GameObject>();

        public ResourceAssetTypeLoader()
        {
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

                // TODO
                if (prefabPathList.Contains(assetTypeId))
                {
                    // Debug.Log($"{nameof(PresetScenarioAssetTypeLoader)} Loaded: {assetTypeId}");
                    //loadedAssetTypes.Add(assetTypeId, presetAsset.assetTypePrefab.gameObject);
                    break;
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