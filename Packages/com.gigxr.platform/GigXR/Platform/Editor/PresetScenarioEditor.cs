using Cysharp.Threading.Tasks;
using GIGXR.Platform.ScenarioBuilder.Data;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Scenarios.GigAssets.Data;
using GIGXR.Platform.Scenarios.Stages.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Text.RegularExpressions;

namespace GIGXR.Platform
{
    // See https://forum.unity.com/threads/property-drawers.595369/page-2#post-7644859
    [CustomEditor(typeof(PresetScenario), true)]
    public class PresetScenarioEditor : Editor
    {
        // This is updated by the user so we need a reference to update it
        private TextElement stageIdText;

        private VisualElement buttonContainer;

        private SerializedProperty stageList;

        private static int currentStageIndex = 0;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            // Only show the button to visualize the assets if they are in Edit mode
            if (!Application.isPlaying)
            {
                // Generate a button to visualize the assets in the scene
                var visualizeAssetsButton = new Button(VisualizeAssetsButton_clicked);
                visualizeAssetsButton.text = "Visualize All Assets";

                root.Add(visualizeAssetsButton);

                buttonContainer = new VisualElement();
                buttonContainer.style.flexDirection = FlexDirection.Row;

                var previousStageButton = new Button(PreviousStage);
                previousStageButton.text = "<";
                buttonContainer.Add(previousStageButton);

                stageIdText = new TextElement();
                stageIdText.style.flexGrow = 1;
                stageIdText.style.unityTextAlign = TextAnchor.MiddleCenter;

                buttonContainer.Add(stageIdText);

                var nextStageButton = new Button(NextStage);
                nextStageButton.text = ">";
                buttonContainer.Add(nextStageButton);

                buttonContainer.SetEnabled(showingAssets);
                root.Add(buttonContainer);
            }

            var prop = serializedObject.GetIterator();

            var assetList = serializedObject.FindProperty(nameof(PresetScenario.presetAssets));
            stageList = serializedObject.FindProperty(nameof(PresetScenario.presetStages));

            if (prop.NextVisible(true))
            {
                do
                {
                    VisualElement field;

                    if (prop.name == "m_Script")
                    {
                        field = new PropertyField(prop);

                        field.SetEnabled(false);
                    }
                    else if (prop.name == nameof(PresetScenario.presetAssets))
                    {
                        field = new ListView();

                        var assetListViewField = field as ListView;
                        assetListViewField.showBoundCollectionSize = true;
                        assetListViewField.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
                        assetListViewField.headerTitle = "Preset Asset List";
                        assetListViewField.showFoldoutHeader = true;
                        assetListViewField.reorderable = true;
                        assetListViewField.selectionType = SelectionType.Single;
                        assetListViewField.showAddRemoveFooter = true;
                        assetListViewField.showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly;
                        assetListViewField.reorderMode = ListViewReorderMode.Simple;
                        assetListViewField.showBorder = true;
                        assetListViewField.horizontalScrollingEnabled = false;

                        assetListViewField.itemsAdded += (IEnumerable<int> addedIndices) =>
                        {
                            var index = addedIndices.First();

                            var assetProperty = assetList.GetArrayElementAtIndex(index);
                            var presetAssetId = assetProperty.FindPropertyRelative(nameof(PresetAsset.presetAssetId));

                            // Assets cannot have the same PresetAssetID and Unity's system does not work here for auto-incrementing the name
                            // so do a simple search for a digit at the end, if there is, add 1 to that digit and replace or add a digit so they
                            // have unique names
                            var match = Regex.Match(presetAssetId.stringValue, @"\d+$");

                            if (match.Success)
                            {
                                if (int.TryParse(match.Value, out int result))
                                {
                                    presetAssetId.stringValue = presetAssetId.stringValue.Replace(match.Value, $"{(result + 1)}");

                                    serializedObject.ApplyModifiedProperties();
                                }
                            }
                            else
                            {
                                // When an asset is added to an empty list, make sure it displays some besides just  '-1'
                                if(string.IsNullOrEmpty(presetAssetId.stringValue))
                                {
                                    // TODO Externalize default name?
                                    presetAssetId.stringValue = "asset";
                                }
                                else
                                {
                                    presetAssetId.stringValue = presetAssetId.stringValue + "-1";
                                }

                                serializedObject.ApplyModifiedProperties();
                            }
                        };

                        /* assetListViewField.itemsRemoved += (IEnumerable<int> removedIndices) =>
                        {
                            if (EditorUtility.DisplayDialog("Delete Prefab Data Container as well?", "Confirm", "Cancel"))
                            {
                                // TODO Automatically remove data container, a bit tricky with removal since you only get the index
                            }
                        };*/

                        assetListViewField.bindingPath = assetList.propertyPath;
                        assetListViewField.BindProperty(assetList);
                    }
                    else if (prop.name == nameof(PresetScenario.presetStages))
                    {
                        field = new ListView();

                        var listViewField = field as ListView;
                        listViewField.showBoundCollectionSize = false;
                        listViewField.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;

                        listViewField.selectionType = SelectionType.Single;

                        listViewField.headerTitle = "Stage List";

                        listViewField.showFoldoutHeader = true;
                        listViewField.showBorder = true;
                        listViewField.showAddRemoveFooter = true;
                        listViewField.showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly;

                        // Set up the initial stage ID
                        UpdateCurrentStageInfo();

                        listViewField.itemsAdded += async (IEnumerable<int> addedIndices) =>
                        {
                            // We do not allow the user to edit the collection directly, so it should be safe to assume they will
                            // only add a single stage at a time
                            var index = addedIndices.First();

                            // A delay is needed as the previous stage will be copied, so the GUID will be the same
                            // for a moment before Validation occurs
                            await UniTask.Delay(500, DelayType.Realtime);

                            var stage = GetStage(index);

                            if (stage != null)
                            {
                                AddStageDataOnAllAssets(assetList, stage.stageId);
                            }

                            serializedObject.ApplyModifiedProperties();
                        };

                        listViewField.itemsRemoved += (IEnumerable<int> removedIndices) =>
                        {
                            var index = removedIndices.First();

                            if (index == stageList.arraySize)
                            {
                                RemoveStageFromAllAssets(assetList, index);
                            }
                            else
                            {
                                var stage = GetStage(index);

                                if (stage != null)
                                {
                                    Debug.Log($"Removing Stage {stage.stageId}:{stage.stageTitle} from all assets.");

                                    RemoveStageFromAllAssets(assetList, stage.stageId);
                                }
                            }

                            serializedObject.ApplyModifiedProperties();
                        };

                        listViewField.bindingPath = stageList.propertyPath;
                        listViewField.BindProperty(stageList);
                    }
                    else
                    {
                        field = new PropertyField(prop);
                    }

                    root.Add(field);
                }
                while (prop.NextVisible(false));
            }

