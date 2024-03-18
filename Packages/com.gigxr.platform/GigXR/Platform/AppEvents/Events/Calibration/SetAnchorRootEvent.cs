namespace GIGXR.Platform.AppEvents.Events.Calibration
{
    using UnityEngine;

    /// <summary>
    /// Event sent out when calibration (setting the anchor root) has been completed from the CalibrationManager side.
    /// </summary>
    public class SetAnchorRootEvent : BaseCalibrationEvent
    {
        public Vector3 TargetWorldPosition;
        public Quaternion TargetWorldRotation;

        public SetAnchorRootEvent(Vector3 targetWorldPosition, Quaternion targetWorldRotation)
        {
            TargetWorldPosition = targetWorldPosition;
            TargetWorldRotation = targetWorldRotation;
        }
    }
}
