namespace GIGXR.Platform.Core.Settings
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Holds data that is related to setting up common data across the calibration process.
    /// </summary>
    [Serializable]
    public class CalibrationProfile
    {
        [Tooltip("The GameObject to spawn as the Anchor Root or the Content Marker when no Asset is defined.")]
        public GameObject DefaultCalibrationHandle;

        [Tooltip("The GameObject to spawn above the content marker to move the entire scenario.")]
        public GameObject MoveScenarioButtonPrefab;
    }
}