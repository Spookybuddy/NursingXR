using UnityEngine;

namespace GIGXR.Platform.Mobile.AppEvents.Events.AR
{
    public class ArSetOriginEvent : BaseArEvent
    {
        public Transform ObjectTransform { get; }

        public Vector3 TargetPosition { get; }

        public Quaternion TargetOrientation { get; }

        public ArSetOriginEvent(Transform root, Vector3 targetPosition, Quaternion targetOrientation)
        {
            ObjectTransform = root;
            TargetPosition = targetPosition;
            TargetOrientation = targetOrientation;
        }
    }
}