using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using GIGXR.Platform.Scenarios.GigAssets.Data;
using GIGXR.Platform.Core.DependencyInjection;

namespace GIGXR.Platform.Scenarios.GigAssets
{
    /// <summary>
    /// Manages instantiation and destruction of instances of specified asset type.
    /// </summary>
    public class InventoryAssetTypeComponent : BaseAssetTypeComponent<EmptyAssetData>
    {
        #region Asset Type Component implementation

        protected override void Setup()
        {

        }

        protected override void Teardown()
        {

        }

        public override void SetEditorValues()
        {
            assetData.name.designTimeData.defaultValue = "Inventory";

            assetData.description.designTimeData.defaultValue
                = "Manages instantiation and destruction of instances of the specified asset type.";
        }

        #endregion

        #region Serialized Configuration

        [Tooltip("The asset to manage instances of. Should be a dependency of the AssetMediator.")]
        [SerializeField] private AssetReference targetAssetType;

        [Tooltip("If true, instantiated assets will be unsaved by default.")]
        [SerializeField] private bool defaultRuntimeOnly = true;

        #endregion

        #region Private Data

        private IGigAssetManager assetManager;
        private Dictionary<Guid, GameObject> instances = new Dictionary<Guid, GameObject>();

        #endregion

        #region Initialization

        [InjectDependencies]
        public virtual void Construct(IGigAssetManager assetManager)
        {
            this.assetManager = assetManager;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Create and return an instance of managed asset type.
        /// </summary>
        public async UniTask<GameObject> CreateInstance()
        {
            return await CreateInstance(Guid.NewGuid(), defaultRuntimeOnly);
        }

        /// <summary>
        /// Create and return an instance of managed asset type.
        /// </summary>
        public async UniTask<GameObject> CreateInstance(bool runtimeOnly)
        {
            return await CreateInstance(Guid.NewGuid(), runtimeOnly);
        }

        /// <summary>
        /// Create and return an instance of managed asset type.
        /// </summary>
        public async UniTask<GameObject> CreateInstance(Guid assetId)
        {
            return await CreateInstance(assetId, defaultRuntimeOnly);
        }

        /// <summary>
        /// Create and return an instance of managed asset type.
        /// </summary>
        public async UniTask<GameObject> CreateInstance(Guid assetId, bool runtimeOnly)
        {
            lock (instances)
            {
                if (instances.ContainsKey(assetId))
                {
                    Debug.LogError("Unable to instantiate asset; duplicate asset id");
                    return null;
                }
            }

            string assetTypeId = targetAssetType.RuntimeKey.ToString();
            GameObject instance = await assetManager.Instantiate(
                new AssetInstantiationArgs(
                    assetTypeId,
                    assetId,
                    assetId.ToString(), // do we need to support human-readable ids even though they're deprecated?
                    true,
                    true,
                    runtimeOnly
                )
            );

            if (instance == null)
            {
                Debug.LogError("Unable to instantiate asset; instantiation returned null.");
                return null;
            }

            lock (instances)
            {
                instances.Add(assetId, instance);
            }

            return instance;
        }

        /// <summary>
        /// Destroy a managed instance specified by asset id.
        /// </summary>
        /// <param name="assetId">
        /// The id of the instance to destroy.
        /// </param>
        public void DestroyInstance(Guid assetId)
        {
            if (instances.Remove(assetId))
            {
                assetManager.Destroy(assetId);
            }
            else
            {
                Debug.LogWarning("Specified asset is either not managed by this inventory or was already destroyed.");
            }
        }

        /// <summary>
        /// Destroy a managed instance.
        /// </summary>
        /// <param name="instance">
        /// The managed instance to destroy.
        /// </param>
        public void DestroyInstance(GameObject instance)
        {
            IAssetMediator assetMediator = instance.GetComponent<IAssetMediator>();

            if (assetMediator == null)
            {
                Debug.LogError("InventoryAssetTypeComponent can only be used to manage valid AssetTypeComponents with AssetMedidators.");
            }

            DestroyInstance(assetMediator.AssetId);
        }

        #endregion
    }
}
