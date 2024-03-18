using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Networking;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.Data;
using GIGXR.Platform.Scenarios.EventArgs;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Scenarios.GigAssets.Data;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace GIGXR.Platform.CommonAssetTypes
{
    /// <summary>
    /// Enforces exclusive manipulation of an asset on the network.
    /// Raises events when manipulation is registered on the network.
    /// </summary>
    [RequireComponent(typeof(PositionAssetTypeComponent))] // at least position ATC must exist
    [RequireComponent(typeof(NetworkAuthorityAssetTypeComponent))] // manipulation requires photon ownership management
    public class ManipulationAssetTypeComponent : BaseAssetTypeComponent<ManipulationAssetData>, IManipulatorProvider
    {
        #region Serialized Configuration

        [Tooltip("Object manipulator to network. Will default to the manipulator on this GameObject if not specified.")]
        [SerializeField] private ObjectManipulator objectManipulator;

        [Tooltip("Bounds control to network. Will default to the bounds control on this GameObject if not specified.")]
        [SerializeField] private BoundsControl boundsControl;

        [Tooltip("If true, the bounds control will be turned on/off alongside the object manipulator.")]
        [SerializeField] private bool manageBoundsControlEnabledState = false;

        [Tooltip("Box collider use for manipulation.")]
        [SerializeField] private Collider manipulationCollider;

        [Tooltip("If true, the manipulation collider will be turend on/off with the object manipulator.")]
        [SerializeField] private bool manageManipulationColliderEnabledState = false;

        #endregion

        #region Private Data

        // the position asset, which determines whether the asset is movable in play mode
        private PositionAssetTypeComponent positionATC;

        // used to manage the ObjectManipulator so it is disabled for users without authority to use it
        private NetworkAuthorityAssetTypeComponent networkAuthorityATC;

        // used to derive current desired manipulation state of asset
        private MRTKAssetTypeComponent mrtkATC;

        // used to check whether the local user has authority via the NetworkAuthorityATC
        private Guid LocalPlayerId = Guid.Empty;

        // paranoiac; used to avoid redundant manipulation events in the case of redundant property updates
        private bool lastIsManipulatingValue;

        // used to cache authority status, so redundant derivations can be avoided
        private bool hasManipulationAuthority = true;

        // true if the local user is manipulating the ObjectManipulator
        private bool isManipulatingLocally;

        private ScenarioStatus cachedScenarioStatus;

        #endregion

        #region AssetTypeComponent Implementation

        public override void SetEditorValues()
        {

        }

        protected override void Setup()
        {
            // if we have neither a serialized reference nor a sibling ObjectManipulator, die.
            if (objectManipulator == null && !TryGetComponent<ObjectManipulator>(out objectManipulator))
            {
                Debug.LogError($"{GetType()} requires an ObjectManipulator");
                return;
            }

            // set up object manipulator listeners
            objectManipulator.OnManipulationStarted.AddListener((e) => OnLocalManipulationStarted());
            objectManipulator.OnManipulationEnded.AddListener((e) => OnLocalManipulationEnded());

            // set up network authority updates
            networkAuthorityATC = GetComponent<NetworkAuthorityAssetTypeComponent>();
            networkAuthorityATC.AuthorityStateUpdated.AddListener(UpdateAuthorityState);

            // get current authority state
            hasManipulationAuthority = LocalPlayerId == Guid.Empty || networkAuthorityATC.HasAuthority(LocalPlayerId);

            if (boundsControl == null)
            {
                TryGetComponent<BoundsControl>(out boundsControl);
            }

            // if there is and MRTKATC, let it do the work of determining when manipulation is allowed
            mrtkATC = GetComponent<MRTKAssetTypeComponent>();

            // if there is not an MRTKATC, assume the constraints are pre-configured on manipulation assets, but do the work of managing their enabled state here.
            if (mrtkATC == null)
            {
                // get position ATC reference, to be used in determining whether manipulation is allowed in the current scenario status
                positionATC = GetComponent<PositionAssetTypeComponent>();

                // handle scenario status changes
                cachedScenarioStatus = ScenarioManager.ScenarioStatus;
                ScenarioManager.ScenarioStatusChanged += OnScenarioStatusChanged;
            }

            UpdateManipulatorActivity();
        }

        protected override void Teardown()
        {
            networkAuthorityATC.AuthorityStateUpdated.RemoveListener(UpdateAuthorityState);

            if (mrtkATC == null)
            {
                ScenarioManager.ScenarioStatusChanged -= OnScenarioStatusChanged;
            }

            if (objectManipulator != null)
            {
                objectManipulator.OnManipulationStarted.RemoveListener((e) => OnLocalManipulationStarted());
                objectManipulator.OnManipulationEnded.RemoveListener((e) => OnLocalManipulationEnded());
            }
        }

        #endregion

        #region IManipulationProvider Implementation

        public ObjectManipulator ObjectManipulator
        {
            get
            {
                return objectManipulator;
            }
        }

        public BoundsControl BoundsControl
        {
            get
            {
                return boundsControl;
            }
        }

        #endregion

        #region Dependencies

        private IScenarioManager ScenarioManager;

        [InjectDependencies]
        public void Construct(INetworkManager networkManager, IScenarioManager scenarioManager)
        {
            string id = networkManager?.LocalPlayer?.UserId ?? string.Empty;
            LocalPlayerId = string.IsNullOrEmpty(id) ? Guid.Empty : Guid.Parse(id);

            ScenarioManager = scenarioManager;
        }

        #endregion

        #region Property Update Handlers

        [RegisterPropertyChange(nameof(ManipulationAssetData.isManipulating))]
        private void OnIsManipulatingChanged(AssetPropertyChangeEventArgs e)
        {
            var isManipulating = (bool)e.AssetPropertyValue;

            if (isManipulating != lastIsManipulatingValue)
            {
                // update the active state of the ObjectManipulator
                UpdateManipulatorActivity();

                lastIsManipulatingValue = isManipulating;

                // invoke manipulation events
                if (isManipulating)
                {
                    OnManipulationStarted?.Invoke();
                }
                else
                {
                    OnManipulationEnded?.Invoke();
                }
            }
        }

        // only registered conditionally, see Setup
        private void OnIsMovableInPlayModeChanged(AssetPropertyChangeEventArgs e)
        {
            if (IsInitialized)
            {
                UpdateManipulatorActivity();
            }
        }

        #endregion

        #region Local Manipulation Event Handlers

        private void OnLocalManipulationStarted()
        {
            isManipulatingLocally = true;
            assetData.isManipulating.runtimeData.Value = true;
        }

        private void OnLocalManipulationEnded()
        {
            isManipulatingLocally = false;
            assetData.isManipulating.runtimeData.Value = false;
        }

        #endregion

        #region Private Methods

        private void UpdateManipulatorActivity()
        {
            // either the local user is the one manipulating, or nobody is manipulating
            bool manipulationStateAllowsLocalManipulation = (isManipulatingLocally || !assetData.isManipulating.runtimeData.Value);

            // either this has an MRTK ATC and it is marked movable or the position ATC marks it movable in the current scenario status
            bool transformAssetsAllowManipulation;
            if (mrtkATC == null)
            {
                transformAssetsAllowManipulation = (cachedScenarioStatus == ScenarioStatus.Playing);
            }
            else
            {
                transformAssetsAllowManipulation = mrtkATC.ManipulationEnabled;
            }

            bool enable = hasManipulationAuthority &&
                manipulationStateAllowsLocalManipulation &&
                transformAssetsAllowManipulation;

            objectManipulator.enabled = enable;

            if (manageBoundsControlEnabledState && boundsControl != null)
            {
                boundsControl.enabled = enable;
            }

            if (manageManipulationColliderEnabledState && manipulationCollider != null)
            {
                manipulationCollider.enabled = enable;
            }
        }

        // update hasManipulationAuthority to reflect whether the local user has authority
        private void UpdateAuthorityState()
        {
            // local id will be Guid.Empty in non-networked scenarios
            hasManipulationAuthority = LocalPlayerId == Guid.Empty || networkAuthorityATC.HasAuthority(LocalPlayerId);

            if (IsInitialized)
            {
                UpdateManipulatorActivity();
            }
        }

        // only registered conditionally, see Setup
        private void OnScenarioStatusChanged(object sender, ScenarioStatusChangedEventArgs args)
        {
            if (IsInitialized)
            {
                cachedScenarioStatus = args.NewStatus;
                UpdateManipulatorActivity();
            }
        }

        #endregion

        #region Public

        // invoked for all users when any user starts / ends manipulation
        public UnityEvent OnManipulationStarted;
        public UnityEvent OnManipulationEnded;

        #endregion
    }

    [Serializable]
    public class ManipulationAssetData : BaseAssetData
    {
        // is someone currently manipulating
        public AssetPropertyDefinition<bool> isManipulating;
    }
}
