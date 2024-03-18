using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GIGXR.Platform.Core.DependencyValidator
{
    public class RequireDependencyAddressables
    {
        // This enumerates all Addressable-marked prefabs and passes them to the RequireDependencyValidator
        public static void ValidateAddressables()
        {
            var validator = new RequireDependencyValidator();

            // Get all addressables
            var prefabs = new List<GameObject>();

            AddressableAssetSettings settings   = AddressableAssetSettingsDefaultObject.Settings;
            var                      allEntries = new List<AddressableAssetEntry>(settings.groups.SelectMany(g => g.entries));

            foreach (var entry in allEntries)
            {
                if (entry.MainAssetType == null) continue;
                if (entry.MainAssetType != typeof(UnityEngine.GameObject)) continue;

                // Debug.Log($"Addressable Entry found: {entry.address} - {entry.TargetAsset?.name} - {entry.MainAssetType?.ToString()}");

                var go = entry.MainAsset as GameObject;

                if (go != null)
                {
                    prefabs.Add(go);
                }
            }

            // Shove addressable list into validator
            var validationPassed = validator.ValidateRequiredDependencies(prefabs.ToArray());

            if (!validationPassed)
                throw new Exception("RequireDependency Validation failed. Please correct Errors listed above to build");

            // Debug.Log("[RequireDependencyBuildProcessor] All [RequireDependency] fields have references!");
        }

        [MenuItem("GIGXR/Health Checks/Validate Required Dependencies in Addressables")]
        private static void ValidateAddressablesMenu()
        {
            try
            {
                ValidateAddressables();
            }
            catch (Exception e)
            {
                Debug.LogError($"[RequireDependencyBuildProcessor] {e.Message}");
            }
        }

        /// <summary>
        /// Checks required dependencies at runtime when ran as a development build.
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        private static void ValidateAddressablesOnStartup()
        {
            try
            {
                ValidateAddressables();
            }
            catch (Exception e)
            {
                Debug.LogError("RequireDependency Validation failed. Application will quit.");
                EditorApplication.ExitPlaymode();
            }
        }
    }
}