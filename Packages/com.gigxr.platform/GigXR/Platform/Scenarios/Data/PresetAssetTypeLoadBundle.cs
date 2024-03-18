using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Newtonsoft.Json;


namespace GIGXR.Platform.ScenarioBuilder.Data
{
    /// <summary>
    /// An <c>PresetAssetTypeLoadBundle</c> lists a group of assets to be
    /// loaded alongside a <c>Scenario</c> but not instantiated.
    /// </summary>
    /// <remarks>
    /// Intended for use with accessory assets managed by a parent,
    /// or inventory assets used to spawn instances of an asset.
    /// </remarks>
    [CreateAssetMenu(fileName = "New Preset Asset Type Load Bundle", menuName = "GIGXR/ScriptableObjects/New Preset Asset Type Load Bundle")]
    public class PresetAssetTypeLoadBundle : ScriptableObject
    {
        /// <summary>
        /// References to the addressable assets that will be loaded.
        /// </summary>
        [Header("Asset Type Prefab References")]
        public List<AssetReference> assetTypePrefabReferences;

        /// <summary>
        /// References to other bundles that should load with this bundle.
        /// </summary>
        [Header("Dependency Bundles")]
        public List<PresetAssetTypeLoadBundle> dependencyBundles;

        /// <summary>
        /// Get a set of all unique asset type ids in the bundle and
        /// recursively through its dependency bundles.
        /// </summary>
        /// <returns>
        /// An <c>ISet</c> of all unique asset type ids represented in the bundle.
        /// </returns>
        public ISet<string> GetUniqueAssetReferences()
        {
            HashSet<string> references = new HashSet<string>();

            foreach(AssetReference reference in assetTypePrefabReferences)
            {
                references.Add(reference.RuntimeKey.ToString());
            }

            foreach(PresetAssetTypeLoadBundle bundle in dependencyBundles)
            {
                references.UnionWith(bundle.GetUniqueAssetReferences());
            }

            return references;
        }

        /// <summary>
        /// Convenience method to allow <c>PresetScenario</c> to easily get
        /// a list of all asset type ids from its list of bundles.
        /// </summary>
        /// <param name="bundles"></param>
        /// <returns>
        /// An <c>ISet</c> of all unique asset type ids represented in the enumerated bundles.
        /// </returns>
        public static ISet<string> GetUniqueAssetReferences(IEnumerable<PresetAssetTypeLoadBundle> bundles)
        {
            HashSet<string> references = new HashSet<string>();

            foreach(PresetAssetTypeLoadBundle bundle in bundles)
            {
                references.UnionWith(bundle.GetUniqueAssetReferences());
            }

            return references;
        }
    }
}
