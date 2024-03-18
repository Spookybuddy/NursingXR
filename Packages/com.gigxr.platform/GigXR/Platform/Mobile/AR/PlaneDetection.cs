namespace GIGXR.Platform.Mobile.AR
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;

    public class PlaneDetection : MonoBehaviour
    {
        public static PlaneDetection Instance;

        public bool PlaneDetected { get; private set; }

        private WaitForFixedUpdate waitForFixedFrame = new WaitForFixedUpdate();

        /// <summary>
        ///     Returns the current area of the generated planes in m2
        /// </summary>
        public float PlaneArea 
        { 
            get
            {
                if (ArPlaneManager == null)
                    return 0.0f;

                if (ArPlaneManager.trackables.count == 0) 
                    return 0.0f;

                float planeArea = 0.0f;

                foreach (var plane in ArPlaneManager.trackables)
                {
                    if (plane.gameObject.activeInHierarchy)
                    {
                        planeArea += plane.CalculatePlaneArea();
                    }
                }

                return planeArea;
            }
        }

        /// <summary>
        ///     Cached coroutine for detecting planes
        /// </summary>
        private Coroutine DetectPlaneRoutine;

        private ARPlaneManager ArPlaneManager;

        private Vector2 centerScreenPoint;

        private List<ARRaycastHit> m_Hits = new List<ARRaycastHit>();

        public ARRaycastHit? LastRaycastHit = null;

        private void Awake()
        {
            CloudLogger.LogMethodTrace("Start method", MethodBase.GetCurrentMethod());

            Instance = this;

            centerScreenPoint = new Vector2(Screen.width / 2.0f, Screen.height / 2.0f);

            CloudLogger.LogMethodTrace("End method", MethodBase.GetCurrentMethod());
        }

        public void SetPlaneManager(ARPlaneManager arPlaneManager)
        {
            ArPlaneManager = arPlaneManager;
        }

        /// <summary>
        /// Start plane detection
        /// </summary>
        public void StartPlaneDetection(ARRaycastManager raycastManager)
        {
            StopPlaneDetection();
            DetectPlaneRoutine = StartCoroutine(DetectPlanes(raycastManager));
        }

        /// <summary>
        ///     Stop plane detection
        /// </summary>
        public void StopPlaneDetection()
        {
            if (DetectPlaneRoutine != null)
            {
                StopCoroutine(DetectPlaneRoutine);
                DetectPlaneRoutine = null;
            }

            PlaneDetected = false;
            LastRaycastHit = null;
        }

        /// <summary>
        ///     Area of the targeted plane
        /// </summary>
        private IEnumerator DetectPlanes(ARRaycastManager raycastManager)
        {
            while (true)
            {
                //A plane has now been detected
                PlaneDetected = raycastManager.Raycast(centerScreenPoint, 
                    m_Hits, 
                    UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon);

                //When a plane is detected
                if (PlaneDetected)
                {
                    // Raycast hits are sorted by distance, so the first one will be the closest hit.
                    LastRaycastHit = m_Hits[0];
                }

                yield return waitForFixedFrame;
            }
        }
    }
}