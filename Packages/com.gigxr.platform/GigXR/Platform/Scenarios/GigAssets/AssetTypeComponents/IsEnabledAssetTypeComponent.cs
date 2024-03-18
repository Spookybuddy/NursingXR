namespace GIGXR.Platform.Scenarios.GigAssets
{
    using Core.DependencyInjection;
    using Data;
    using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
    using Scenarios.Data;
    using Scenarios.EventArgs;
    using UnityEngine;

    /// <summary>
    /// The component responsible for handling whether an interactable is active or not.
    /// </summary>
    public class IsEnabledAssetTypeComponent : BaseAssetTypeComponent<IsEnabledAssetData>
    {
        #region Private Variables

        private GameObject displayRoot;

        #endregion

        #region Public Variables
        
        /// <summary>
        /// Display Root Transform. 
        /// </summary>
        public Transform DisplayRootTransform
        {
            get
            {
                if (displayRoot == null)
                {
                    return null;
                }

                return displayRoot.transform;
            }
        }

        /// <summary>
        /// Returns true if asset is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return assetData.isEnabled.runtimeData.Value;
            }
        }

        #endregion

        private IGigAssetManager AssetManager;
        private IScenarioManager scenarioManager;
        private ScenarioStatus cachedScenarioStatus;

        [InjectDependencies]
        public void Construct(IGigAssetManager assetManager, IScenarioManager scenarioManagerReference)
        {
            AssetManager = assetManager;
            scenarioManager = scenarioManagerReference;
            cachedScenarioStatus = scenarioManager.ScenarioStatus;

            // ScenarioManager.ScenarioPlaying += (object sender, ScenarioPlayingEventArgs e)
            // => SetAssetEnableStateForClients();

            // TODO - subscribe to host transfer events in some smart way 
        }

        #region Unity Lifecycle

        /// <summary>
        /// Sets up asset references.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            ValidateDisplayRoot();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks if DisplayRoot exists, and verifies that it is set up correctly.
        /// Creates DisplayRoot GO if needed. 
        /// </summary>
        private void ValidateDisplayRoot()
        {
            // Create DisplayRoot if this is a new AssetType.
            if (transform.childCount == 0)
            {
                displayRoot = Instantiate(new GameObject(), transform);
                displayRoot.name = "DisplayRoot";
            }
            else if (displayRoot == null)
            {
                displayRoot = transform.Find("DisplayRoot")?.gameObject;
            }

            // Sometimes this happens when you copy/paste game objects
            if (transform.childCount > 1)
            {
                Debug.LogWarning
                    (
                        $"Please add one GameObject called DisplayRoot to this asset ({gameObject.name}), and nest everything beneath DisplayRoot."
                    );
            }
        }

        private void SetAssetVisibilityAndInteractability(bool isEnabled)
        {
            SetAssetVisibility(isEnabled);
            SetAssetInteractability(isEnabled);
        }

        /// <summary>
        /// Toggles active state of DisplayRoot and everything beneath it.
        /// </summary>
        /// <param name="isEnabled"></param>
        private void SetAssetVisibility(bool isEnabled)
        {
            displayRoot.SetActive(isEnabled);
        }

        /// <summary>
        /// Sets collider active states at the root level of the asset type.
        /// </summary>
        /// <param name="isEnabled"></param>
        private void SetAssetInteractability(bool isEnabled)
        {
            // This needs reconsideration, as just turning all colliders on/off interferes with other ATCs' component management.
            // TODO - verify this change
            // I think this interferes with positon ATC changes

            // foreach (var colliderComponent in colliderComponents)
            // {
            //     colliderComponent.enabled = isEnabled;
            // }
        }

        #endregion

        [RegisterPropertyChange(nameof(IsEnabledAssetData.isEnabledHostOnly))]
        private void HandleEnabledHostOnlyChange(AssetPropertyChangeEventArgs e)
        {
            UpdateEnabledState();
        }

        [RegisterPropertyChange(nameof(IsEnabledAssetData.isEnabled))]
        private void HandleEnabledChange(AssetPropertyChangeEventArgs e)
        {
            UpdateEnabledState();
        }

        private void UpdateEnabledState()
        {
            bool isEnabled = assetData.isEnabled.runtimeData.Value;
            bool isEnabledHostOnly = assetData.isEnabledHostOnly.runtimeData.Value;
            bool isHost = AssetManager.AssetContext.IsScenarioAuthority;

            if (isEnabled)
            {
                bool enabledForLocalUser = !isEnabledHostOnly || isHost;
                SetAssetVisibilityAndInteractability(enabledForLocalUser);
            }
            else
            {
                SetAssetVisibilityAndInteractability(false);
            }
        }


        #region BaseAssetTypeComponent Implementation

        protected override void Setup()
        {
            scenarioManager.ScenarioStatusChanged += OnScenarioStatusChanged;
        }

        private void OnScenarioStatusChanged(object sender, ScenarioStatusChangedEventArgs e)
        {
            cachedScenarioStatus = e.NewStatus;
            UpdateEnabledState();
        }

        protected override void Teardown()
        {
            scenarioManager.ScenarioStatusChanged -= OnScenarioStatusChanged;
        }

        public override void SetEditorValues()
        {
            assetData.name.designTimeData.defaultValue = "Is Enabled";

            assetData.description.designTimeData.defaultValue
                = "Sets the asset's active state in the scenario.";

            assetData.isEnabled.designTimeData.defaultValue = true;
            assetData.isEnabled.designTimeData.isEditableByAuthor = true;
        }

      
        
        #endregion
    }
}