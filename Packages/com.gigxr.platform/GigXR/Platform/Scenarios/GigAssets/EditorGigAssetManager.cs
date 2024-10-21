namespace GIGXR.Platform.Scenarios.GigAssets
{
    using GIGXR.Platform.Utilities;
    using GIGXR.Platform.Scenarios.GigAssets.Loader;
    using GIGXR.Platform.Scenarios.Stages;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Cysharp.Threading.Tasks;
    using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
    using GIGXR.Platform.Scenarios.Stages.Data;
    using GIGXR.Platform.Scenarios.GigAssets.Data;
    using System.Linq;
    using Photon.Pun;
    using System.Threading;

    public class EditorGigAssetManager : IGigAssetManager
    {
        /// <summary>
        /// Storage for assets which should be saved with the session.
        private readonly Dictionary<Guid, InstantiatedAsset> instantiatedAssets
            = new Dictionary<Guid, InstantiatedAsset>();

        private readonly Dictionary<string, Guid> presetAssetMap = new Dictionary<string, Guid>();

        public EditorGigAssetManager
        (
            IAssetTypeLoader assetTypeLoader
        )
        {
            AssetTypeLoader = assetTypeLoader;
        }

        public IAssetTypeLoader AssetTypeLoader { get; }

        public IAddressablesGameObjectLoader AddressablesGameObjectLoader => throw new NotImplementedException();

        public IStageManager StageManager => throw new NotImplementedException();

        public ICalibrationRootProvider CalibrationRootProvider => throw new NotImplementedException();

        public IReadOnlyDictionary<Guid, InstantiatedAsset> InstantiatedAssets => throw new NotImplementedException();

        public IReadOnlyDictionary<Guid, InstantiatedAsset> AllInstantiatedAssets => throw new NotImplementedException();

        public IUniTaskAsyncEnumerable<(Guid, InstantiatedAsset)> AllInstantiatedAssetsAsync => throw new NotImplementedException();

        public IUniTaskAsyncEnumerable<(Guid, InstantiatedAsset)> InstantiatedAssetsAsync => throw new NotImplementedException();

        public IReadOnlyDictionary<Guid, InstantiatedAsset> RuntimeInstantiatedAssets => throw new NotImplementedException();

        public int AssetCount => throw new NotImplementedException();

        public int DesigntimeAssetCount => throw new NotImplementedException();

        public int RuntimeAssetCount => throw new NotImplementedException();

        public bool AssetsAreVisible => throw new NotImplementedException();

        public IGigAssetContext AssetContext => throw new NotImplementedException();

        public IAssetMediator ContentMarkerAsset => throw new NotImplementedException();

        public Transform ContentMarkerInstance => throw new NotImplementedException();

        public event EventHandler<AssetInstantiatedEventArgs> AssetInstantiated;
        public event EventHandler<AssetDestroyedEventArgs> AssetDestroyed;
        public event EventHandler<AllAssetsDestroyedEventArgs> AllAssetsDestroyed;
        public event EventHandler<AllAssetInstantiatedEventArgs> AllAssetsInstantiated;
        public event EventHandler<AllAssetsReloadedEventArgs> AllAssetsReloaded;
        public event EventHandler<AssetPropertyChangeEventArgs> AssetPropertyUpdated;
        public event EventHandler<ContentMarkerUpdateEventArgs> ContentMarkerUpdated;

        public void Destroy(Guid assetId)
        {
            throw new NotImplementedException();
        }

        public void DestroyAll()
        {
            foreach (Guid assetId in instantiatedAssets.Keys.ToList())
            {
                DestroyInternal(assetId);
            }

            AllAssetsDestroyed?.Invoke(this, new AllAssetsDestroyedEventArgs());
        }

        private bool DestroyInternal
        (
            Guid assetId
        )
        {
            if (instantiatedAssets.TryGetValue(assetId, out var instantiatedAsset))
            {
                instantiatedAssets.Remove(assetId);

                // If the GameObject was destroyed without being routed through here,
                // then trying to find its AssetMediator to unregister will create an NRE.
                if (instantiatedAsset.GameObject != null)
                {
                    UnityEngine.Object.Destroy(instantiatedAsset.GameObject);
                }
                else
                {
                    Debug.LogWarning
                        ($"[GigAssetManager] An asset with type id {instantiatedAsset.AssetTypeId} " +
                         "was destroyed outside of GigAssetManager.DestroyInternal.");
                }

                return true;
            }

            return false;
        }

        public GameObject GetById(Guid assetId)
        {
            Debug.Log($"EditorGigAssetManager TODO GetById {assetId}");
            return instantiatedAssets.TryGetValue(assetId, out InstantiatedAsset instantiatedAsset)
                ?
                instantiatedAsset.GameObject
                : null;
        }

        public GameObject GetByPresetId(string presetAssetId)
        {
            Debug.Log($"Editor GetByPresetId {presetAssetId} out of {presetAssetMap.Count}");
            return presetAssetMap.TryGetValue(presetAssetId, out Guid instantiatedAssetId)
                ? GetById(instantiatedAssetId)
                : null;
        }

        public void SetLayerOnAsset(GameObject asset, string layerName = null)
        {
            throw new NotImplementedException();
        }

        public T GetComponentOnId<T>(Guid assetId) where T : Component
        {
            throw new NotImplementedException();
        }

        public T GetComponentOnPresetId<T>(string presetAssetId) where T : Component
        {
            throw new NotImplementedException();
        }

        public void HideAll(HideAssetReasons hideReason)
        {
            throw new NotImplementedException();
        }

        public async UniTask<GameObject> Instantiate(AssetInstantiationArgs instantiationArgs)
        {
            if (instantiatedAssets.ContainsKey(instantiationArgs.AssetId))
            {
                Debug.LogWarning("Trying to instantiate an asset with a non-unique assetId!");
                return instantiatedAssets[instantiationArgs.AssetId].GameObject;
            }

            if (!AssetTypeLoader.LoadedAssetTypes.TryGetValue
                (instantiationArgs.AssetTypeId, out var prefab))
            {
                Debug.LogWarning("Trying to instantiate an assetType that doesn't exist!");
                return null;
            }

            string presetAssetId = string.IsNullOrEmpty(instantiationArgs.PresetAssetId)
                ? instantiationArgs.AssetId.ToString()
                : instantiationArgs.PresetAssetId;

            var assetGameObject = UnityEngine.Object.Instantiate
                (prefab, CalibrationRootProvider.ContentMarkerRoot.transform);

            assetGameObject.name = $"{presetAssetId} ({assetGameObject.name})";

            // Instantiation takes the longest in a single frame, so give a quick yield to suspend execution
            await UniTask.Yield();

            var assetMediator = assetGameObject?.GetComponent<IAssetMediator>();

            instantiatedAssets.Add
                (instantiationArgs.AssetId,
                    new InstantiatedAsset
                    {
                        AssetId = instantiationArgs.AssetId,
                        AssetTypeId = instantiationArgs.AssetTypeId,
                        GameObject = assetGameObject,
                        InstantiatedAssetMediator = assetMediator
                    });

            presetAssetMap.Add(instantiationArgs.PresetAssetId, instantiationArgs.AssetId);

            // if a runtime instantiation originates here, then the asset data will be empty up to this point; populate it
            if (instantiationArgs.IsRuntimeInstantiation &&
                instantiationArgs.RuntimeInstantiationOriginatedLocally)
            {
                instantiationArgs.AssetData = assetMediator?.SerializeToJson() ?? "{}";
            }

            AssetInstantiated?.InvokeSafely
                (this,
                 new AssetInstantiatedEventArgs
                     (instantiationArgs.AssetTypeId,
                      instantiationArgs.AssetId,
                      instantiationArgs.PresetAssetId,
                      Vector3.zero,
                      Quaternion.identity,
                      instantiationArgs.IsRuntimeInstantiation,
                      instantiationArgs.RuntimeOnly,
                      instantiationArgs.AssetData,
                      assetGameObject?.GetComponent<PhotonView>(),
                      instantiationArgs.RuntimeInstantiationOriginatedLocally));

            return assetGameObject;
        }

        public void InstantiateInteractablePositionedAndOriented(AssetInstantiationArgs instantiationArgs, Vector3 position, Quaternion rotation)
        {
            throw new NotImplementedException();
        }

        public UniTask LoadStagesAndInstantiateAssetsAsync(IEnumerable<Stage> stagesToLoad, IEnumerable<Asset> assetsToInstantiate, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public UniTask ReloadStagesAndAssetsAsync(object scenarioData, Guid currentStageId)
        {
            throw new NotImplementedException();
        }

        public UniTask<List<Asset>> SerializeToAssetDataAsync(bool includeRuntimeOnlyAssets)
        {
            throw new NotImplementedException();
        }

        public void ShowAll(HideAssetReasons showReason)
        {
            throw new NotImplementedException();
        }

        public void SyncAllRuntimeRoomData()
        {
            throw new NotImplementedException();
        }

        public void UpdateAssetProperty(Guid assetId, string propertyName, byte[] value)
        {
            throw new NotImplementedException();
        }

        public void RemoveContentMarkerHandle(bool wasCancelled = false)
        {
            throw new NotImplementedException();
        }

        public void SpawnContentMarkerHandle(bool assetsHidden, bool contentMarkerVisualState = false)
        {
            throw new NotImplementedException();
        }

        public void SetContentMarker()
        {
            throw new NotImplementedException();
        }

        public UniTask DisableInteractivityForAllAssetsAsync()
        {
            throw new NotImplementedException();
        }

        public UniTask EnableOrDisableInteractivityForPlayScenarioAsync()
        {
            throw new NotImplementedException();
        }

        public UniTask EnableOrDisableInteractivityAsync()
        {
            throw new NotImplementedException();
        }

        public GameObject Instantiate(PrefabInstantiationArgs instantiationArgs)
        {
            throw new NotImplementedException();
        }
    }
}