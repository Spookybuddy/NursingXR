namespace GIGXR.Platform.Scenarios.Data
{
    using System;

    /// <summary>
    /// A data structure to map a presetStageId (human-readable) to an stageId (GUID).
    /// </summary>
    [Serializable]
    public class PresetStageMapping
    {
        public string presetStageId;
        public string stageId;
    }
}