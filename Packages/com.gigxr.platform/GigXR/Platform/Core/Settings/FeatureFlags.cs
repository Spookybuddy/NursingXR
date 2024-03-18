namespace GIGXR.Platform.Core.Settings
{
    using System;

    [Flags] // Add new features as a power of 2
    public enum FeatureFlags
    {
        None = 0,
        Dictation = 1, 
        Avatars = 2,
        DisplayDebugObjects = 4,
        // NewFeature = 8,
        // NewFeature = 16,
        // NewFeature = 32,
    }
}