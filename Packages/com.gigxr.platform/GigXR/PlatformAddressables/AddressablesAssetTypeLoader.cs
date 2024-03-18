namespace GIGXR.Platform.Scenarios.GigAssets.Loader
{
    using Cysharp.Threading.Tasks;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;

    /// <summary>
    /// Loads AssetTypes into memory via Addressables.
    /// </summary>
    public class AddressablesAssetTypeLoader : IAssetTypeLoader
    {
        private readonly Dictionary<string, GameObject> loadedAssetTypes = new Dictionary<string, GameObject>();

        public IReadOnlyDictionary<string, GameObject> LoadedAssetTypes => loadedAssetTypes;

        public async UniTask LoadAssetTypesAsync(ISet<string> assetTypeIds)
        {
            // get the new asset type ids from the input.
            // put them in loadedAssetTypes with placeholder values to ensure they are not loaded again.
            lock (loadedAssetTypes)
            {
                assetTypeIds.ExceptWith(loadedAssetTypes.Keys);
                foreach (var assetTypeId in assetTypeIds)
                {
                    loadedAssetTypes[assetTypeId] = null;
                }
            }

            // track number of total assets to load, and number successfully loaded
            int assetsToLoad = assetTypeIds.Count;
            int assetsLoaded = 0;

            // Convert to array for multiple enumeration and indexing.
            string[] assetTypeIdArray = assetTypeIds
                .Where(assetTypeId => !string.IsNullOrWhiteSpace(assetTypeId))
                .ToArray();

            for (var i = 0; i < assetTypeIdArray.Length; i++)
            {
                string assetTypeId = assetTypeIdArray[i];

                var asyncloadAssetOperation = Addressables.LoadAssetAsync<GameObject>(assetTypeId);

                asyncloadAssetOperation.Completed += Completed(async (gameObject) =>
                {
                    if (!IsGameObjectAnAssetType(gameObject))
                    {
                        Debug.LogWarning($"Failed to load asset type with ID {assetTypeId}.");
                        lock (loadedAssetTypes)
                        {
                            if (!loadedAssetTypes.Remove(assetTypeId))
                            {
                                Debug.LogError("Error in asset load logic; failed to load an asset that was not registered to load");
                            }
                        }
                        assetsToLoad--;
                        return;
                    }

                    loadedAssetTypes[assetTypeId] = gameObject;

                    // recursively load any assets that are dependencies of this new asset and might be instantiated down the line
                    // repeated ids will be filtered out at the start of the recursive call
                    IAssetMediator assetMediator = gameObject.GetComponent<IAssetMediator>();
                    await LoadAssetTypesAsync(new HashSet<string>(assetMediator.AssetTypeDependencyIds));

                    assetsLoaded++;
                });
            }

            var loadingTimeout = new CancellationTokenSource();
            loadingTimeout.CancelAfter(60000);

            await UniTask.WaitUntil(() => assetsLoaded == assetsToLoad, 
                                    PlayerLoopTiming.Update, 
                                    loadingTimeout.Token);

            loadingTimeout.Dispose();
        }

        /// <summary>
        /// Releases all saved Addressable assets.
        /// Releases async operation handles for Addressables as well. 
        /// </summary>
        /// <returns></returns>
        public UniTask UnloadAllAssetTypesAsync()
        {
            foreach (GameObject assetType in loadedAssetTypes.Values)
            {
                Release(assetType);
            }

            loadedAssetTypes.Clear();

            Resources.UnloadUnusedAssets();

            return UniTask.CompletedTask;
        }

        private Action<AsyncOperationHandle<GameObject>> Completed(Action<GameObject> completedLoadingAction)
        {
            return (asyncOperation) => 
            {
                asyncOperation.Completed -= Completed(completedLoadingAction);
                completedLoadingAction?.Invoke(asyncOperation.Result); 
            };
        }

        protected void Release
        (
            GameObject gameObject
        ) => Addressables.Release(gameObject);

        protected bool IsGameObjectAnAssetType
        (
            GameObject gameObject
        )
        {
            if(gameObject != null)
                return gameObject.GetComponent<IAssetMediator>() != null;
            
            return false;
        }
    }
}