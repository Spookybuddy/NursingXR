namespace GIGXR.Platform.AppEvents.Events.Calibration
{
    using GIGXR.Platform.Scenarios.GigAssets;
    using UnityEngine;

    /// <summary>
    /// Event sent out when calibration has been completed from the CalibrationManager side.
    /// </summary>
    public class SetContentMarkerEvent : BaseCalibrationEvent
    {
        public Vector3 contentMarkerPosition;

        public Quaternion contentMarkerRotation;

        public IAssetMediator assetContentMarker;

        public SetContentMarkerEvent(Vector3 position, Quaternion rotation, IAssetMediator contentMarker = null)
        {
            contentMarkerPosition = position;
            contentMarkerRotation = rotation;
            assetContentMarker = contentMarker;
        }
    }
}
