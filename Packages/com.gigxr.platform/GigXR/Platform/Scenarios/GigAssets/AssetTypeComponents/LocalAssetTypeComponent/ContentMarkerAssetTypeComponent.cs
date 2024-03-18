using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Scenarios.GigAssets.Data;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;

namespace GIGXR.Platform.Scenarios.GigAssets
{
    /// <summary>
    /// This component establishes an asset as the 'Content Marker' for a scenario. Setup the needed data with this component. When needed,
    /// this will use the position and direction of the proxy GameObject as the set Content Marker position and rotation. 
    /// </summary>
    public class ContentMarkerAssetTypeComponent : LocalAssetTypeComponent
    {
        [Tooltip("Mark true if the forward vector of the GameObject points in the opposite direction.")]
        public bool reverseForward;

        [Tooltip("Set the GameObject that will be replicated to show the asset, but this will be a proxy that the user moves.")]
        public GameObject modelDuplication;

        [Tooltip("The position of the button to move the scene, local to the model itself.")]
        public Vector3 buttonOffset;

        public Vector3 RootOffset => loadedPosition;

        public Quaternion RotationOffset => loadedRotation;

        public Bounds ModelBounds => modelBounds;

        public Transform ProxyTransform => proxyModel?.transform;

        private Vector3 loadedPosition;

        private Quaternion loadedRotation;

        private GameObject proxyModel;

        private Bounds modelBounds;

        // Always null check, this will not exist for client users
        public GameObject MoveSceneButton
        {
            get
            {
                if(_moveSceneButton == null)
                {
                    SetSceneButton();
                }

                return _moveSceneButton;
            }
        }

        private GameObject _moveSceneButton;

        #region Dependencies

        private IScenarioManager ScenarioManager { get; set; }

        private ProfileManager ProfileManager { get; set; }

        #endregion

        [InjectDependencies]
        public void Construct(IScenarioManager scenarioManager, ProfileManager profileManager)
        {
            ScenarioManager = scenarioManager;
            ProfileManager = profileManager;

            ScenarioManager.ScenarioStatusChanged += ScenarioManager_ScenarioStatusChanged;
        }

        protected override void AssetTypeDependenciesConstructed()
        {
            AssetManager.ContentMarkerUpdated += AssetManager_ContentMarkerUpdated;
        }

        private void AssetManager_ContentMarkerUpdated(object sender, ContentMarkerUpdateEventArgs e)
        {
            SetButtonState(false);
        }

        private void Awake()
        {
            if (modelDuplication != null)
            {
                Renderer[] renderers = modelDuplication.GetComponentsInChildren<Renderer>();

                // Start with the bounds of the first renderer
                modelBounds = renderers[0].bounds;

                foreach (Renderer currentRenderer in renderers) 
                {
                    modelBounds.Encapsulate(currentRenderer.bounds); 
                }
            }
        }

        private void OnDestroy()
        {
            if (ScenarioManager != null)
            {
                ScenarioManager.ScenarioStatusChanged -= ScenarioManager_ScenarioStatusChanged;
            }

            if (AssetManager != null)
            {
                AssetManager.ContentMarkerUpdated -= AssetManager_ContentMarkerUpdated;
            }
        }

        public GameObject GetPlacementMarker(ContentMarkerAssetTypeComponent original)
        {
            if(proxyModel == null )
            {
                proxyModel = Instantiate(modelDuplication);

                // We need to disable animations on the proxy object otherwise the user won't be able to move it
                var proxyAnimator = proxyModel.GetComponentInChildren<Animator>();
                if (proxyAnimator != null)
                {
                    proxyAnimator.enabled = false;
                }
                
                // The proxy model might be hidden during loading, just make sure it's in the default layer here
                proxyModel.SetLayerRecursively(0);

                var proxyContentMarker = proxyModel.EnsureComponent<ContentMarkerAssetTypeComponent>();
                proxyContentMarker.Copy(original);
            }

            return proxyModel;
        }

        public void SetButtonState(bool value)
        {
            MoveSceneButton?.SetActive(value);
        }

        public void DestroyPlacementMarker()
        {
            if(proxyModel != null)
            {
                Destroy(proxyModel);
            }
        }

