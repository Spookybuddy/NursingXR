namespace GIGXR.Platform.Scenarios.GigAssets
{
    using GIGXR.Platform.Scenarios.GigAssets.Data;
    using GIGXR.Platform.Utilities;
    using System;
    using UnityEngine;

    /// <summary>
    /// The component responsible for firing collision related events.
    /// </summary>
    public class CollisionAssetTypeComponent : BaseAssetTypeComponent<EmptyAssetData>
    {
        #region Events

        public event EventHandler<CollisionEventArgs> CollisionEnter;
        public event EventHandler<CollisionEventArgs> CollisionExit;
        public event EventHandler<ColliderEventArgs> TriggerEnter;
        public event EventHandler<ColliderEventArgs> TriggerExit;

        #endregion

        #region AssetTypeComponentOverrides

        protected override void Setup()
        {

        }

        protected override void Teardown()
        {

        }

        public override void SetEditorValues()
        {
            assetData.name.designTimeData.defaultValue = "Collision";
            assetData.description.designTimeData.defaultValue = "Provides the collision/trigger methods to GIGXR's Asset System.";
        }

        #endregion

        #region CollisionFunctions

        private void OnCollisionEnter(Collision other)
        {
            CollisionEnter.Invoke(this, new CollisionEventArgs(other));
        }

        private void OnCollisionExit(Collision other)
        {
            CollisionExit.Invoke(this, new CollisionEventArgs(other));
        }

        private void OnTriggerEnter(Collider other)
        {
            TriggerEnter.Invoke(this, new ColliderEventArgs(other));
        }

        private void OnTriggerExit(Collider other)
        {
            TriggerExit.Invoke(this, new ColliderEventArgs(other));
        }

        #endregion
    }
}