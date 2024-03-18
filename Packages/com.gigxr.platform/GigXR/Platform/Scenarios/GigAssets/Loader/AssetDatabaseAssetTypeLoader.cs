#if UNITY_EDITOR // had to wrap this because it was failing builds
namespace GIGXR.Platform
{
    using Cysharp.Threading.Tasks;
    using Scenarios.GigAssets.Loader;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// An implementation of <c>IAssetTypeLoader</c> that loads asset types from the AssetDatabase. Use only in the Editor.
    /// </summary>
    public class AssetDatabaseAssetTypeLoader : IAssetTypeLoader
    {
        private readonly Dictionary<string, GameObject> loadedAssetTypes = new Dictionary<string, GameObject>();

        public AssetDatabaseAssetTypeLoader()
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
                
                loadedAssetTypes[assetTypeId] = AssetDatabase.LoadAssetAtPath(assetTypeId, typeof(GameObject)) as GameObject;
                // TODO
                // gameObject;
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
#endif