using UnityEngine;

namespace GIGXR.Platform.Mobile.AppEvents.Events.AR
{
    /// <summary>
    ///     Event raised when the user places the AR Target at the end of scanning.
    ///     The position and rotation of the AR Target are used for calibration.
    /// </summary>
    public class ArTargetPlacedEvent : BaseArEvent
    {
        public Vector3 TargetSessionPosition;
        public Quaternion TargetWorldRotation;

        public ArTargetPlacedEvent(Vector3 targetSessionPosition, Quaternion targetWorldRotation)
        {
            TargetSessionPosition = targetSessionPosition;
            TargetWorldRotation = targetWorldRotation;
        }
    }
}