            return root;
        }

        private Stage GetStage(int index)
        {
            if (index >= 0 && index < stageList.arraySize)
            {
                var stageProperty = stageList.GetArrayElementAtIndex(index);
                var stage = stageProperty.FindPropertyRelative(nameof(PresetStage.stage));

                return (Stage)stage.GetValue();
            }

            return null;
        }

        private static bool showingAssets = false;

        private static Dictionary<string, GameObject> visualizedAssets = new Dictionary<string, GameObject>();

        public static void MoveAsset(string assetId, SerializedProperty stageAssetProperties, Vector3 sharedValue)
        {
            if(showingAssets && visualizedAssets.ContainsKey(assetId))
            {
                // Get the position based on the current selected stage
                var currentStage = stageAssetProperties.GetArrayElementAtIndex(currentStageIndex);

                var asset = visualizedAssets[assetId];

                if(currentStage.FindPropertyRelative("useShared").boolValue)
                {
                    asset.transform.localPosition = sharedValue;
                }
                else
                {
                    asset.transform.localPosition = currentStage.FindPropertyRelative("localValue").vector3Value;
                }
            }
        }

        public static void RotateAsset(string assetId, SerializedProperty stageAssetProperties, Quaternion sharedRotation)
        {
            if (showingAssets && visualizedAssets.ContainsKey(assetId))
            {
                // Get the position based on the current selected stage
                var currentStage = stageAssetProperties.GetArrayElementAtIndex(currentStageIndex);

                var asset = visualizedAssets[assetId];

                if (currentStage.FindPropertyRelative("useShared").boolValue)
                {
                    asset.transform.localRotation = sharedRotation;
                }
                else
                {
                    asset.transform.localRotation = currentStage.FindPropertyRelative("localValue").quaternionValue;
                }
            }
        }

        public static void ScaleAsset(string assetId, SerializedProperty stageAssetProperties, Vector3 sharedScale)
        {
            if (showingAssets && visualizedAssets.ContainsKey(assetId))
            {
                // Get the position based on the current selected stage
                var currentStage = stageAssetProperties.GetArrayElementAtIndex(currentStageIndex);

                var asset = visualizedAssets[assetId];

                if (currentStage.FindPropertyRelative("useShared").boolValue)
                {
                    asset.transform.localScale = sharedScale;
                }
                else
                {
                    asset.transform.localScale = currentStage.FindPropertyRelative("localValue").vector3Value;
                }
            }
        }

        private void OnSelectionChanged()
        {
            CleanUpAllVisualizedAssets();
        }

        private void CleanUpAllVisualizedAssets()
        {
            Selection.selectionChanged -= OnSelectionChanged;

            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;

            while (visualizedAssets.Count > 0)
            {
                var element = visualizedAssets.FirstOrDefault();
                visualizedAssets.Remove(element.Key);
                DestroyImmediate(element.Value);
            }

            buttonContainer.SetEnabled(false);

            showingAssets = false;
        }

        private void PreviousStage()
        {
            buttonContainer.SetEnabled(false);

            currentStageIndex--;

            if (currentStageIndex < 0)
            {
                currentStageIndex = stageList.arraySize - 1;
            }

            UpdateCurrentStageInfo();

            buttonContainer.SetEnabled(true);
        }