        public void Copy(ContentMarkerAssetTypeComponent other)
        {
            reverseForward = other.reverseForward;

            loadedPosition = other.RootOffset;

            loadedRotation = other.RotationOffset;

            modelBounds = other.ModelBounds;
        }

        private void SetSceneButton()
        {
            // Only create the button if you have the authority
            if (_moveSceneButton == null && 
                ScenarioManager.AssetManager.AssetContext.IsScenarioAuthority)
            {
                _moveSceneButton = CreateSceneButton(transform);
            }
        }
        
        public GameObject CreateSceneButton(Transform parent = null)
        {
            GameObject sceneButton;

            // This button prefab is already configured to start the content marker on button click
            if (parent == null)
            {
                sceneButton = AssetManager.Instantiate(new PrefabInstantiationArgs(ProfileManager.CalibrationProfile.MoveScenarioButtonPrefab, true));
            }
            else
            {
                sceneButton = AssetManager.Instantiate(new PrefabInstantiationArgs(ProfileManager.CalibrationProfile.MoveScenarioButtonPrefab, false, parent));
            }

            sceneButton.transform.localPosition = buttonOffset;
            sceneButton.transform.localScale = Vector3.one * 5;

            return sceneButton;
        }

        private void ScenarioManager_ScenarioStatusChanged(object sender, Scenarios.EventArgs.ScenarioStatusChangedEventArgs e)
        {
            SetButtonState(false);
        }

        public void AdjustProxyDistanceToUser()
        {
            var cameraTransform = Camera.main.transform;

            // Make sure the bound's center is in the correct world space position
            modelBounds.center += proxyModel.transform.position;

            // Only adjust the position if the camera/user is inside the Content Marker
            if(modelBounds.Contains(cameraTransform.position))
            {
                // Find the position of the bounds that is located behind the user so we can push that
                // wall portion forward
                var pointBehindUser = -1 * modelBounds.size.z * cameraTransform.forward;

                var closestBoundedPoint = modelBounds.ClosestPoint(pointBehindUser);

                var distanceToBoundedWall = closestBoundedPoint - proxyModel.transform.position;

                // How much the content marker needs to move forwards so that the user is staring at the back
                var traversalDistance = modelBounds.size.z - Mathf.Abs(distanceToBoundedWall.z);

                // We do not want to push the content marker up or down, just forwards, so save the current y value
                var contentMarkerYPosition = transform.position.y;

                // Now push the content marker in the user's view direction by the amount needed to see the back wall
                proxyModel.transform.position += (cameraTransform.forward * traversalDistance);

                proxyModel.transform.position = new Vector3(proxyModel.transform.position.x, contentMarkerYPosition, proxyModel.transform.position.z);
            }

            modelBounds.center -= proxyModel.transform.position;
        }

        [RegisterPropertyChange(nameof(PositionAssetData.position))]
        private void HandlePositionChange(AssetPropertyChangeEventArgs e)
        {
            // When first loading, save the initial position of the object to use as the
            if (ScenarioManager.ScenarioStatus == Scenarios.Data.ScenarioStatus.Loading)
            {
                loadedPosition = (Vector3)e.AssetPropertyValue;
            }
            // When the scenario goes from Edit to Stopped, the position property is updated due to 
            // values in the Stage Data, but since it's a content marker, it needs to stay at the root
            // and not actually change
            else if (ScenarioManager.ScenarioStatus == Scenarios.Data.ScenarioStatus.Stopped)
            {
                transform.localPosition = loadedPosition;
            }
        }

        [RegisterPropertyChange(nameof(RotationAssetData.rotation))]
        private void HandleRotationChange(AssetPropertyChangeEventArgs e)
        {
            if (ScenarioManager.ScenarioStatus == Scenarios.Data.ScenarioStatus.Loading)
            {
                loadedRotation = (Quaternion)e.AssetPropertyValue;
            }
            if (ScenarioManager.ScenarioStatus == Scenarios.Data.ScenarioStatus.Stopped)
            {
                transform.localRotation = loadedRotation;
            }
        }
    }
}