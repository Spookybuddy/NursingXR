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
    /// The component responsible for handling scale data and related events.
    /// </summary>
    [RequireComponent(typeof(PhotonLocalTransformView))]
    public class ScaleAssetTypeComponent : BaseAssetTypeComponent<ScaleAssetData>
    {
    #region Events

        public event EventHandler<ScaleEventArgs> ScaleUpdated;

    #endregion

        public bool IsScaleEditableByAuthor => assetData.scale.designTimeData.isEditableByAuthor;

        [RegisterPropertyChange(nameof(ScaleAssetData.scale))]
        private void HandleScaleChange(AssetPropertyChangeEventArgs e)
        {
            var localScale = (Vector3)e.AssetPropertyValue;

            transform.localScale = localScale;

            ScaleUpdated?.Invoke(this, new ScaleEventArgs(localScale));
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
            assetData.name.designTimeData.defaultValue = "Scale";

            assetData.description.designTimeData.defaultValue
                = "The start scale of the interactable.";

            assetData.scale.designTimeData.defaultValue       = Vector3.one;
            assetData.scale.designTimeData.isEditableByAuthor = true;
        }

        #endregion

        #region Update Routine

        private Vector3 cachedScaleData;
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
                if (transform.hasChanged && Vector3.Distance(cachedScaleData, transform.localScale) > .01f)
                {
                    cachedScaleData = transform.localScale;
                    assetData.scale.runtimeData.UpdateValueLocally(transform.localScale);
                }
                yield return null;
            }
        }

        #endregion
    }
}