        private void NextStage()
        {
            buttonContainer.SetEnabled(false);

            currentStageIndex++;

            if (currentStageIndex >= stageList.arraySize)
            {
                currentStageIndex = 0;
            }

            UpdateCurrentStageInfo();

            buttonContainer.SetEnabled(true);
        }

        private void UpdateCurrentStageInfo()
        {
            var stage = GetStage(currentStageIndex);

            if (stage != null)
            {
                SetStageDisplay(stage.stageTitle);

                SetStageForVisualizedAsset(stage.stageId);
            }
        }

        private void SetStageDisplay(string stageId)
        {
            stageIdText.text = stageId;
        }

        private void VisualizeAssetsButton_clicked()
        {
            if (!showingAssets)
            {
                Selection.selectionChanged += OnSelectionChanged;

                AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;

                var scenario = serializedObject.targetObject as PresetScenario;

                var stage = GetStage(currentStageIndex);

                if (stage == null)
                    return;

                var currentStage = Guid.Parse(stage.stageId);

                foreach (var asset in scenario.presetAssets)
                {
                    GameObject assetGO = null;

                    try
                    {
                        var path = AssetDatabase.GUIDToAssetPath(asset.assetTypePrefabReference.AssetGUID);

                        var assetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                        var assetMediator = assetPrefab.GetComponent<IAssetMediator>();

                        var assetData = scenario.assetDataPrefabReferences.GetAsset(asset.presetAssetId);

                        assetMediator.SetRuntimeID(asset.AssetId, asset.presetAssetId);

                        assetGO = Instantiate(assetPrefab);
                        assetGO.hideFlags = HideFlags.HideAndDontSave;

                        SetTransformData(assetData, assetGO, currentStage);

                        visualizedAssets.Add(asset.presetAssetId, assetGO);
                    }
                    catch (Exception ex)
                    {
                        Debug.Log($"Error on AssetId {asset.AssetId} - Type {asset.AssetTypeId}");
                        Debug.LogError(ex);

                        if (assetGO != null)
                        {
                            DestroyImmediate(assetGO);
                        }
                    }
                }

                showingAssets = true;
            }
            else
            {
                CleanUpAllVisualizedAssets();
            }

            buttonContainer.SetEnabled(showingAssets);
        }

        private void SetTransformData(GameObject assetData, GameObject assetGO, Guid stageId)
        {
            var positionData = assetData.GetComponent<PositionAssetTypeComponent>();
            var rotationData = assetData.GetComponent<RotationAssetTypeComponent>();
            var scaleData = assetData.GetComponent<ScaleAssetTypeComponent>();

            if(positionData != null)
            {
                assetGO.transform.localPosition = (Vector3)positionData.AssetData.GetPropertyValueAtStageSlow(stageId, nameof(PositionAssetData.position), false);
            }

            if(rotationData != null)
            {
                assetGO.transform.localRotation = (Quaternion)rotationData.AssetData.GetPropertyValueAtStageSlow(stageId, nameof(RotationAssetData.rotation), false);

            }

            if(scaleData != null)
            {
                assetGO.transform.localScale = (Vector3)scaleData.AssetData.GetPropertyValueAtStageSlow(stageId, nameof(ScaleAssetData.scale), false);
            }
        }

        private void SetStageForVisualizedAsset(string stageId)
        {
            if (!showingAssets)
                return;

            var scenario = serializedObject.targetObject as PresetScenario;

            foreach (var presetAsset in scenario.presetAssets)
            {
                var assetData = scenario.assetDataPrefabReferences.GetAsset(presetAsset.presetAssetId);

                SetTransformData(assetData, visualizedAssets[presetAsset.presetAssetId], Guid.Parse(stageId));
            }
        }

        private void OnBeforeAssemblyReload()
        {
            if (showingAssets)
            {
                CleanUpAllVisualizedAssets();
            }
        }

        private void AddStageDataOnAllAssets(SerializedProperty assetList, string stageId)
        {
            var scenario = assetList.serializedObject.targetObject as PresetScenario;

            for (int n = 0; n < assetList.arraySize; n++)
            {
                var asset = assetList.GetArrayElementAtIndex(n);
                var property = asset.FindPropertyRelative(nameof(PresetAsset.presetAssetId));

                scenario.AddStageData(property.stringValue, stageId);
            }
        }

        private void RemoveStageFromAllAssets(SerializedProperty assetList, string stageId)
        {
            var scenario = assetList.serializedObject.targetObject as PresetScenario;

            for (int n = 0; n < assetList.arraySize; n++)
            {
                var asset = assetList.GetArrayElementAtIndex(n);
                var property = asset.FindPropertyRelative(nameof(PresetAsset.presetAssetId));

                scenario.RemoveStageData(property.stringValue, stageId);
            }
        }

        private void RemoveStageFromAllAssets(SerializedProperty assetList, int stageIndex)
        {
            var scenario = assetList.serializedObject.targetObject as PresetScenario;

            for (int n = 0; n < assetList.arraySize; n++)
            {
                var asset = assetList.GetArrayElementAtIndex(n);
                var property = asset.FindPropertyRelative(nameof(PresetAsset.presetAssetId));

                scenario.RemoveStageData(property.stringValue, stageIndex);
            }
        }
    }
}