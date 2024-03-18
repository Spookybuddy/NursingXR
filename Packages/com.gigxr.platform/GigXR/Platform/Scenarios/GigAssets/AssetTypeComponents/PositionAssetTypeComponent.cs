namespace GIGXR.Platform.Scenarios.GigAssets
{
    using GIGXR.Platform.Networking.Utilities;
    using GIGXR.Platform.Scenarios.GigAssets.Data;
    using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
    using GIGXR.Platform.Utilities;
    using System;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// The component responsible for handling position data and related events.
    /// </summary>
    [RequireComponent(typeof(PhotonLocalTransformView))]
    public class PositionAssetTypeComponent : BaseAssetTypeComponent<PositionAssetData>
    {
        private PhotonLocalTransformView localTransformView;

        #region Events

        public event EventHandler<PositionEventArgs> PositionUpdated;

        #endregion

        private void Awake()
        {
            localTransformView = GetComponent<PhotonLocalTransformView>();
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

        [RegisterPropertyChange(nameof(PositionAssetData.position))]
        private void HandlePositionChange(AssetPropertyChangeEventArgs e)
        {
            var localPosition = (Vector3)e.AssetPropertyValue;

            transform.localPosition = localPosition;

            PositionUpdated?.Invoke(this, new PositionEventArgs(localPosition));
        }

        // This could be on any of the transform related ATCs, but position is the most common to think about
        // so I put it here
        public void SetTransformUpdates(bool value)
        {
            localTransformView.enabled = value;
        }

        public void IgnoreTransformUpdates(bool value)
        {
            localTransformView.SuppressTransformWrite(value);
        }

        public override void SetEditorValues()
        {
            assetData.name.designTimeData.defaultValue = "Position";

            assetData.description.designTimeData.defaultValue = "The start position of the interactable.";

            assetData.position.designTimeData.defaultValue = Vector3.zero;
            assetData.position.designTimeData.isEditableByAuthor = true;
        }

        #endregion

        #region Update Routine

        private Vector3 cachedPositionData;
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
                if (transform.hasChanged && Vector3.Distance(cachedPositionData, transform.localPosition) > .01f)
                {
                    cachedPositionData = transform.localPosition;
                    assetData.position.runtimeData.UpdateValueLocally(transform.localPosition);
                }
                yield return null;
            }
        }

        // done in LateUpdate so Rotation and Scale ATCs can also use Transform.hasChanged in their update routines
        private void LateUpdate()
        {
            transform.hasChanged = false;
        }

        #endregion
    }
}