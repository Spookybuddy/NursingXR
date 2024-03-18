namespace GIGXR.Platform.Core.Settings
{
    using System;

    /// <summary>
    /// Holds data that is related to setting up dictation via Azure Cognitive Service.
    /// </summary>
    [Serializable]
    public class PerformanceProfile
    {
        /// <summary>
        /// How much time an assets is given before the next asset is instantiated.
        /// </summary>
        public int WaitTimeBetweenAssetGroupsMilliseconds = 500;
    }
}