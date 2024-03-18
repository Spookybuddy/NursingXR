namespace GIGXR.Platform.Core.Settings
{
    using System;

    /// <summary>
    /// Holds data that is related to what features are enabled for this app.
    /// </summary>
    [Serializable]
    public class FeatureFlagsProfile
    {
        /// <summary>
        /// Determines what features are enabled or not
        /// </summary>
        public FeatureFlags FeatureFlags;
    }
}