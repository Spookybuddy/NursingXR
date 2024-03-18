using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GIGXR.Platform.Scenarios.GigAssets
{
    /// <summary>
    /// <see cref="AssetContext"/> extension to provide accessors
    /// to <c>AssetContext</c> data through formal accessors.
    /// </summary>
    /// <remarks>
    /// Integration of new <c>AssetContext</c> data into this class
    /// is wholly optional; this is a matter of convenience.
    /// </remarks>
    public class GigAssetContext : AssetContext, IGigAssetContext
    {
        public bool IsScenarioAuthority
        {
            get
            {
                // default to true; if this property has not been set
                // then assume that there is no networked activity and
                // the scenario is running independently for a single user
                return GetContext<bool>(nameof(IsScenarioAuthority), true);
            }
        }
    }
}
