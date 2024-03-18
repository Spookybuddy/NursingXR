namespace GIGXR.Platform.Scenarios.GigAssets.Loader
{
    using Cysharp.Threading.Tasks;
    using GIGXR.Platform.Utilities;
    using Microsoft.MixedReality.Toolkit;
    using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Loads Game Objects into memory via Addressables.
    /// </summary>
    public class AddressablesGameObjectLoader : IAddressablesGameObjectLoader
    {
        private readonly Dictionary<string, GameObject> loadedGameObjects
            = new Dictionary<string, GameObject>();

        public IReadOnlyDictionary<string, GameObject> LoadedGameObjects => loadedGameObjects;

        private IGigAssetManager AssetManager { get; set; }

        public void SetManager(IGigAssetManager asssetManager)
        {
            AssetManager = asssetManager;
        }

        /// <summary>
        /// Loads one game object.
        /// </summary>
        /// <param name="addressableKey"></param>
        public async UniTask LoadGameObjectAsync(string addressableKey)
        {
            await LoadGameObjectsAsync(new HashSet<string>(new[] { addressableKey }));
        }

        /// <summary>
        /// Loads several game objects.
        /// </summary>
        /// <param name="addressableKeys"></param>
        public async UniTask LoadGameObjectsAsync(ISet<string> addressableKeys)
        {
            // get the new asset type ids from the input.
            // put them in loadedGameObjects with placeholder values to ensure they are not loaded again.
            lock (loadedGameObjects)
            {
                addressableKeys.ExceptWith(loadedGameObjects.Keys);

                foreach (var assetTypeId in addressableKeys)
                {
                    loadedGameObjects[assetTypeId] = null;
                }
            }

            // track number of total assets to load, and number successfully loaded
            int assetsToLoad = addressableKeys.Count;
            int assetsLoaded = 0;

            // Convert to array for multiple enumeration and indexing.
            string[] addressableKeyArray = addressableKeys.ToArray();

            for (var i = 0; i < addressableKeyArray.Length; i++)
            {
                string assetTypeId = addressableKeyArray[i];

                var asyncloadAssetOperation = Addressables.LoadAssetAsync<GameObject>(assetTypeId);

                asyncloadAssetOperation.Completed += Completed
                    (async (gameObject) =>
                    {
                        loadedGameObjects[assetTypeId] = gameObject;

                        assetsLoaded++;
                    });
            }

            var loadingTimeout = new CancellationTokenSource();
            loadingTimeout.CancelAfter(60000);

            await UniTask.WaitUntil
                (() => assetsLoaded == assetsToLoad,
                 PlayerLoopTiming.Update,
                 loadingTimeout.Token);

            loadingTimeout.Dispose();
        }

        /// <summary>
        /// Instantiates a game object using the addressable key. 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="spawnTransform"></param>
        /// <returns></returns>
        public async UniTask<GameObject> InstantiateGameObject(string key, Transform spawnTransform)
        {
            await LoadGameObjectAsync(key);

            if (!LoadedGameObjects.TryGetValue(key, out var prefab))
            {
                Debug.LogError($"Trying to instantiate an object that doesn't exist! Key: {key}");
                return null;
            }
            
            if (prefab != null)
            {
                var gameObject = Object.Instantiate(prefab, spawnTransform);

                if (!AssetManager.AssetsAreVisible)
                {
                    var cacheComponent = gameObject.EnsureComponent<LayerCache>();

                    cacheComponent.SetCache("HideFromCamera");
                    
                    // Make sure the LayerCache is placed with the AssetMediator
                    var assetMediator = gameObject.GetComponentInParent<IAssetMediator>();                    

                    if (assetMediator != null)
                    {
                        var parentCache = assetMediator.AttachedGameObject.EnsureComponent<LayerCache>();

                        // The visuals of the 'rigRoot' of the BoundsControl can get regenerated, so make sure that the active state
                        // is set to false here so the Bounds Control is not visible
                        var bounds = assetMediator.AttachedGameObject.GetComponent<BoundsControl>();

                        if (bounds != null)
                        {
                            bounds.Active = false;
                        }

                        Object.Destroy(cacheComponent);
                    }
                    // Else this object itself will hold the layer cache data, make sure that Restore is called
                }

                return gameObject;
            }
                
            Debug.LogError($"Trying to instantiate an object that doesn't exist! Key: {key}");
            return null;
        }

        /// <summary>
        /// Releases all saved Addressable assets.
        /// Releases async operation handles for Addressables as well. 
        /// </summary>
        public UniTask UnloadAllAddressableGameObjectsAsync()
        {
            foreach (GameObject assetType in loadedGameObjects.Values)
            {
                Release(assetType);
            }

            loadedGameObjects.Clear();

            Resources.UnloadUnusedAssets();

            return UniTask.CompletedTask;
        }

        public Action<AsyncOperationHandle<GameObject>> Completed
            (Action<GameObject> completedLoadingAction)
        {
            return (asyncOperation) =>
            {
                asyncOperation.Completed -= Completed(completedLoadingAction);
                completedLoadingAction?.Invoke(asyncOperation.Result);
            };
        }

        public void Release(GameObject gameObject) => Addressables.Release(gameObject);
    }
}