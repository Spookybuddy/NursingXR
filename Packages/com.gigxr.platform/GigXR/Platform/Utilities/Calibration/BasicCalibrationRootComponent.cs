namespace GIGXR.Platform.Utilities
{
    using Microsoft.MixedReality.Toolkit;
    using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
    using UnityEngine;

    /// <summary>
    /// A basic implementation of ICalibrationRootProvider that provides the transform of the attached GameObject as the Anchor Point
    /// and a Content Marker for the scenario.
    /// </summary>
    public class BasicCalibrationRootComponent : MonoBehaviour, ICalibrationRootProvider
    {
        public Transform ContentMarkerRoot => transform;

        public Transform AnchorRoot => anchorRootMarker.transform;

        private GameObject anchorRootMarker;

        private void Awake()
        {
            anchorRootMarker = new GameObject("Anchor Root");

            // Give the Anchor root the same parent as the content marker
            anchorRootMarker.transform.SetParent(transform.parent, false);

            // Move the content marker below the anchor root
            transform.SetParent(anchorRootMarker.transform, false);
        }

        public Vector3 WorldToAnchoredPosition(Vector3 worldPosition)
        {
            return AnchorRoot.InverseTransformPoint(worldPosition);
        }

        public Quaternion WorldToAnchoredRotation(Quaternion worldRotation)
        {
            return Quaternion.Inverse(AnchorRoot.rotation) * worldRotation;
        }

        public Vector3 WorldToContentPosition(Vector3 worldPosition)
        {
            return ContentMarkerRoot.InverseTransformPoint(worldPosition);
        }

        public Quaternion WorldToContentRotation(Quaternion worldRotation)
        {
            return Quaternion.Inverse(ContentMarkerRoot.rotation) * worldRotation;
        }

        public Vector3 ContentToWorldPosition(Vector3 contentPosition)
        {
            return ContentMarkerRoot.TransformPoint(contentPosition);
        }

        public void ContentMarkerFollows(Transform contentMarkerTransform, Vector3 positionOffset, Quaternion rotationOffset)
        {
            var follow = ContentMarkerRoot.gameObject.AddComponent<Follow>();
            var solver = ContentMarkerRoot.gameObject.EnsureComponent<SolverHandler>();

            solver.TrackedTargetType = Microsoft.MixedReality.Toolkit.Utilities.TrackedObjectType.CustomOverride;
            solver.TransformOverride = contentMarkerTransform;
            solver.AdditionalOffset = positionOffset;
            solver.AdditionalRotation = rotationOffset.eulerAngles;

            follow.MoveLerpTime = 0f;
            follow.RotateLerpTime = 0f;
            follow.ScaleLerpTime = 0f;

            follow.FaceUserDefinedTargetTransform = false;
            follow.OrientationType = Microsoft.MixedReality.Toolkit.Utilities.SolverOrientationType.FollowTrackedObject;
            follow.FaceTrackedObjectWhileClamped = false;
            follow.ReorientWhenOutsideParameters = false;

            follow.IgnoreDistanceClamp = false;
            follow.MinDistance = 0f;
            follow.MaxDistance = 0f;
            follow.DefaultDistance = 0f;
            follow.VerticalMaxDistance = 0f;

            follow.IgnoreAngleClamp = true;
        }

        public void ContentMarkerRemoveFollows()
        {
            if(ContentMarkerRoot != null)
            {
                var follow = ContentMarkerRoot.gameObject.GetComponent<Follow>();
                var solver = ContentMarkerRoot.gameObject.GetComponent<SolverHandler>();

                if (follow != null)
                {
                    Destroy(follow);
                }

                if (solver != null)
                {
                    Destroy(solver);
                }
            }
        }
    }
}