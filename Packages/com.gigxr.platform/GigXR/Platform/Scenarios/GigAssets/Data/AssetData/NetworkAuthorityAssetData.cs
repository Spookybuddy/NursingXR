using System;
using System.Collections.Generic;

namespace GIGXR.Platform.Scenarios.GigAssets.Data
{
    [Serializable]
    public class NetworkAuthorityAssetData : BaseAssetData
    {
        /// <summary>
        /// Who can influence this asset.
        /// </summary>
        public AssetPropertyDefinition<Authority> authority;

        /// <summary>
        /// Set of IDs that can influence the asset if authority is set to AuthoritySet
        /// </summary>
        public AssetPropertyDefinition<List<string>> authoritySet;

        public enum Authority { 
            Everyone,
            HostOnly,
            AuthoritySet
        }
    }
}