using UnityEngine;

namespace GIGXR.Platform.Utilities
{
    /// <summary>
    /// Provides a level rotation towards a given target.
    /// </summary>
    public class RotateTowardsTarget : MonoBehaviour
    {
        [Tooltip("What this GameObject should rotate towards. If blank, will default to the Main Camera.")]
        public Transform Target;

        [Tooltip("If true, will run the rotation method in the Update loop.")]
        public bool update = false;

        [Tooltip("Rotation Offset")]
        public Vector3 offset;

        [Tooltip("If true, will reverse the 'LookAt' call. Useful for non-UI objects to use this.")]
        public bool reverseForward = false;

        private Transform _Target
        {
            get
            {
                if (Target != null)
                    return Target;

                Target = Camera.main.transform;

                return Target;
            }
        }

        private void Update()
        {
            if(update)
            {
                RotateTowards();
            }
        }

        public void RotateTowards()
        {
            Vector3 point = new Vector3(_Target.position.x, transform.position.y, _Target.position.z);

            if(reverseForward)
            {
                transform.LookAt(2 * transform.position - point);
            }
            else
            {
                transform.LookAt(point);
            }

            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + offset);
        }
    }
}