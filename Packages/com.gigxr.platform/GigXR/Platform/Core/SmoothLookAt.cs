using UnityEngine;

namespace GIGXR.Platform
{
    public class SmoothLookAt : MonoBehaviour
    {
        // --- Serialized Variables:

        [SerializeField] private float smoothTime;

        [SerializeField] private Transform target;

        // --- Private Variables:

        private bool looking = true;

        // --- Public Methods:

        public void SetLooking(bool isLooking)
        {
            looking = isLooking;
        }

        // --- Private Methods:

        private void FixedUpdate()
        {
            SmoothLookRotation();
        }

        private void SmoothLookRotation()
        {
            if (!target || !looking) return;
            Vector3 relativePos = target.transform.localPosition - transform.position;
            Quaternion toRotation = Quaternion.LookRotation(relativePos);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, smoothTime * Time.deltaTime);
        }
    }
}