using System.Collections;
using GIGXR.Platform.Utilities;
using UnityEngine;

namespace GIGXR.Platform.Utilities
{
    //Simple script for applying a rotation towards the main camera
    [RequireComponent(typeof(RotateTowardsTarget))]
    public class LookAtCamera : MonoBehaviour
    {
        /// <summary>
        /// The cached enumerator
        /// </summary>
        private Coroutine coroutine;

        /// <summary>
        /// The wait for seconds between each rotation update
        /// </summary>
        private readonly WaitForSeconds wfs = new WaitForSeconds(0.1f);

        private RotateTowardsTarget towardsCameraHelper;

        private void Awake()
        {
            towardsCameraHelper = GetComponent<RotateTowardsTarget>();
        }

        /// <summary>
        /// On enable start the update rotation logic
        /// </summary>
        private void OnEnable()
        {
            coroutine = StartCoroutine(UpdateRotation());
        }

        /// <summary>
        /// On disable stop the update rotation logic
        /// </summary>
        private void OnDisable()
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }

        /// <summary>
        /// Update the rotation of this object
        /// </summary>
        /// <returns></returns>
        private IEnumerator UpdateRotation()
        {
            while (Application.isPlaying)
            {
                towardsCameraHelper.RotateTowards();

                yield return wfs;
            }
        }
    }
}