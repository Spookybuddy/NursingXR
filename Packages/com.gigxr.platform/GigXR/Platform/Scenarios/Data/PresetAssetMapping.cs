namespace GIGXR.Platform.Scenarios.Data
{
    using System;

    /// <summary>
    /// A data structure to map a presetAssetId (human-readable) to an assetId (GUID).
    /// </summary>
    [Serializable]
    public class PresetAssetMapping
    {
        public string presetAssetId;
        public string assetId;
    }
}