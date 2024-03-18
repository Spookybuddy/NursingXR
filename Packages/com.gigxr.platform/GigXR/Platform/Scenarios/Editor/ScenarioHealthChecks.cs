using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using GIGXR.Platform.Scenarios.GigAssets;

namespace GIGXR.Platform.Core.DependencyValidator
{
    public class ScenarioHealthChecks
    {
        public static void ValidateNoEmptyStageGuid()
        {
            // Get all addressables
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            var allEntries = new List<AddressableAssetEntry>(settings.groups.SelectMany(g => g.entries));

            foreach (var entry in allEntries)
            {
                if (entry.MainAssetType == null) continue;
                if (entry.MainAssetType != typeof(GameObject)) continue;
            
                var go = entry.MainAsset as GameObject;
                
                if (go != null)
                {
                    foreach (var atc in go.GetComponentsInChildren<IAssetTypeComponent>())
                    {
                        var allFailedATCPropertyNames = atc.GetAssetPropertiesWithRuntimeStageValues();

                        foreach(var atcPropertyName in allFailedATCPropertyNames)
                        {
                            var clear = EditorUtility.DisplayDialog("Invalid Asset", $"{go.name}'s property {atcPropertyName} has invalid stage data. Clear runtime stage data?", "Clear", "Skip");

                            if (clear)
                            {
                                atc.ClearRuntimeStageDataValues();

                                PrefabUtility.SavePrefabAsset(go);
                            }
                        }
                    }
                }
            }

            AssetDatabase.SaveAssets();

            AssetDatabase.Refresh();

            Debug.Log($"Completed checking Stage GUIDs");
        }

        [MenuItem("GIGXR/Health Checks/GUID Addressable Health Check")]
        private static void ValidateNoEmptyStageGuidMenu()
        {
            ValidateNoEmptyStageGuid();
        }
    }
}