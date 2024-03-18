namespace GIGXR.Platform.ScenarioBuilder.Data
{
    using Scenarios.Stages.Data;
    using System;
    using UnityEngine;

    /// <summary>
    /// A <c>PresetStage</c> is a wrapper for a <c>Stage</c> that provides a human-usable identifier.
    /// </summary>
    [Serializable]
    public class PresetStage
    {
        /// <summary>
        /// May become obsolete soon due to DD-related structural changes. 
        /// </summary>
        [Header("Preset Stage ID, e.g., \"stage-one\". Should not have spaces.")]
        public string presetStageId;

        public Stage stage;
    }
}