using System;


namespace GIGXR.Platform.Scenarios.GigAssets
{
    /// <summary>
    /// Convenience accessors for asset context data.
    /// Optional; IAssetContext interface can be used instead.
    /// This exists so those of us who hate hard-coded string literals
    /// can put in a little more work to avoid them.
    /// </summary>
    public interface IGigAssetContext : IAssetContext
    {
        public bool IsScenarioAuthority { get; }
    }
}
