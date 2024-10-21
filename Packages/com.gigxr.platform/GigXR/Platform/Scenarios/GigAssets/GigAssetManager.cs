namespace GIGXR.Platform.Scenarios.GigAssets
{
    using Utilities;
    using Data;
    using EventArgs;
    using Loader;
    using Stages;
    using GIGXR.Platform.Scenarios.Stages.Data;
    using GIGXR.Platform.Scenarios.Stages.EventArgs;
    using Photon.Pun;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Object = UnityEngine.Object;
    using Cysharp.Threading.Tasks;
    using System.Reflection;
    using Cysharp.Threading.Tasks.Linq;
    using Microsoft.MixedReality.Toolkit;
    using System.Threading;
    using Logger = GIGXR.Platform.Utilities.Logger;
    using Microsoft.MixedReality.Toolkit.Utilities;
    using Microsoft.MixedReality.Toolkit.Input;
    using Microsoft.MixedReality.Toolkit.UI;
    using GIGXR.Platform.Core.Settings;
    using GIGXR.Platform.Core.FeatureManagement;
    using GIGXR.Platform.Scenarios.Data;

    public class GigAssetManager : IGigAssetManager, IDisposable
    {
        public IGigAssetContext AssetContext { get; private set; }

        public Transform ContentMarkerInstance
        {
            get
            {
                return _contentMarkerTransformInstance;
            }
        }

        public static string HideFromCamera = "HideFromCamera";

        private Transform _contentMarkerTransformInstance;

        private ContentMarkerAssetTypeComponent contentMarkerInstance;

        private Vector3? originalCalibratedPosition = null;
        private Quaternion? originalCalibratedRotation = null;

        /// <summary>
        /// Storage for assets which should be saved with the session.
        /// 
        /// TODO rename "DesignTimeInstantiatedAssets" and break all existing session plans.
        /// </summary>
        private readonly Dictionary<Guid, InstantiatedAsset> instantiatedAssets = new Dictionary<Guid, InstantiatedAsset>();

        /// <summary>
        /// Storage for assets which should not be saved with the session. These
        /// will be lost once the session is closed, but will be stored in ephemeral
        /// data while the session is running.
        /// </summary>
        private readonly Dictionary<Guid, InstantiatedAsset>
            runtimeInstantiatedAssets = new Dictionary<Guid, InstantiatedAsset>();

        private readonly Dictionary<string, Guid> presetAssetMap = new Dictionary<string, Guid>();

        private readonly Dictionary<Guid, string> reversePresetAssetMap = new Dictionary<Guid, string>(); // TODO remove this

        private readonly ForwardPropertyUpdateHandler forwardPropertyUpdateHandler = new ForwardPropertyUpdateHandler();

        private readonly IUnityScheduler unityScheduler;

        private HideAssetReasons assetsHiddenReasons;

        public GigAssetManager(ICalibrationRootProvider calibrationRoot, IAssetTypeLoader assetTypeLoader,
            IAddressablesGameObjectLoader addressablesGameObjectLoader, IStageManager stageManager,
            IUnityScheduler unityScheduler, ProfileManager profileManager,
            Func<IUniTaskAsyncEnumerable<(MonoBehaviour, MethodInfo)>, UniTask> runtimeExpeditedFunction,
            IFeatureManager featureManager)
        {
            CalibrationRootProvider = calibrationRoot;
            AssetTypeLoader = assetTypeLoader;
            AddressablesGameObjectLoader = addressablesGameObjectLoader;
            StageManager = stageManager;
            this.unityScheduler = unityScheduler;
            ProfileManager = profileManager;
            RuntimeExpeditedDependencyInjectionFunction = runtimeExpeditedFunction;
            AssetContext = new GigAssetContext();
            FeatureManager = featureManager;

            StageManager.StageSwitched += StageManagerOnStageSwitched;

            Logger.AddTaggedLogger("GigAssetManager", "Asset Manager Console");
        }

        public event EventHandler<AssetInstantiatedEventArgs> AssetInstantiated;
        public event EventHandler<AssetDestroyedEventArgs> AssetDestroyed;
        public event EventHandler<AllAssetsDestroyedEventArgs> AllAssetsDestroyed;
        public event EventHandler<AllAssetInstantiatedEventArgs> AllAssetsInstantiated;
        public event EventHandler<AllAssetsReloadedEventArgs> AllAssetsReloaded;
        public event EventHandler<ContentMarkerUpdateEventArgs> ContentMarkerUpdated;

        public event EventHandler<AssetPropertyChangeEventArgs> AssetPropertyUpdated
        {
            add => forwardPropertyUpdateHandler.AssetPropertyUpdated += value;
            remove => forwardPropertyUpdateHandler.AssetPropertyUpdated -= value;
        }

        public IAssetTypeLoader AssetTypeLoader { get; }

        public IAddressablesGameObjectLoader AddressablesGameObjectLoader { get; }

        public IStageManager StageManager { get; }

        public ICalibrationRootProvider CalibrationRootProvider { get; }

        private IFeatureManager FeatureManager { get; }

        public IReadOnlyDictionary<Guid, InstantiatedAsset> InstantiatedAssets => instantiatedAssets;

        public IUniTaskAsyncEnumerable<(Guid, InstantiatedAsset)> InstantiatedAssetsAsync
        {
            get
            {
                return UniTaskAsyncEnumerable.Create<(Guid, InstantiatedAsset)>
                (
                    async (writer, token) =>
                    {
                        foreach (var asset in instantiatedAssets)
                        {
                            if (token.IsCancellationRequested)
                                return;

                            await writer.YieldAsync((asset.Key, asset.Value));

                            await UniTask.Yield();
                        }
                    }
                );
            }
        }

        public IReadOnlyDictionary<Guid, InstantiatedAsset> AllInstantiatedAssets
        {
            get
            {
                // Start the dictionary with the instantiated assets
                var allInstantiatedAssets = new Dictionary<Guid, InstantiatedAsset>(instantiatedAssets);

                // Add the runtime assets
                runtimeInstantiatedAssets.ToList().ForEach(pair => allInstantiatedAssets.Add(pair.Key, pair.Value));

                return allInstantiatedAssets;
            }
        }

        public IUniTaskAsyncEnumerable<(Guid, InstantiatedAsset)> AllInstantiatedAssetsAsync
        {
            get
            {
                return UniTaskAsyncEnumerable.Create<(Guid, InstantiatedAsset)>
                (
                    async (writer, token) =>
                    {
                        foreach (KeyValuePair<Guid, InstantiatedAsset> asset in instantiatedAssets)
                        {
                            if (token.IsCancellationRequested)
                                return;

                            await writer.YieldAsync((asset.Key, asset.Value));

                            await UniTask.Yield();
                        }

                        foreach (KeyValuePair<Guid, InstantiatedAsset> asset in runtimeInstantiatedAssets)
                        {
                            if (token.IsCancellationRequested)
                                return;

                            await writer.YieldAsync((asset.Key, asset.Value));

                            await UniTask.Yield();
                        }
                    }
                );
            }
        }

        public IReadOnlyDictionary<Guid, InstantiatedAsset> RuntimeInstantiatedAssets => runtimeInstantiatedAssets;

        private Func<IUniTaskAsyncEnumerable<(MonoBehaviour, MethodInfo)>, UniTask> RuntimeExpeditedDependencyInjectionFunction;

        private ProfileManager ProfileManager { get; }

        public int AssetCount => DesigntimeAssetCount + RuntimeAssetCount;
        public int DesigntimeAssetCount => instantiatedAssets.Count;
        public int RuntimeAssetCount => runtimeInstantiatedAssets.Count;

        public bool AssetsAreVisible => assetsHiddenReasons == HideAssetReasons.None;

        public IAssetMediator ContentMarkerAsset { get; private set; }

        public void SpawnContentMarkerHandle(bool assetsHidden, bool contentMarkerVisualState = false)
        {
            // Store the original transform data in case this process is cancelled
            originalCalibratedPosition = CalibrationRootProvider.ContentMarkerRoot.position;
            originalCalibratedRotation = CalibrationRootProvider.ContentMarkerRoot.rotation;

            // If the scenario has defined a content marker asset, use that asset to determine the location of the content marker
            // for the scenario
            if (ContentMarkerAsset != null)
            {
                Logger.Info($"Using Content Marker Asset {ContentMarkerAsset.PresetAssetId}", "GigAssetManager");

                contentMarkerInstance = ContentMarkerAsset.AttachedGameObject.GetComponent<ContentMarkerAssetTypeComponent>();
                _contentMarkerTransformInstance = contentMarkerInstance.GetPlacementMarker(contentMarkerInstance).transform;

                _contentMarkerTransformInstance.SetParent(CalibrationRootProvider.AnchorRoot.parent);

                // We do not want the SetParent method to change the position of the asset since the relative location matters here
                _contentMarkerTransformInstance.transform.position = ContentMarkerAsset.AttachedGameObject.transform.position;
                _contentMarkerTransformInstance.transform.rotation = ContentMarkerAsset.AttachedGameObject.transform.rotation;

                contentMarkerInstance.AdjustProxyDistanceToUser();

                // If the assets are not hidden, then we need the calibration root to follow the content marker
                if (!assetsHidden)
                {
                    // If the assets are not hidden, don't show the extra model since the real asset will be seen
                    foreach (var renderer in _contentMarkerTransformInstance.gameObject.GetComponentsInChildren<Renderer>())
                    {
                        renderer.enabled = contentMarkerVisualState;
                    }

                    // When you don't have a reversed forward vector we need a negative offset
                    int offsetDirection = -1;
                    if (contentMarkerInstance.reverseForward)
                    {
                        offsetDirection = 1;
                    }

                    CalibrationRootProvider.ContentMarkerFollows(_contentMarkerTransformInstance,
                        positionOffset: offsetDirection * ContentMarkerAsset.AttachedGameObject.transform.localPosition,
                        rotationOffset: ContentMarkerAsset.AttachedGameObject.transform.localRotation);
                }

                SetContentMarkerAssetMovement(_contentMarkerTransformInstance.gameObject, TransformFlags.Move | TransformFlags.Rotate);
            }
            else if (_contentMarkerTransformInstance == null)
            {
                Logger.Info("Spawning Default Content Marker Handle", "GigAssetManager");

                _contentMarkerTransformInstance = GameObject.Instantiate(ProfileManager.CalibrationProfile.DefaultCalibrationHandle).transform;
                _contentMarkerTransformInstance.SetParent(CalibrationRootProvider.AnchorRoot, false);
            }
        }

        public void RemoveContentMarkerHandle(bool wasCancelled = false)
        {
            if (wasCancelled)
            {
                if (originalCalibratedPosition.HasValue)
                {
                    CalibrationRootProvider.ContentMarkerRoot.position = originalCalibratedPosition.Value;

                    originalCalibratedPosition = null;
                }

                if (originalCalibratedRotation.HasValue)
                {
                    CalibrationRootProvider.ContentMarkerRoot.rotation = originalCalibratedRotation.Value;

                    originalCalibratedRotation = null;
                }
            }

            // If the scenario has defined a content marker asset, then don't destroy the asset and return it to the normal
            // position in the hierarchy
            if (ContentMarkerAsset != null)
            {
                Logger.Info($"Returning Content Marker Asset {ContentMarkerAsset.PresetAssetId}", "GigAssetManager");

                CalibrationRootProvider.ContentMarkerRemoveFollows();

                contentMarkerInstance?.DestroyPlacementMarker();

                _contentMarkerTransformInstance = null;
                contentMarkerInstance = null;
            }
            else if (_contentMarkerTransformInstance != null)
            {
                Logger.Info("Destroying Content Marker Handle", "GigAssetManager");

                GameObject.Destroy(_contentMarkerTransformInstance.gameObject);

                _contentMarkerTransformInstance = null;
            }
        }

        public void SetContentMarker()
        {
            var position = CalibrationRootProvider.WorldToAnchoredPosition(
                ContentMarkerInstance.position);

            var rotation = CalibrationRootProvider.WorldToAnchoredRotation(
                ContentMarkerInstance.rotation);

            // The 'forward' of an asset may not actually be the forward that is the GameObject's. Use the value set in the ContentMarker asset
            // to determine this
            if (ContentMarkerInstance.TryGetComponent(out ContentMarkerAssetTypeComponent contentMarker))
            {
                if (contentMarker.reverseForward)
                {
                    rotation *= Quaternion.Euler(0, 180f, 0);
                }

                ContentMarkerUpdated?.Invoke(this,
                                             new ContentMarkerUpdateEventArgs(position,
                                                                              rotation,
                                                                              ContentMarkerAsset));
            }
            else
            {
                ContentMarkerUpdated?.Invoke(this,
                                             new ContentMarkerUpdateEventArgs(position, rotation));
            }

            RemoveContentMarkerHandle();

            ShowAll(HideAssetReasons.ContentMarker);
        }

        private void SetContentMarkerAssetMovement(GameObject instantiatedAsset, TransformFlags transformFlags = 0)
        {
            try
            {
                var mrtkComponent = instantiatedAsset.EnsureComponent<MRTKAssetTypeComponent>();

                if (transformFlags == 0)
                {
                    mrtkComponent.DisableManipulation();
                    mrtkComponent.HideBoundsControl();
                    mrtkComponent.RestoreConstraints();
                }
                else
                {
                    mrtkComponent.EnableManipulation(transformFlags);

                    // Ensure there are no constraints on the rotation of the asset
                    mrtkComponent.OverrideConstraints(0);

                    mrtkComponent.SetBoundsConstraint(false);

                    instantiatedAsset.EnsureComponent<NearInteractionGrabbable>();

                    // We do not want the content marker to rotate when moving, only when using the handles
                    var fixedConstraint = instantiatedAsset.EnsureComponent<FixedRotationToWorldConstraint>();
                    fixedConstraint.HandType = ManipulationHandFlags.OneHanded;
                    fixedConstraint.ProximityType = (ManipulationProximityFlags)~(~0 << 2); // both

                    mrtkComponent.SetBoundsControl(showRotationHandles: true, showScaleHandles: false);

                    mrtkComponent.ActivateBoundControl(true);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public GameObject Instantiate(PrefabInstantiationArgs instantiationArgs)
        {
            GameObject assetGameObject;

            if (instantiationArgs.Parent != null)
            {
                assetGameObject = GameObject.Instantiate(instantiationArgs.Prefab, instantiationArgs.Parent);
            }
            else
            {
                assetGameObject = GameObject.Instantiate(instantiationArgs.Prefab);
            }

            if (assetsHiddenReasons != HideAssetReasons.None && !instantiationArgs.IgnoreSettingLayer)
            {
                SetAssetVisibility(assetGameObject, HideFromCamera);
            }

            return assetGameObject;
        }

        public async UniTask<GameObject> Instantiate(AssetInstantiationArgs instantiationArgs)
        {
            if (instantiatedAssets.ContainsKey(instantiationArgs.AssetId))
            {
                Logger.Warning("Trying to instantiate an asset with a non-unique assetId!", "GigAssetManager");
                return instantiatedAssets[instantiationArgs.AssetId].GameObject;
            }
            else if (runtimeInstantiatedAssets.ContainsKey(instantiationArgs.AssetId))
            {
                Logger.Warning("Trying to instantiate an asset with a non-unique assetId!", "GigAssetManager");
                return runtimeInstantiatedAssets[instantiationArgs.AssetId].GameObject;
            }

            if (!AssetTypeLoader.LoadedAssetTypes.TryGetValue(instantiationArgs.AssetTypeId, out var prefab))
            {
                Logger.Warning("Trying to instantiate an assetType that doesn't exist!", "GigAssetManager");
                return null;
            }

            string presetAssetId = string.IsNullOrEmpty(instantiationArgs.PresetAssetId)
                ? instantiationArgs.AssetId.ToString()
                : instantiationArgs.PresetAssetId;

            var assetGameObject = Object.Instantiate(prefab, CalibrationRootProvider.ContentMarkerRoot.transform);

            assetGameObject.name = $"{presetAssetId} ({assetGameObject.name})";

            var assetMediator = assetGameObject.EnsureComponent<AssetMediator>();

            InstantiatedAsset instantiatedData;

            // store runtime-only assets separately to easily avoid saving them with the session
            if (instantiationArgs.RuntimeOnly)
            {
                instantiatedData = new InstantiatedAsset
                {
                    AssetId = instantiationArgs.AssetId,
                    AssetTypeId = instantiationArgs.AssetTypeId,
                    GameObject = assetGameObject,
                    InstantiatedAssetMediator = assetMediator
                };

                runtimeInstantiatedAssets.Add(instantiationArgs.AssetId, instantiatedData);
            }
            else
            {
                instantiatedData = new InstantiatedAsset
                {
                    AssetId = instantiationArgs.AssetId,
                    AssetTypeId = instantiationArgs.AssetTypeId,
                    GameObject = assetGameObject,
                    InstantiatedAssetMediator = assetMediator
                };

                instantiatedAssets.Add(instantiationArgs.AssetId, instantiatedData);
            }

            if (assetsHiddenReasons != HideAssetReasons.None)
            {
                SetAssetVisibility(instantiatedData.GameObject, HideFromCamera);

                var mrtkComponent = instantiatedData.GameObject.GetComponent<MRTKAssetTypeComponent>();

                if (mrtkComponent != null)
                {
                    mrtkComponent.DisableManipulation();
                    mrtkComponent.HideBoundsControl();
                }
            }

            // Instantiation takes the longest in a single frame, so give a quick yield to suspend execution
            await UniTask.Yield();

            Logger.Debug
            (
                $"Added new asset with ID {instantiationArgs.AssetId} with the preset asset ID {instantiationArgs.PresetAssetId}",
                "GigAssetManager"
            );

            presetAssetMap.Add(instantiationArgs.PresetAssetId, instantiationArgs.AssetId);
            reversePresetAssetMap.Add(instantiationArgs.AssetId, instantiationArgs.PresetAssetId);

            // if a runtime instantiation originates here, then the asset data will be empty up to this point; populate it
            if (instantiationArgs.IsRuntimeInstantiation && instantiationArgs.RuntimeInstantiationOriginatedLocally)
            {
                instantiationArgs.AssetData = assetMediator.SerializeToJson() ?? "{}";
            }

            AssetInstantiated?.InvokeSafely
            (
                this,
                new AssetInstantiatedEventArgs
                (
                    instantiationArgs.AssetTypeId,
                    instantiationArgs.AssetId,
                    instantiationArgs.PresetAssetId,
                    Vector3.zero,
                    Quaternion.identity,
                    instantiationArgs.IsRuntimeInstantiation,
                    instantiationArgs.RuntimeOnly,
                    instantiationArgs.AssetData,
                    assetGameObject.EnsureComponent<PhotonView>(),
                    instantiationArgs.RuntimeInstantiationOriginatedLocally
                )
            );

            forwardPropertyUpdateHandler.RegisterPropertyUpdateEventHandler(assetGameObject);

            await RuntimeExpeditedDependencyInjectionFunction.Invoke(assetMediator.GetAllInjectableDependenciesAsync());

            if (!instantiationArgs.IsRuntimeInstantiation)
            {
                return assetGameObject;
            }

            // handle the post-instantiation setup that would otherwise be handled en-masse in InstantiateAssetsAsync
            await HandleRuntimeInstantiation
                (assetGameObject, instantiationArgs, instantiationArgs.RuntimeInstantiationOriginatedLocally);

            // HACK delay return of the GO (and thereby property updates)
            // to allow receivers of the instantiation event time to instantiate
            // locally before property updates can occur
            if (instantiationArgs.RuntimeInstantiationOriginatedLocally)
            {
                await UniTask.Delay(500);
            }

            return assetGameObject;
        }

        /// <summary>
        /// This handles the post-instantiation setup for runtime instantiations which would be handled
        /// in bulk for en-masse design time instantiations
        /// </summary>
        /// <param name="assetGameObject"></param>
        /// <param name="instantiationArgs"></param>
        /// <param name="isAssetDataSource"></param>
        /// <returns></returns>
        private UniTask HandleRuntimeInstantiation(GameObject assetGameObject, AssetInstantiationArgs instantiationArgs, bool isAssetDataSource)
        {
            if (assetGameObject != null && assetGameObject.TryGetComponent(out IAssetMediator assetMediator))
            {
                //Added to be able to change the position and rotation of the runtime instantiated asset
                assetGameObject.transform.localPosition = instantiationArgs.AssetPosition;
                assetGameObject.transform.localRotation = instantiationArgs.AssetRotation;

                assetMediator.SetRuntimeID(instantiationArgs.AssetId, instantiationArgs.PresetAssetId);

                foreach (IAssetTypeComponent atc in assetGameObject.GetComponents<IAssetTypeComponent>())
                {
                    atc.OnMount();
                }

                assetMediator.OnAssetMounted();
            }

            return UniTask.CompletedTask;
        }

        public GameObject GetById(Guid assetId)
        {
            return instantiatedAssets.TryGetValue
                    (assetId, out InstantiatedAsset instantiatedAsset) ? instantiatedAsset.GameObject :
                runtimeInstantiatedAssets.TryGetValue
                    (assetId, out InstantiatedAsset runtimeAsset) ? runtimeAsset.GameObject : null;
        }

        public GameObject GetByPresetId(string presetAssetId)
        {
            return presetAssetMap.TryGetValue(presetAssetId, out Guid instantiatedAssetId) ? GetById(instantiatedAssetId) : null;
        }

        public void SetLayerOnAsset(GameObject asset, string layerName = null)
        {
            // Restore to default layers if empty or null is given
            if (string.IsNullOrEmpty(layerName))
            {
                var cache = asset.GetComponent<LayerCache>();

                if (cache != null)
                {
                    cache.Restore();
                }
            }
            else
            {
                var cache = asset.EnsureComponent<LayerCache>();

                cache.SetCache(layerName);
            }
        }

        public T GetComponentOnId<T>(Guid assetId) where T : Component
        {
            return instantiatedAssets.TryGetValue
                    (assetId, out InstantiatedAsset instantiatedAsset)
                    ?
                    instantiatedAsset.GameObject.GetComponent<T>()
                    :
                    runtimeInstantiatedAssets.TryGetValue
                        (assetId, out InstantiatedAsset runtimeAsset)
                        ? runtimeAsset.GameObject.GetComponent<T>()
                        : null;
        }

        /// <summary>
        /// Uses the human-readable preset ID rather than the Guid. 
        /// </summary>
        /// <param name="presetAssetId"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponentOnPresetId<T>(string presetAssetId) where T : Component
        {
            if (!presetAssetMap.TryGetValue(presetAssetId, out var assetId))
                return default;

            var presetGameObject = GetById(assetId);
            return presetGameObject.GetComponent<T>();
        }

        public void Destroy(Guid assetId)
        {
            Destroy(assetId, false);
        }

        private void Destroy(Guid assetId, bool fromReload)
        {
            if (DestroyInternal(assetId))
            {
                AssetDestroyed?.InvokeSafely(this, new AssetDestroyedEventArgs(assetId, fromReload));
            }
        }

        public void DestroyAll()
        {
            assetsHiddenReasons = HideAssetReasons.None;

            foreach (Guid assetId in instantiatedAssets.Keys.ToList())
            {
                DestroyInternal(assetId);
            }

            foreach (Guid assetId in runtimeInstantiatedAssets.Keys.ToList())
            {
                DestroyInternal(assetId);
            }

            AllAssetsDestroyed?.InvokeSafely(this, new AllAssetsDestroyedEventArgs());
        }

        public void HideAll(HideAssetReasons hideReason)
        {
            // Only hide all the assets once, if there are no reasons why it's hidden yet, it hasn't been applied yet
            if (assetsHiddenReasons == HideAssetReasons.None)
            {
                HideAllAssets();
            }
            else
            {
                Logger.Info($"Assets are already hidden. Reason: {assetsHiddenReasons}", "GigAssetManager");
            }

            // Adds the new hidden reason to the known reasons to hide
            assetsHiddenReasons |= hideReason;
        }

        public void ShowAll(HideAssetReasons showReason)
        {
            // Removes the reason that the assets were hidden
            assetsHiddenReasons &= ~showReason;

            // There are no more reasons to hide the assets, show the them
            if (assetsHiddenReasons == HideAssetReasons.None)
            {
                ShowAllAssets();
            }
            else
            {
                Logger.Info($"Assets are still hidden. Reason: {assetsHiddenReasons}", "GigAssetManager");
            }
        }

        private async void HideAllAssets()
        {
            await ForAllAssets(instantiatedAsset =>
            {
                SetAssetVisibility(instantiatedAsset.GameObject, HideFromCamera);
            });

            await DisableInteractivityForAllAssetsAsync();
        }

        private async void ShowAllAssets()
        {
            await ForAllAssets(instantiatedAsset =>
            {
                SetAssetVisibility(instantiatedAsset.GameObject);
            });
        }

        private void SetAssetVisibility(GameObject asset, string layerName = null)
        {
            SetLayerOnAsset(asset, layerName);
        }

        /// <summary>
        /// Generate a list of assets in an easily serializable format.
        /// Optionally include runtime-only assets, to include in ephemeral data
        /// but exclude from saved sessions.
        /// </summary>
        /// <param name="includeRuntimeOnlyAssets"></param>
        /// <returns></returns>
        public async UniTask<List<Asset>> SerializeToAssetDataAsync(bool includeRuntimeOnlyAssets)
        {
            List<Asset> serializedAssets = new List<Asset>();

            await AddSerializedAssetsToList(serializedAssets, instantiatedAssets, false);

            if (includeRuntimeOnlyAssets)
            {
                await AddSerializedAssetsToList(serializedAssets, runtimeInstantiatedAssets, true);
            }

            return serializedAssets;
        }

        /// <summary>
        /// Go through the provided assets to serialize, and add their serialized
        /// Asset representation to the provided list. 
        /// </summary>
        /// <param name="serializedAssets"></param>
        /// <param name="assetsToSerialize"></param>
        /// <param name="runtimeOnly"></param>
        /// <returns></returns>
        private async UniTask AddSerializedAssetsToList(List<Asset> serializedAssets,
            Dictionary<Guid, InstantiatedAsset> assetsToSerialize, bool runtimeOnly)
        {
            foreach (var asset in assetsToSerialize)
            {
                Asset serializedAssetData = null;

                var assetMediator = await unityScheduler.GetComponentTaskSafeAsync<IAssetMediator>
                    (asset.Value.GameObject);

                try
                {
                    await UniTask.RunOnThreadPool
                    (
                        () =>
                        {
                            serializedAssetData = new Asset
                            {
                                assetTypeId = asset.Value.AssetTypeId,
                                assetId = asset.Value.AssetId.ToString(),
                                presetAssetId = reversePresetAssetMap[asset.Key],
                                assetData = assetMediator.SerializeToJson(),
                                runtimeOnly = runtimeOnly
                            };
                        }
                    );
                }
                catch (Exception e)
                {
                    Logger.Error
                    (
                        $"Error occurred while serializing Asset {asset.Key} of Type {asset.Value.AssetTypeId}",
                        "GigAssetManager",
                        e
                    );
                }

                if (serializedAssetData != null)
                {
                    serializedAssets.Add(serializedAssetData);
                }
            }
        }

        public async UniTask LoadStagesAndInstantiateAssetsAsync(IEnumerable<Stage> stagesToLoad,
            IEnumerable<Asset> assetsToInstantiate, CancellationToken cancellationToken)
        {
            List<Stage> stages = stagesToLoad != null ? stagesToLoad.ToList() : new List<Stage>();

            if (stages.Count > 0)
            {
                // Load existing stages.
                StageManager.LoadStages(stages);
            }
            else
            {
                Logger.Warning($"The Scenario has 0 stages in the session plan. Creating default stage.", "GigAssetManager");

                // No stages provided, create one.
                var stage = StageManager.CreateStage();
                stages.Add(stage);
            }

            cancellationToken.ThrowIfCancellationRequested();

            StageManager.SwitchToStage(stages.First().StageId);

            await InstantiateAssetsAsync(assetsToInstantiate, cancellationToken);
        }

        public async UniTask ReloadStagesAndAssetsAsync(object scenarioData, Guid currentStageId)
        {
            var scenario = (Scenario)scenarioData;

            List<Stage> stages = scenario.stages?.ToList() ?? new List<Stage>();

            if (stages.Count > 0)
            {
                // Load existing stages.
                StageManager.LoadStages(stages);
            }
            else
            {
                Logger.Warning($"The Scenario has 0 stages in the session plan. Creating default stage.", "GigAssetManager");

                // No stages provided, create one.
                var stage = StageManager.CreateStage();
                stages.Add(stage);
            }

            StageManager.SwitchToStage(currentStageId == Guid.Empty ? stages.First().StageId : currentStageId);

            // Do not cancel when reloading
            await InstantiateAssetsAsync(scenario.assets, CancellationToken.None, true);
        }

        public void UpdateAssetProperty(Guid assetId, string propertyName, object value)
        {
            IAssetMediator assetMediator = GetById(assetId)?.GetComponent<IAssetMediator>();

            if (assetMediator != null)
            {
                assetMediator.SetAssetProperty(propertyName, value);
            }
            else
            {
                Logger.Warning
                    ($"Failed while attempting to update an asset that does not exist with ID {assetId}", "GigAssetManager");
            }
        }

        public void UpdateAssetProperty(Guid assetId, string propertyName, byte[] value)
        {
            IAssetMediator assetMediator = GetById(assetId)?.GetComponent<IAssetMediator>();

            if (assetMediator != null)
            {
                assetMediator.SetAssetProperty(propertyName, value);
            }
            else
            {
                Logger.Warning
                    ($"Failed while attempting to update an asset that does not exist with ID {assetId}", "GigAssetManager");
            }
        }

        public void SyncAllRuntimeRoomData()
        {
            // Iterate through all assets and tell them to sync their components
            foreach (var currentAsset in instantiatedAssets.Values)
            {
                var currentMediator = currentAsset.GameObject.GetComponent<IAssetMediator>();

                currentMediator.SyncRuntimeRoomData();
            }

            foreach (var currentAsset in runtimeInstantiatedAssets.Values)
            {
                var currentMediator = currentAsset.GameObject.GetComponent<IAssetMediator>();

                currentMediator.SyncRuntimeRoomData();
            }
        }

        private void InvokeAllAssetEvent(bool reload)
        {
            if (reload)
            {
                AllAssetsReloaded?.InvokeSafely(this, new AllAssetsReloadedEventArgs());
            }
            else
            {
                AllAssetsInstantiated?.InvokeSafely(this, new AllAssetInstantiatedEventArgs());
            }
        }

        /// <summary>
        /// Instantiate the listed assets. Intended only for use when loading a scenario;
        /// instantiations made after the scenario is loaded should be done
        /// directly with Instantiate.
        /// </summary>
        /// <param name="assetsToLoad"></param>
        /// <param name="reload"></param>
        /// <returns></returns>
        private async UniTask InstantiateAssetsAsync(IEnumerable<Asset> assetsToLoad, CancellationToken cancellationToken, bool reload = false)
        {
            // No assets, do nothing
            if (assetsToLoad == null)
            {
                InvokeAllAssetEvent(reload);

                return;
            }

            IEnumerable<Asset> toLoad = assetsToLoad.ToList();

            if (!toLoad.Any())
            {
                InvokeAllAssetEvent(reload);

                return;
            }

            // Destroy any assets that exist locally but not on the network on reload.
            if (reload)
            {
                List<UniTask> destructionTasks = new List<UniTask>();

                HashSet<Guid> assetsToKeep = new HashSet<Guid>(toLoad.Select(asset => asset.AssetId));

                foreach (var asset in AllInstantiatedAssets)
                {
                    if (!assetsToKeep.Contains(asset.Key))
                    {
                        destructionTasks.Add
                        (
                            UniTask.Create
                            (
                                async () =>
                                {
                                    try
                                    {
                                        Destroy(asset.Key, true);
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Error
                                        (
                                            $"Error while trying to destroy Asset {asset.Key}",
                                            "GigAssetManager",
                                            ex
                                        );
                                    }
                                }
                            )
                        );
                    }
                }

                await UniTask.WhenAll(destructionTasks);
            }

            List<UniTask> instantiationTasks = new List<UniTask>();
            List<CancellationTokenSource> assetCancellationTokenSources = new List<CancellationTokenSource>();
            HashSet<string> dependencyAssetTypeIds = new HashSet<string>();

            // Create a set of cancellation tokens for the delay each asset will have at the beginning of each instantiation. If canceled,
            // then the previous asset was faster than the delay was needed for the next asset
            // TODO Is there a one liner/LINQ/better way for this?
            for (int i = 0; i < toLoad.Count(); i++)
            {
                assetCancellationTokenSources.Add(new CancellationTokenSource());
            }

            for (int i = 0; i < toLoad.Count(); i++)
            {
                var asset = toLoad.ElementAt(i);
                var index = i;

                instantiationTasks.Add
                (
                    UniTask.RunOnThreadPool
                    (
                        async () =>
                        {
                            try
                            {
                                // Wait to instantiate itself based on it's index in the list to avoid
                                // long frame delays as instantiation must occur on the main thread
                                if (ProfileManager.PerformanceProfile.WaitTimeBetweenAssetGroupsMilliseconds > 0)
                                    await UniTask.Delay
                                    (
                                        index * ProfileManager.PerformanceProfile.WaitTimeBetweenAssetGroupsMilliseconds,
                                        true,
                                        PlayerLoopTiming.Update,
                                        assetCancellationTokenSources[index].Token
                                    );
                                else
                                    await UniTask.DelayFrame
                                        (index, PlayerLoopTiming.Update, assetCancellationTokenSources[index].Token);
                            }
                            catch (OperationCanceledException _)
                            {
                                // Do nothing, the previous task has finished before the delay was complete, move on now
                            }

                            // Since this is on the thread pool, just return
                            if (cancellationToken.IsCancellationRequested)
                            {
                                return;
                            }

                            GameObject gameObject = null;
                            Guid assetId = Guid.Parse(asset.assetId);

                            if (reload)
                            {
                                gameObject = instantiatedAssets.TryGetValue
                                    (assetId, out InstantiatedAsset instantiatedAsset)
                                    ?
                                    instantiatedAsset.GameObject
                                    : runtimeInstantiatedAssets.TryGetValue(assetId, out InstantiatedAsset runtimeAsset)
                                        ? runtimeAsset.GameObject
                                        : null;

                                // Return to main thread now for the Instantiate and TryGetComponent calls below
                                await UniTask.SwitchToMainThread();

                                try
                                {
                                    // instantiate missing assets (i.e. runtime-instantiations made before joining)
                                    // necessary when updating on ephemeral data update
                                    if (gameObject == null)
                                    {
                                        gameObject = await Instantiate
                                        (
                                            new AssetInstantiationArgs
                                                (asset.assetTypeId, assetId, asset.presetAssetId, true, false, asset.runtimeOnly)
                                        );
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.Error
                                    (
                                        $"Error while trying to reload Asset {asset.presetAssetId}, AssetTypeId:{asset.assetTypeId}, AssetId:{asset.AssetId}",
                                        "GigAssetManager",
                                        ex
                                    );
                                }
                            }
                            else
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    return;
                                }

                                await UniTask.SwitchToMainThread();

                                try
                                {
                                    gameObject = await Instantiate
                                    (
                                        new AssetInstantiationArgs
                                            (asset.assetTypeId, assetId, asset.presetAssetId, false, false, asset.runtimeOnly)
                                    );
                                }
                                catch (Exception ex)
                                {
                                    Logger.Error
                                    (
                                        $"Error while trying to instantiate the Asset {asset.presetAssetId}, AssetTypeId:{asset.assetTypeId}, AssetId:{asset.AssetId}",
                                        "GigAssetManager",
                                        ex
                                    );
                                }
                            }

                            if (gameObject != null && gameObject.TryGetComponent(out IAssetMediator assetMediator))
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    return;
                                }

                                var allAssetTypeComponents = gameObject.GetComponents<IAssetTypeComponent>();

                                await UniTask.SwitchToThreadPool();

                                // mark the ATCs as runtime-instances, so they can allow runtime data changes on validate.
                                foreach (var assetTypeComponent in allAssetTypeComponents)
                                {
                                    assetTypeComponent.IsRuntimeInstance = true;
                                }

                                assetMediator.SetRuntimeID(Guid.Parse(asset.assetId), asset.presetAssetId);

                                assetMediator.SetStage(StageManager.CurrentStage.StageId);

                                try
                                {
                                    if (cancellationToken.IsCancellationRequested)
                                    {
                                        return;
                                    }

                                    await assetMediator.DeserializeFromJson(asset.assetData, allAssetTypeComponents);

                                    // Check to see if the asset that was just created is a Content Marker and save the reference if so
                                    if (gameObject.TryGetComponent(out ContentMarkerAssetTypeComponent contentMarker))
                                    {
                                        if (ContentMarkerAsset != null)
                                        {
                                            Logger.Warning($"Another content marker ({ContentMarkerAsset.PresetAssetId}) was set in this scenario, but now {asset.presetAssetId} will be used.",
                                                            "GigAssetManager");
                                        }

                                        ContentMarkerAsset = assetMediator;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.Error
                                    (
                                        $"Error while trying to serialize data for Asset {asset.presetAssetId}, AssetTypeId:{asset.assetTypeId}, AssetId:{asset.AssetId}\n{asset.assetData}",
                                        "GigAssetManager",
                                        ex
                                    );
                                }
                            }

                            // Your task is complete, cancel the delay for the next asset so it can start it's process
                            if (index < toLoad.Count() - 1)
                            {
                                assetCancellationTokenSources[index + 1].Cancel();
                            }
                        }
                    )
                );
            }

            await UniTask.WhenAll(instantiationTasks);

            if (!cancellationToken.IsCancellationRequested)
            {
                await UniTask.SwitchToMainThread();

                InvokeAllAssetEvent(reload);
            }
        }

        private bool DestroyInternal(Guid assetId)
        {
            if (ContentMarkerAsset != null &&
               assetId == ContentMarkerAsset.AssetId)
            {
                ContentMarkerAsset = null;
            }

            if (instantiatedAssets.TryGetValue(assetId, out var instantiatedAsset))
            {
                instantiatedAssets.Remove(assetId);

                if (reversePresetAssetMap.ContainsKey(assetId))
                {
                    presetAssetMap.Remove(reversePresetAssetMap[assetId]);
                    reversePresetAssetMap.Remove(assetId);
                }

                // If the GameObject was destroyed without being routed through here,
                // then trying to find its AssetMediator to unregister will create an NRE.
                if (instantiatedAsset.GameObject != null)
                {
                    forwardPropertyUpdateHandler.UnregisterPropertyUpdateEventHandler(instantiatedAsset.GameObject);

                    Object.Destroy(instantiatedAsset.GameObject);
                }
                else
                {
                    Logger.Warning
                    (
                        $"An asset with type id {instantiatedAsset.AssetTypeId} was destroyed outside of GigAssetManager.DestroyInternal.",
                        "GigAssetManager"
                    );
                }

                return true;
            }
            else if (runtimeInstantiatedAssets.TryGetValue(assetId, out instantiatedAsset))
            {
                runtimeInstantiatedAssets.Remove(assetId);

                if (reversePresetAssetMap.ContainsKey(assetId))
                {
                    presetAssetMap.Remove(reversePresetAssetMap[assetId]);
                    reversePresetAssetMap.Remove(assetId);
                }

                // If the GameObject was destroyed without being routed through here,
                // then trying to find its AssetMediator to unregister will create an NRE.
                if (instantiatedAsset.GameObject != null)
                {
                    forwardPropertyUpdateHandler.UnregisterPropertyUpdateEventHandler(instantiatedAsset.GameObject);

                    Object.Destroy(instantiatedAsset.GameObject);
                }
                else
                {
                    Logger.Warning
                    (
                        $"An asset with type id {instantiatedAsset.AssetTypeId} was destroyed outside of GigAssetManager.DestroyInternal.",
                        "GigAssetManager"
                    );
                }

                return true;
            }

            return false;
        }

        // On stage switch update the stage on all the Assets.
        private void StageManagerOnStageSwitched(object sender, StageSwitchedEventArgs args)
        {
            foreach (InstantiatedAsset instantiatedAsset in instantiatedAssets.Values)
            {
                if (instantiatedAsset.GameObject.TryGetComponent(out IAssetMediator assetMediator))
                {
                    assetMediator.SetStage(StageManager.CurrentStage.StageId);
                }
            }

            foreach (InstantiatedAsset instantiatedAsset in runtimeInstantiatedAssets.Values)
            {
                if (instantiatedAsset.GameObject.TryGetComponent(out IAssetMediator assetMediator))
                {
                    assetMediator.SetStage(StageManager.CurrentStage.StageId);
                }
            }
        }

        /// <summary>
        /// Instantiate the interactable and assign it the input values.
        /// </summary>
        /// <param name="instantiationArgs"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        protected async virtual UniTask HandleInstantiation(AssetInstantiationArgs instantiationArgs, Vector3 position,
            Quaternion rotation)
        {
            GameObject assetGameObject = await Instantiate(instantiationArgs);
            IAssetMediator interactable = assetGameObject.EnsureComponent<AssetMediator>();

            if (interactable != null)
            {
                // TODO avoid these property updates on assets without position and rotation ATCs
                interactable.SetAssetProperty(nameof(PositionAssetData.position), position);
                interactable.SetAssetProperty(nameof(RotationAssetData.rotation), rotation);
            }
        }

        /// <summary>
        /// Instantiate the input prefab at the input (calibrated) local position and local rotation.
        /// </summary>
        /// <param name="instantiationArgs"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public async void InstantiateInteractablePositionedAndOriented(AssetInstantiationArgs instantiationArgs, Vector3 position,
            Quaternion rotation)
        {
            if (instantiationArgs.AssetId == Guid.Empty)
                instantiationArgs.AssetId = Guid.NewGuid();

            await HandleInstantiation(instantiationArgs, position, rotation);
        }

        private async UniTask ForAllAssets(Action<InstantiatedAsset> action)
        {
            // Same as with Play mode, we still need to disable assets in Pause mode, which exists in the both the 
            // Enabled and Disabled PlayEdit Mode states.
            await AllInstantiatedAssetsAsync.ForEachAsync
                (
                    instantiatedAsset =>
                    {
                        try
                        {
                            action?.Invoke(instantiatedAsset.Item2);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                );
        }

        public async UniTask DisableInteractivityForAllAssetsAsync()
        {
            // Same as with Play mode, we still need to disable assets in Pause mode, which exists in the both the 
            // Enabled and Disabled PlayEdit Mode states.
            await ForAllAssets(instantiatedAsset =>
            {
                var mrtkComponent = instantiatedAsset.GameObject.GetComponent<MRTKAssetTypeComponent>();

                if (mrtkComponent != null)
                {
                    mrtkComponent.DisableManipulation();
                    mrtkComponent.HideBoundsControl();
                }
            });
        }

        public async UniTask EnableOrDisableInteractivityAsync()
        {
            // Spec: https://docs.google.com/document/d/1xJ4ncknQL8r2EdZaO4_u_DqDJ_BOpQs3jSZDEMFptOM/edit
            await AllInstantiatedAssetsAsync.ForEachAsync
                (
                    instantiatedAsset =>
                    {
                        try
                        {
                            var mrtkComponent = instantiatedAsset.Item2.GameObject
                            .GetComponent<MRTKAssetTypeComponent>();

                            if (mrtkComponent == null)
                                return;

                            // Don't enable the assets if they are not visible
                            if (!AssetsAreVisible)
                            {
                                // No manipulation is available because position, scale, and rotation are not editable.
                                mrtkComponent.DisableManipulation();
                                mrtkComponent.HideBoundsControl();

                                return;
                            }

                            // MRTKAssetTypeComponent requires transform syncing ATCs, so we know these exist
                            var positionComponent = instantiatedAsset.Item2.GameObject
                            .GetComponent<PositionAssetTypeComponent>();

                            var rotationComponent = instantiatedAsset.Item2.GameObject
                                .GetComponent<RotationAssetTypeComponent>();

                            var scaleComponent = instantiatedAsset.Item2.GameObject
                                .GetComponent<ScaleAssetTypeComponent>();

                            var isMovable = true;

                            TransformFlags manipulationFlags = TransformFlags.Rotate;

                            // Check scale editability 
                            if (scaleComponent.IsScaleEditableByAuthor)
                            {
                                manipulationFlags |= TransformFlags.Scale;
                            }

                            // Check position editability 
                            if (isMovable)
                            {
                                manipulationFlags |= TransformFlags.Move;
                            }

                            if (manipulationFlags == 0)
                            {
                                // No manipulation is available because position, scale, and rotation are not editable.
                                mrtkComponent.DisableManipulation();
                                mrtkComponent.HideBoundsControl();
                            }
                            else
                            {
                                // Some manipulation is available.
                                mrtkComponent.EnableManipulation(manipulationFlags);
                                mrtkComponent.SetBoundsControl(showRotationHandles: false,
                                                               showScaleHandles: scaleComponent.IsScaleEditableByAuthor);
                                mrtkComponent.ActivateBoundControl(true);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                );
        }

        public UniTask EnableOrDisableInteractivityForPlayScenarioAsync()
        {
            // Spec: https://docs.google.com/document/d/1xJ4ncknQL8r2EdZaO4_u_DqDJ_BOpQs3jSZDEMFptOM/edit
            // This method should not be hidden behind the FeatureManger for PlayEdit Mode, because 'Play Mode' is the default
            // state that exists in both the Enabled and Disabled state, so we need bounding boxes hidden and the like, while
            // still allowing the authors to decide what objects can be moved while the scenario is playing
            AllInstantiatedAssetsAsync.ForEachAsync
                (
                    instantiatedAsset =>
                    {
                        try
                        {
                            var mrtkComponent = instantiatedAsset.Item2.GameObject.GetComponent<MRTKAssetTypeComponent>();

                            if (mrtkComponent == null)
                                return;

                            // No BoundsControl for any assets in Play Mode.
                            mrtkComponent.HideBoundsControl();

                            var assetMediator = instantiatedAsset.Item2.GameObject.GetComponent<IAssetMediator>();

                            // the asset has an MRTKAssetTypeComponent, therefore it has a PositionAssetTypeComponent
                            var isMovableInPlayMode = true;

                            if (isMovableInPlayMode)
                            {
                                mrtkComponent.EnableManipulation(TransformFlags.Move);
                                return;
                            }

                            // IsMovable is a Play Mode construct (ignored in Edit Mode) so if it is 
                            // disabled then this asset should have its interaction disabled.
                            mrtkComponent.DisableManipulation();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                );

            return UniTask.CompletedTask;
        }

        #region IDisposableImplementation

        public void Dispose()
        {
            StageManager.StageSwitched -= StageManagerOnStageSwitched;
        }

        #endregion
    }
}