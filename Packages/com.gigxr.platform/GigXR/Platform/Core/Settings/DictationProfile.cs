namespace GIGXR.Platform.Core.Settings
{
    using System;

    /// <summary>
    /// Enum that mirrors CognitiveServices's Profanity Options, but
    /// does not require to import their namespace
    /// </summary>
    public enum ProfanityOption
    {
        Masked,
        Removed,
        Raw
    }

    /// <summary>
    /// Holds data that is related to setting up dictation via Azure Cognitive Service.
    /// </summary>
    [Serializable]
    public class DictationProfile
    {
        /// <summary>
        /// The access key associated with the Azure Cognitive Service.
        /// </summary>
        public string Key;

        /// <summary>
        /// The Cognitive Service availablity region targeted.
        /// </summary>
        public string Region;

        /// <summary>
        /// Determines whether profanity should be filtered, masked or shown as intended.
        /// </summary>
        public ProfanityOption ProfanityOptionSetting;
    }
}