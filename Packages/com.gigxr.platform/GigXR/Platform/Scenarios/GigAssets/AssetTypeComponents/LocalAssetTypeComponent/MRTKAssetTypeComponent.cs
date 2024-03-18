namespace GIGXR.Platform.Scenarios.GigAssets
{
    using Microsoft.MixedReality.Toolkit.Input;
    using Microsoft.MixedReality.Toolkit.UI;
    using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
    using Microsoft.MixedReality.Toolkit.Utilities;
    using UnityEngine;

    /// <summary>
    /// A component that provides methods for configuring MRTK components.
    /// </summary>
    [RequireComponent(typeof(ObjectManipulator))]
    [RequireComponent(typeof(BoundsControl))]
    public class MRTKAssetTypeComponent : LocalAssetTypeComponent
    {
        #region Serialized Variables

        /// <summary>
        /// Collider reference. This component does the following:
        ///     1. Disables this collider outside of Edit mode
        ///     2. Sets this as the main collider in BoundsControl if needed
        /// </summary>
        [SerializeField]
        [Header("If set, this collider will be disabled outside of Edit mode.")]
        private BoxCollider editModeCollider;

        #endregion

        #region PrivateVariables

        private ObjectManipulator objectManipulatorComponent;

        private BoundsControl boundsControlComponent;

        private AxisFlags? OriginalRotationConstraint;

        private RotationAxisConstraint rotationConstraint;

        #endregion

        #region UnityMethods

        private void Awake()
        {
            objectManipulatorComponent = GetComponent<ObjectManipulator>();
            boundsControlComponent = GetComponent<BoundsControl>();
            rotationConstraint = GetComponent<RotationAxisConstraint>();

            // We need to manually control the bounds control for assets to deal with things like handling the state during loading and Edit mode
            boundsControlComponent.BoundsControlActivation = Microsoft.MixedReality.Toolkit.UI.BoundsControlTypes.BoundsControlActivationType.ActivateManually;

#if UNITY_EDITOR
            // Set the collider in boundsControlComponent if needed
            if (editModeCollider != null &&
                boundsControlComponent.BoundsOverride == null)
                boundsControlComponent.BoundsOverride = editModeCollider;
#endif

            boundsControlComponent.RotationHandlesConfig = Instantiate(boundsControlComponent.RotationHandlesConfig);
            boundsControlComponent.ScaleHandlesConfig = Instantiate(boundsControlComponent.ScaleHandlesConfig);
        }

        #endregion

        #region PublicAPI

        private bool cachedManipulationEnabled;
        public bool ManipulationEnabled => cachedManipulationEnabled;

        public void OverrideConstraints(AxisFlags transformFlags)
        {
            if(rotationConstraint != null)
            {
                if(!OriginalRotationConstraint.HasValue)
                {
                    OriginalRotationConstraint = rotationConstraint.ConstraintOnRotation;
                }

                rotationConstraint.ConstraintOnRotation = transformFlags;
            }
        }

        public void RestoreConstraints()
        {
            if (rotationConstraint != null)
            {
                if (OriginalRotationConstraint.HasValue)
                {
                    rotationConstraint.ConstraintOnRotation = OriginalRotationConstraint.Value;
                }
            }
        }

        /// <summary>
        /// Enables MRTK manipulation with the given TransformFlags.
        ///
        /// None   = 0
        /// Move   = 1
        /// Rotate = 2
        /// Scale  = 4
        /// </summary>
        /// <param name="transformFlags">The manipulations to allow.</param>
        public void EnableManipulation(TransformFlags transformFlags)
        {
            // Debug.LogError($"EnableManipulation for {name}");

            cachedManipulationEnabled = true;

            ManipulationHandFlags manipulationType = default;

            if (transformFlags != 0 &&
                transformFlags.HasFlag(TransformFlags.Move))
            {
                // If the Move flag is provided, allow one-handed manipulation as this is used to
                // move the Asset.
                manipulationType |= ManipulationHandFlags.OneHanded;
            }

            if (transformFlags != 0 &&
                transformFlags.HasFlag(TransformFlags.Rotate) &&
                transformFlags.HasFlag(TransformFlags.Scale))
            {
                // If both the Rotate and Scale flag are provided, allow two-handed manipulation.
                // The default two-handed manipulation can both rotate and scale which is why both
                // need to be disabled. This could be improved by using MRTK's Constraint Manager 
                // for more granular control.
                manipulationType |= ManipulationHandFlags.TwoHanded;
            }

            if (objectManipulatorComponent != null)
            {
                objectManipulatorComponent.ManipulationType = manipulationType;
                objectManipulatorComponent.AllowFarManipulation = true;
                objectManipulatorComponent.TwoHandedManipulationType = transformFlags;

                objectManipulatorComponent.enabled = true;
            }

            // TODO 
            if (editModeCollider != null)
            {
                editModeCollider.enabled = true;
            }
        }

        /// <summary>
        /// Disables MRTK manipulation.
        /// </summary>
        public void DisableManipulation()
        {
            cachedManipulationEnabled = false;

            if (objectManipulatorComponent != null)
            {
                objectManipulatorComponent.ManipulationType = 0;
                objectManipulatorComponent.AllowFarManipulation = false;
                objectManipulatorComponent.TwoHandedManipulationType = 0;
                objectManipulatorComponent.enabled = false;
            }

            if (editModeCollider != null)
            {
                editModeCollider.enabled = false;
            }
        }

        /// <summary>
        /// Shows the MRTK BoundsControl component with the provided handle parameters.
        /// </summary>
        /// <param name="showRotationHandles">Whether rotation handles should be displayed.</param>
        /// <param name="showScaleHandles">Whether scale handles should be displayed.</param>
        public void SetBoundsControl(bool showRotationHandles, bool showScaleHandles)
        {
            cachedManipulationEnabled = true;

            boundsControlComponent.RotationHandlesConfig.ShowHandleForX = showRotationHandles;
            boundsControlComponent.RotationHandlesConfig.ShowHandleForY = showRotationHandles;
            boundsControlComponent.RotationHandlesConfig.ShowHandleForZ = showRotationHandles;
            boundsControlComponent.ScaleHandlesConfig.ShowScaleHandles = showScaleHandles;

            // BoundsControl will not reflect changes in the handles unless the component is
            // reinitialized, so turn it off first.
            boundsControlComponent.enabled = false;
            boundsControlComponent.enabled = true;

            if (objectManipulatorComponent != null)
            {
                objectManipulatorComponent.enabled = true;
            }

            if (editModeCollider != null)
            {
                editModeCollider.enabled = true;
            }
        }

        /// <summary>
        /// Sets the state of the BoundsControl via the BoundControl's Active state.
        /// </summary>
        /// <param name="state"></param>
        public void ActivateBoundControl(bool state)
        {
            boundsControlComponent.Active = state;
        }

        /// <summary>
        /// Hides the MRTK BoundsControl component.
        /// </summary>
        public void HideBoundsControl()
        {
            ActivateBoundControl(false);

            if (editModeCollider != null)
            {
                editModeCollider.enabled = false;
            }
        }

        public void SetBoundsConstraint(bool value)
        {
            if (boundsControlComponent != null)
            {
                boundsControlComponent.EnableConstraints = value;
            }
        }

        public void SetBoxDisplayConfigurations(BoxDisplayConfiguration linkConfiguration)
        {
            boundsControlComponent.BoxDisplayConfig = linkConfiguration;
        }

        public void SetScaleConfigurations(ScaleHandlesConfiguration scaleConfiguration)
        {
            boundsControlComponent.ScaleHandlesConfig = scaleConfiguration;
        }

        public void SetRotationConfigurations(RotationHandlesConfiguration rotationConfiguration)
        {
            boundsControlComponent.RotationHandlesConfig = rotationConfiguration;
        }

        public void SetTranslationConfigurations(TranslationHandlesConfiguration translationConfiguration)
        {
            boundsControlComponent.TranslationHandlesConfig = translationConfiguration;
        }

        public void SetLinksConfigurations(LinksConfiguration linkConfiguration)
        {
            boundsControlComponent.LinksConfig = linkConfiguration;
        }

        public void SetProximityConfigurations(ProximityEffectConfiguration proximityConfiguration)
        {
            boundsControlComponent.HandleProximityEffectConfig = proximityConfiguration;
        }

        #endregion
    }
}