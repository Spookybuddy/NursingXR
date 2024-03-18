namespace GIGXR.Platform.Scenarios.GigAssets
{
    using Cysharp.Threading.Tasks;
    using Data;
    using EventArgs;
    using GIGXR.Platform.Scenarios.Stages.Data;
    using GIGXR.Platform.Utilities;
    using Loader;
    using Stages;
    using System;
    using System.Threading;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Bitmap enum to allow the assets to be hidden without colliding with each other over 1 bool value.
    /// </summary>
    [Flags]
    public enum HideAssetReasons
    {
        None = 0,
        Loading = 1,
        Syncing = 2,
        WaitingRoom = 4,
        ContentMarker = 8
    }

    public class ContentMarkerUpdateEventArgs : System.EventArgs
    {
        public Vector3 contentMarkerPosition;

        public Quaternion contentMarkerRotation;

        public IAssetMediator assetContentMarker;

        public ContentMarkerUpdateEventArgs(Vector3 position, Quaternion rotation, IAssetMediator contentMarker = null)
        {
            contentMarkerPosition = position;
            contentMarkerRotation = rotation;
            assetContentMarker = contentMarker;
        }
    }

    /// <summary>
    /// Responsible for managing the lifecycle of assets.
    /// </summary>
    /// <remarks>
    /// The default implementation should use an ICalibrationProvider to instantiate the objects as children to the
    /// provided CalibrationRoot.
    /// </remarks>

    public interface IAssetManager<TAssetContext> : IAssetProvider, IAssetContextProvider<TAssetContext> where TAssetContext : IAssetContext
    {
        event EventHandler<AssetInstantiatedEventArgs> AssetInstantiated;
        event EventHandler<AssetDestroyedEventArgs> AssetDestroyed;
        event EventHandler<AllAssetsDestroyedEventArgs> AllAssetsDestroyed;
        event EventHandler<AllAssetInstantiatedEventArgs> AllAssetsInstantiated;
        event EventHandler<AllAssetsReloadedEventArgs> AllAssetsReloaded;
        event EventHandler<AssetPropertyChangeEventArgs> AssetPropertyUpdated;

        IAssetTypeLoader AssetTypeLoader { get; }
        IAddressablesGameObjectLoader AddressablesGameObjectLoader { get; }
        IStageManager StageManager { get; }
        ICalibrationRootProvider CalibrationRootProvider { get; }
        IReadOnlyDictionary<Guid, InstantiatedAsset> InstantiatedAssets { get; }
        IReadOnlyDictionary<Guid, InstantiatedAsset> AllInstantiatedAssets { get; }
        IUniTaskAsyncEnumerable<(Guid, InstantiatedAsset)> AllInstantiatedAssetsAsync { get; }
        IUniTaskAsyncEnumerable<(Guid, InstantiatedAsset)> InstantiatedAssetsAsync { get; }
        IReadOnlyDictionary<Guid, InstantiatedAsset> RuntimeInstantiatedAssets { get; }

        int AssetCount { get; }
        
        int DesigntimeAssetCount { get; }
        
        int RuntimeAssetCount { get; }

        bool AssetsAreVisible { get; }

        #region ContentMarker

        /// <summary>
        /// Event sent out locally when the content marker is set
        /// </summary>
        event EventHandler<ContentMarkerUpdateEventArgs> ContentMarkerUpdated;

        /// <summary>
        /// The Asset in the Scenario's PresetAsset list which contains the ContentMarkerAssetTypeComponent
        /// </summary>
        IAssetMediator ContentMarkerAsset { get; }

        /// <summary>
        /// This is the spawned copy of the asset that the user will move to set the content marker
        /// </summary>
        Transform ContentMarkerInstance { get; }

        /// <summary>
        /// Sets the content marker based on the current position of the content marker instance.
        /// </summary>
        void SetContentMarker();

        /// <summary>
        /// Sets up a content marker handle, the object that the user will manipulate to set the
        /// center of the scenario.
        /// </summary>
        /// <param name="assetsHidden">If true, will hide all the instantiated assets in the scenario</param>
        void SpawnContentMarkerHandle(bool assetsHidden, bool contentMarkerVisualState = false);

        /// <summary>
        /// Removes the content marker handle out of the scene
        /// </summary>
        void RemoveContentMarkerHandle(bool wasCancelled = false);

        #endregion

        UniTask<GameObject> Instantiate(AssetInstantiationArgs instantiationArgs);
        GameObject Instantiate(PrefabInstantiationArgs instantiationArgs);
        void InstantiateInteractablePositionedAndOriented(AssetInstantiationArgs instantiationArgs, Vector3 position, Quaternion rotation);
        void Destroy(Guid assetId);
        void DestroyAll();
        void HideAll(HideAssetReasons hideReason);
        void ShowAll(HideAssetReasons showReason);
        UniTask<List<Asset>> SerializeToAssetDataAsync(bool includeRuntimeOnlyAssets);

        UniTask LoadStagesAndInstantiateAssetsAsync(IEnumerable<Stage> stagesToLoad, IEnumerable<Asset> assetsToInstantiate, CancellationToken cancellationToken);

        UniTask ReloadStagesAndAssetsAsync(IEnumerable<Stage> stagesToReload, IEnumerable<Asset> assetsToReload, Guid currentStageId);

        /// <summary>
        /// Updates an Asset Property run time data immediately
        /// </summary>
        /// <param name="assetId">The instance Asset ID</param>
        /// <param name="propertyName">The property name of the Asset Property</param>
        /// <param name="value">The new value to set</param>
        void UpdateAssetProperty(Guid assetId, string propertyName, byte[] value);

        /// <summary>
        /// Allows an Asset (and their AssetTypeComponents) to update data that is not synced over GMS, but over the Network layer (i.e. Photon)
        /// </summary>
        void SyncAllRuntimeRoomData();

        /// <summary>
        /// Disables the interactivity state for all assets.
        /// </summary>
        UniTask DisableInteractivityForAllAssetsAsync();

        /// <summary>
        /// Sets the interactivity state for all assets for Play mode.
        /// </summary>
        UniTask EnableOrDisableInteractivityForPlayScenarioAsync();

        /// <summary>
        /// Sets the interactivity state for all assets for Edit mode.
        /// </summary>
        UniTask EnableOrDisableInteractivityAsync();
    }

    public interface IAssetProvider
    {
        GameObject GetById(Guid assetId);

        GameObject GetByPresetId(string presetAssetId);

        void SetLayerOnAsset(GameObject asset, string layerName = null);

        T GetComponentOnId<T>(Guid assetId) where T : Component;

        /// <summary>
        /// Uses the human-readable preset ID rather than the Guid. 
        /// </summary>
        /// <param name="presetAssetId"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetComponentOnPresetId<T>(string presetAssetId) where T : Component;
    }

    public class InstantiatedAsset
    {
        public Guid AssetId;
        public string AssetTypeId;
        public GameObject GameObject;
        public IAssetMediator InstantiatedAssetMediator;
    }

    public class AssetInstantiationArgs
    {
        public string AssetTypeId { get; }

        public Guid AssetId;

        public string PresetAssetId { get; }

        // true if the asset is being instantiated after the scenario has loaded.
        public bool IsRuntimeInstantiation { get; }
        public bool RuntimeInstantiationOriginatedLocally { get; }

        // true if the asset should not be saved with the session
        public bool RuntimeOnly { get; }

        public string AssetData;
        
        //Added for handling runtime instantiation positions

        public Vector3 AssetPosition;
        public Quaternion AssetRotation;

        public AssetInstantiationArgs(
            string assetTypeId, Guid assetId, string presetAssetId, 
            bool isRuntimeInstantiation=false, bool runtimeInstantiationOriginatedLocally = true,
            bool runtimeOnly = false, string assetData = "", Vector3 assetPosition = default(Vector3), 
            Quaternion assetRotation = default(Quaternion))
        {
            AssetTypeId = assetTypeId;
            AssetId = assetId;
            PresetAssetId = presetAssetId;
            IsRuntimeInstantiation = isRuntimeInstantiation;
            RuntimeInstantiationOriginatedLocally = runtimeInstantiationOriginatedLocally;
            RuntimeOnly = runtimeOnly;
            AssetData = assetData;
            AssetPosition = assetPosition;
            AssetRotation = assetRotation;
        }
    }

    public class PrefabInstantiationArgs
    {
        public GameObject Prefab;
        public Transform Parent;
        public bool IgnoreSettingLayer;

        public PrefabInstantiationArgs(GameObject prefab, bool ignoreLayer, Transform parent = null)
        {
            Prefab = prefab;
            Parent = parent;
            IgnoreSettingLayer = ignoreLayer;
        }

        public PrefabInstantiationArgs(GameObject prefab, Transform parent = null) : this(prefab, false, parent)
        {

        }
    }
}