namespace GIGXR.Platform.Utilities
{
    using UnityEngine;

    /// <summary>
    /// Responsible for providing the parent transform for all Assets to be instantiated under.
    /// </summary>
    public interface ICalibrationRootProvider
    {
        /// <summary>
        /// This is the actual transform in the hierarchy that represents the content marker where
        /// all assets are under
        /// </summary>
        Transform ContentMarkerRoot { get; }

        /// <summary>
        /// This is the location in space where the user will sync with other users to have a common
        /// origin.
        /// </summary>
        Transform AnchorRoot { get; }

        /// <summary>
        /// Convert a world position to a local position relative to the anchor root
        /// </summary>
        Vector3 WorldToAnchoredPosition(Vector3 worldPosition);

        /// <summary>
        /// Convert a world rotation to a local rotation relative to the anchor root
        /// </summary>
        Quaternion WorldToAnchoredRotation(Quaternion rotation);

        Vector3 WorldToContentPosition(Vector3 worldPosition);

        Quaternion WorldToContentRotation(Quaternion worldRotation);

        Vector3 ContentToWorldPosition(Vector3 contentPosition);

        void ContentMarkerFollows(Transform contentMarkerTransform, Vector3 positionOffset, Quaternion rotationOffset);
        
        void ContentMarkerRemoveFollows();
    }
}