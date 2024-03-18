using System;

namespace GIGXR.Platform.Scenarios.GigAssets.Data
{
    [Serializable]
    public enum AssetPropertyChangeOrigin
    {
        Initialization,
        StageChange,
        RuleSet,
        ValueSet // catch-all for "this was set by an ATC, or something outside of Platform"
    }
}
