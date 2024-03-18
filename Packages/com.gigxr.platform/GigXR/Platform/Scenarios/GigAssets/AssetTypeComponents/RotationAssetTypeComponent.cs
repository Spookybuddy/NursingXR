namespace GIGXR.Platform.Scenarios.GigAssets
{
    using Core.DependencyInjection;
    using GIGXR.Platform.Networking.Utilities;
    using Data;
    using EventArgs;
    using Utilities;
    using Microsoft.MixedReality.Toolkit.UI;
    using Scenarios.EventArgs;
    using System;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// The component responsible for handling rotation data and related events.
    /// </summary>
    [RequireComponent(typeof(RotationAxisConstraint))]
    [RequireComponent(typeof(PhotonLocalTransformView))]
    public class RotationAssetTypeComponent : BaseAssetTypeComponent<RotationAssetData>
    {
        #region Events

        public event EventHandler<RotationEventArgs> RotationUpdated;

        #endregion

        [RegisterPropertyChange(nameof(RotationAssetData.rotation))]
        private void HandleRotationChange(AssetPropertyChangeEventArgs e)
        {
            var localRotation = (Quaternion)e.AssetPropertyValue;
            transform.localRotation = localRotation;
            RotationUpdated?.Invoke(this, new RotationEventArgs(localRotation));
        }

        #region AssetTypeComponentOverrides

        protected override void Setup()
        {
            StartUpdateRoutine();
        }

        protected override void Teardown()
        {
            StopUpdateRoutine();
        }

        public override void SetEditorValues()
        {
            assetData.name.designTimeData.defaultValue = "Rotation";
            assetData.description.designTimeData.defaultValue = "The start rotation of the interactable in a Quaternion.";
            assetData.rotation.designTimeData.defaultValue = Quaternion.identity;
        }

        #endregion

        #region Update Routine

        private Quaternion cachedRotationData;
        private Coroutine updateRoutine;

        private void StartUpdateRoutine()
        {
            StopUpdateRoutine();
            updateRoutine = StartCoroutine(UpdateRoutine());
        }

        private void StopUpdateRoutine()
        {
            if (updateRoutine != null)
            {
                StopCoroutine(updateRoutine);
                updateRoutine = null;
            }
        }

        private IEnumerator UpdateRoutine()
        {
            while (true)
            {
                // transform.hasChanged is reset in PostionATC's LateUpdate
                if (transform.hasChanged && Quaternion.Angle(cachedRotationData, transform.localRotation) > .01f)
                {
                    cachedRotationData = transform.localRotation;
                    assetData.rotation.runtimeData.UpdateValueLocally(transform.localRotation);
                }
                yield return null;
            }
        }

        #endregion
    }
}