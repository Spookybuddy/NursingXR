namespace GIGXR.Platform.Core.Settings
{
    using System;
    using UnityEditor;

    /// <summary>
    /// Used to store information about this particular app, such as name and version.
    /// This data is used to populate relevant fields such as the AppTitle in the UI.
    /// Can also be used to support automation scripts, such as incrementing the version
    /// in new builds. 
    /// </summary>
    [Serializable]
    public class StyleProfile
    {
        public StyleScriptableObject styleScriptableObject;
    }
}