namespace GIGXR.Platform.HMD.QR
{
    using System;
    using UnityEngine;
    using Microsoft.MixedReality.Toolkit.Utilities;
    using Microsoft.MixedReality.OpenXR;

    /// <summary>
    /// Adapted from
    /// https://github.com/chgatla-microsoft/QRTracking/blob/master/SampleQRCodes/Assets/Scripts/SpatialGraphCoordinateSystem.cs
    /// </summary>
    public class SpatialGraphCoordinateSystem : MonoBehaviour
    {
        public delegate void EventHandler(Vector3 position, Quaternion orientation);

        public static event EventHandler PositionUpdatedEvent;

        private SpatialGraphNode _node = null;

        private SpatialGraphNode Node
        {
            get
            {
                if (_node == null)
                {
                    if (Id != Guid.Empty)
                    {
                        _node = SpatialGraphNode.FromStaticNodeId(Id);
                    }
                    // else TODO? This would apply to in Editor only
                }

                return _node;
            }
        }

        private Guid id;

        public Guid Id
        {
            get { return id; }
            set { id = value; }
        }

        void Update()
        {
            UpdateLocation();
        }

        private void UpdateLocation()
        {
            if (Node != null)
            {
                if (Node.TryLocate(FrameTime.OnUpdate, out Pose pose))
                {
                    // If there is a parent to the camera that means we are using teleport and we should not apply the teleport
                    // to these objects so apply the inverse
                    if (CameraCache.Main?.transform.parent != null)
                    {
                        pose = pose.GetTransformedBy(CameraCache.Main.transform.parent);
                    }

                    gameObject.transform.SetPositionAndRotation(pose.position, pose.rotation);
                    //Debug.Log("Id= " + id + " QRPose = " +  pose.position.ToString("F7") + " QRRot = "  +  pose.rotation.ToString("F7"));

                    PositionUpdatedEvent?.Invoke(pose.position, pose.rotation);
                }
            }
        }
    }
}