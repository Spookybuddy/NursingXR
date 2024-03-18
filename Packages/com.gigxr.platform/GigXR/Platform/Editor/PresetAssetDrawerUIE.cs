using GIGXR.Platform.ScenarioBuilder.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;
using System.Linq;
using System.Reflection;
using System;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.ExtensionClasses;
using Newtonsoft.Json.Linq;
using GIGXR.Utilities;
using GIGXR.Platform.Scenarios.GigAssets.Data;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using GIGXR.Platform.Utilities;
using GIGXR.Platform.Scenarios.Stages.Data;
using GIGXR.Platform.Scenarios;

namespace GIGXR.Platform
{
    [CustomPropertyDrawer(typeof(PresetAsset))]
    public class PresetAssetDrawerUIE : PropertyDrawer
    {
        // Maps the foldout for each asset in the scenario as UI elements can be reused
        protected static Dictionary<string, Foldout> assetFoldouts = new Dictionary<string, Foldout>();

        // Maps if the preset asset has created the needed visual elements so it will only do so one time
        // on the first time the foldout is first out
        protected static Dictionary<string, bool> visualElementsCreated = new Dictionary<string, bool>();

        // Maps Asset Properties via the serialized object and property name to the Preset Asset IDs
        protected static Dictionary<(SerializedObject, string), string> mappedPropertiesToPresetAsset = new Dictionary<(SerializedObject, string), string>();

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Grab data from the scenario
            var assetDataPrefabReferences = property.serializedObject.FindProperty(nameof(PresetScenario.assetDataPrefabReferences));

            AssetPrefabDataScriptableObject assetPrefabData;

            if (assetDataPrefabReferences.objectReferenceValue == null)
            {
                assetPrefabData = ScriptableObject.CreateInstance<AssetPrefabDataScriptableObject>();
                assetPrefabData.name = $"Runtime Data";

                AssetDatabase.AddObjectToAsset(assetPrefabData, property.serializedObject.targetObject);

                assetDataPrefabReferences.objectReferenceValue = assetPrefabData;

                assetDataPrefabReferences.serializedObject.ApplyModifiedProperties();
            }
            else
            {
                assetPrefabData = assetDataPrefabReferences.objectReferenceValue as AssetPrefabDataScriptableObject;
            }

            // Create property container element.
            var container = new Foldout();

            var presetAssetId = property.FindPropertyRelative(nameof(PresetAsset.presetAssetId));
            var assetData = property.FindPropertyRelative(nameof(PresetAsset.assetData));
            var assetTypePrefabReference = property.FindPropertyRelative(nameof(PresetAsset.assetTypePrefabReference));
            var assetId = property.FindPropertyRelative(nameof(PresetAsset.assetId));

            container.text = presetAssetId.stringValue;
            container.value = false;

            MegaAssetData assetDataToLoad = null;

            // Check to see if there is also data that needs to be imported
            if (!string.IsNullOrEmpty(assetData.stringValue))
            {
                assetDataToLoad = DeserializeJson(assetData.stringValue);
            }

            // Create property fields.
            var presetAssetIdField = new TextField("PresetAssetId");
            presetAssetIdField.value = presetAssetId.stringValue;
            presetAssetIdField.isDelayed = true;

            var assetTypePrefabReferenceField = new PropertyField(assetTypePrefabReference);
            var assetIdField = new PropertyField(assetId);

            string label = "";
            var assetRef = GetActualObjectForSerializedProperty<AssetReference>(assetTypePrefabReference, fieldInfo, ref label);

            if (!assetFoldouts.ContainsKey(presetAssetId.stringValue))
            {
                // Add fields to the container.
                container.Add(presetAssetIdField);
                container.Add(assetTypePrefabReferenceField);
                container.Add(assetIdField);

                // Monitor when the PresetAssetID changes so you can update the related files
                presetAssetIdField.RegisterValueChangedCallback((evt) =>
                {
                    if (evt.target == presetAssetIdField)
                    {
                        // Redundancy check, this value has already been set
                        if (evt.previousValue == evt.newValue)
                            return;

                        var prefabAsset = assetPrefabData.GetAsset(evt.previousValue);

                        if (prefabAsset != null)
                        {
                            var path = AssetDatabase.GetAssetPath(prefabAsset);

                            // Update the prefab data container
                            if (!string.IsNullOrEmpty(path))
                            {
                                var newAssetName = GetAssetFileName(evt.newValue);

                                var result = AssetDatabase.RenameAsset(path, newAssetName);

                                if (!string.IsNullOrEmpty(result))
                                {
                                    GIGXR.Platform.Utilities.Logger.Warning($"Did not rename asset at path {path} to {newAssetName}: {result}");
                                }
                            }

                            // Update the ID in the SO asset data
                            assetPrefabData.ReplaceAsset(evt.previousValue, evt.newValue);
                        }

                        container.text = evt.newValue;
                        presetAssetId.stringValue = evt.newValue;

                        property.serializedObject.ApplyModifiedProperties();
                    }
                });

                // Wait for the foldout value to change to create the elements below
                container.RegisterValueChangedCallback((evt) =>
                {
                    if (evt.target == container)
                    {
                        AssetFoldoutValueChanged(property, assetPrefabData, presetAssetId.stringValue, assetRef);
                    }
                });

                // Handle setting a new asset in the Editor as the foldout has to be refreshed with the new Asset data
                assetTypePrefabReferenceField.RegisterValueChangeCallback((evt) =>
                {
                    if (evt.target == assetTypePrefabReferenceField)
                    {
                        GameObject prefabAssetWithData = assetPrefabData.GetAsset(presetAssetId.stringValue);

                        // See if the user wants to delete or preserve the previous prefab asset
                        if (prefabAssetWithData != null && EditorUtility.DisplayDialog("Delete Prefab",
                                                                                       $"Delete the previous Prefab data container {prefabAssetWithData.name} as well?",
                                                                                       "Delete",
                                                                                       "Preserve"))
                        {
                            assetPrefabData.DestroyAsset(presetAssetId.stringValue);
                        }
                        else
                        {
                            // If they preserve the data, we still need to remove it from the dictionary in the SO
                            assetPrefabData.RemoveAsset(presetAssetId.stringValue);
                        }

                        for (int index = assetFoldouts[presetAssetId.stringValue].contentContainer.childCount - 1; index >= 0; index--)
                        {
                            // Only remove the nested foldouts in the asset foldout list
                            if (assetFoldouts[presetAssetId.stringValue].ElementAt(index) is Foldout)
                            {
                                assetFoldouts[presetAssetId.stringValue].RemoveAt(index);
                            }
                        }

                        // Remove the previous Editor details so they are not around
                        visualElementsCreated[presetAssetId.stringValue] = false;

                        // Move the foldout to false to force the refresh of the screen
                        assetFoldouts[presetAssetId.stringValue].value = false;

                        string newLabel = "";
                        var newAssetRef = GetActualObjectForSerializedProperty<AssetReference>(assetTypePrefabReference, fieldInfo, ref newLabel);

                        SetupPrefabAsset(property, assetPrefabData, newAssetRef.AssetGUID, presetAssetId.stringValue, assetDataToLoad);
                    }
                });

                assetFoldouts.Add(presetAssetId.stringValue, container);
            }
            else
            {
                container = assetFoldouts[presetAssetId.stringValue];
            }

            if (!string.IsNullOrEmpty(assetRef.AssetGUID))
            {
                SetupPrefabAsset(property, assetPrefabData, assetRef.AssetGUID, presetAssetId.stringValue, assetDataToLoad);

                // Remove the JSON blob from the Asset Data for the Preset Asset
                assetData.stringValue = null;

                assetDataPrefabReferences.serializedObject.ApplyModifiedProperties();
            }

            Selection.selectionChanged += OnSelectionChanged;

            AssetDatabase.SaveAssets();

            return container;
        }

        private void SetupPrefabAsset(SerializedProperty property, AssetPrefabDataScriptableObject assetPrefabData, string assetReferenceGuid, string presetAssetId, MegaAssetData assetDataToLoad = null)
        {
            // Check to see if we have created a copy of the prefab to be the serialized object to hold our data
            GameObject prefabAssetWithData = assetPrefabData.GetAsset(presetAssetId);

            // Create a copy of the Addressable asset to hold the data for each component, while also providing a serialized property
            if (prefabAssetWithData == null)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetReferenceGuid);

                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                GameObject temp = GameObject.Instantiate(asset);
                temp.transform.DestroyChildrenImmediate();

                prefabAssetWithData = PrefabUtility.SaveAsPrefabAsset(temp, GetAssetFileName(presetAssetId, property));

                assetPrefabData.AddAsset(presetAssetId, prefabAssetWithData);

                // Remove the GameObject from the Scene
                // TODO Is there a way to create a Prefab asset without touching the Scene?
                GameObject.DestroyImmediate(temp);

                var assetTypeComponents = prefabAssetWithData.GetComponents<IAssetTypeComponent>();

                if (assetDataToLoad != null)
                {
                    foreach (var atc in assetTypeComponents)
                    {
                        //BaseAssetType<> is generic, so we can't cast directly to that, but Component will allow us the same thing
                        var serializedAssetProperty = new SerializedObject((Component)atc);
                        var assetDataProperty = serializedAssetProperty.FindProperty("assetData");

                        foreach (var assetPropertyDefinition in atc.GetAllPropertyDefinition())
                        {
                            // We only want the end users to edit the runtime data in the Editor
                            var propertySerialized = assetDataProperty.FindPropertyRelative(assetPropertyDefinition.PropertyName).FindPropertyRelative("runtimeData");

                            // See AssetPropertyRuntimeData<> for these data points
                            var sharedValueProperty = propertySerialized.FindPropertyRelative("sharedValue");

                            var stageDataProperty = propertySerialized.FindPropertyRelative("stageAssetProperties");

                            var propertyData = assetDataToLoad.GetProperty(atc.GetAssetTypeName(), assetPropertyDefinition.PropertyName);

                            if (propertyData != null)
                            {
                                // Grab the shared value to display
                                if (sharedValueProperty != null)
                                {
                                    var shared = propertyData["runtimeData"]["sharedValue"];
                                    var shared_data = shared.ToObject(assetPropertyDefinition.SpecifiedType);

                                    sharedValueProperty.SetValue(shared_data);
                                }

                                // Grab all the stage data to display
                                if (stageDataProperty != null)
                                {
                                    var serializedStages = propertyData["runtimeData"]["stageAssetProperties"];

                                    Type genericStageAsset = typeof(StageAssetProperty<>).MakeGenericType(assetPropertyDefinition.SpecifiedType);
                                    Type listType = typeof(List<>).MakeGenericType(genericStageAsset);

                                    try
                                    {
                                        var stageListData = property.serializedObject.FindProperty(nameof(PresetScenario.presetStages));

                                        // Check the length of the serialized stage data, if it does not match the length, we assume there is extra or missing data
                                        if (serializedStages.ToArray().Length != stageListData.arraySize)
                                        {
                                            var serializedStageArray = serializedStages.ToList();
                                            var presetScenario = stageListData.serializedObject.targetObject as PresetScenario;
                                            List<PresetStage> knownStages = new List<PresetStage>();
                                            List<RuntimeStageInput> knownStageData = new List<RuntimeStageInput>();

                                            for (int n = serializedStageArray.Count - 1; n >= 0; n--)
                                            {
                                                var stageId = serializedStageArray[n].Value<string>("stageId");

                                                // Save valid stage info so it can be reconstructed with all the stage data
                                                if (Guid.TryParse(stageId, out Guid result) && result != Guid.Empty &&
                                                   presetScenario.TryGetStage(stageId, out PresetStage stage))
                                                {
                                                    knownStages.Add(stage);
                                                    knownStageData.Add(new RuntimeStageInput(stageId,
                                                                                             serializedStageArray[n].Value<bool>("useShared"),
                                                                                             serializedStageArray[n].Value<bool>("resetValueOnStageChange"),
                                                                                             serializedStageArray[n].SelectToken("localValue").ToObject(assetPropertyDefinition.SpecifiedType),
                                                                                             presetScenario.presetStages.IndexOf(stage)));
                                                }
                                                // Remove invalid and empty IDs as we do not want those saved in our lists
                                                else
                                                {
                                                    serializedStageArray.RemoveAt(n);
                                                }
                                            }

                                            foreach (var runtimeData in knownStageData ?? Enumerable.Empty<RuntimeStageInput>())
                                            {
                                                SetupStageData(property, presetAssetId, new StageInput(assetPropertyDefinition, runtimeData.stageIndex, runtimeData));
                                            }

                                            // The serialized data could be missing stage data as well, add any that were not saved now
                                            var missingeStages = presetScenario.presetStages.Except(knownStages);
                                            foreach (var stage in missingeStages ?? Enumerable.Empty<PresetStage>())
                                            {
                                                SetupStageData(property, presetAssetId, new StageInput(assetPropertyDefinition, presetScenario.presetStages.IndexOf(stage)));
                                            }
                                        }
                                        else
                                        {
                                            var stage_data = serializedStages.ToObject(listType);

                                            stageDataProperty.SetValue(stage_data);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.Log($"There was an issue while trying to create the stage list for Property {assetPropertyDefinition.PropertyName} " +
                                            $"on Asset {atc.GetAssetTypeName()}. Stage data was not created, please see the error below. {e.Message}");

                                        Debug.LogError(e);
                                    }
                                }
                            }
                        }
                    }
                }
                // There is no saved data to default to, start from default data with all the stages
                else
                {
                    var scenario = property.serializedObject.targetObject as PresetScenario;

                    scenario.ResetRuntimeSharedValue(presetAssetId);

                    SetupStageData(property, presetAssetId);
                }
            }
        }

        private void SetupStageData(SerializedProperty property, string presetAssetId, StageInput? stageInfo = null)
        {
            var scenario = property.serializedObject.targetObject as PresetScenario;
            var stageListData = property.serializedObject.FindProperty(nameof(PresetScenario.presetStages));

            if (stageInfo != null)
            {
                var actualStage = GetStage(stageListData, stageInfo.specificStageIndex);

                scenario.AddStageData(presetAssetId, actualStage.stageId, stageInfo.assetPropertyDefinition, stageInfo.runtimeData);
            }
            // Setup stage data for all of the known stages
            else
            {
                // We must traverse the stage data in the PresetScenario and build the default data for each stage
                for (int i = 0; i < stageListData.arraySize; i++)
                {
                    var actualStage = GetStage(stageListData, i);

                    scenario.AddStageData(presetAssetId, actualStage.stageId);
                }
            }
        }

        private void EnsureStageData(SerializedProperty property, IAssetTypeComponent atc)
        {
            var stageListData = property.serializedObject.FindProperty(nameof(PresetScenario.presetStages));

            for (int i = 0; i < stageListData.arraySize; ++i)
            {
                var actualStage = GetStage(stageListData, i);

                atc.AddStageData(actualStage.stageId);
            }
        }

        private Stage GetStage(SerializedProperty stageListData, int index)
        {
            var stageProperty = stageListData.GetArrayElementAtIndex(index);
            var stage = stageProperty.FindPropertyRelative(nameof(PresetStage.stage));

            return (Stage)stage.GetValue();
        }

        private string GetAssetFileName(string presetAssetId, SerializedProperty property = null)
        {
            if (property != null)
            {
                var assetPath = AssetDatabase.GetAssetPath(property.serializedObject.targetObject);
                return assetPath + "." + presetAssetId + ".prefab";
            }
            else
            {
                return "asset." + presetAssetId + ".prefab";
            }
        }

        private void OnSelectionChanged()
        {
            Selection.selectionChanged -= OnSelectionChanged;

            assetFoldouts.Clear();
            visualElementsCreated.Clear();
            mappedPropertiesToPresetAsset.Clear();
        }

        private void AssetFoldoutValueChanged(SerializedProperty property, AssetPrefabDataScriptableObject assetPrefabData, string presetAssetId, AssetReference sourcePrefab)
        {
            Foldout assetFoldout = assetFoldouts[presetAssetId];

            //Using the UI Debugger, I figured out where in the VisaualElement hierarchy the arrow was located
            if (assetFoldout.value)
            {
                if (!visualElementsCreated.ContainsKey(presetAssetId) || !visualElementsCreated[presetAssetId])
                {
                    visualElementsCreated[presetAssetId] = true;

                    GameObject prefabAssetWithData = assetPrefabData.GetAsset(presetAssetId);

                    // In order to apply prefab changes, create an instance of the data holder prefab
                    GameObject prefabInstance = null;

                    if (prefabAssetWithData != null)
                    {
                        IAssetTypeComponent[] assetTypeComponentsOnDataHolder = prefabAssetWithData.GetComponents<IAssetTypeComponent>();

                        // Check to see if this ATC exists in the source ATC, if not, remove it from the prefab data holder
                        string path = AssetDatabase.GUIDToAssetPath(sourcePrefab.AssetGUID);

                        GameObject sourceAssetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                        Dictionary<Type, IAssetTypeComponent> addedATCs = new Dictionary<Type, IAssetTypeComponent>();

                        // Go through every ATC on the data holder prefab and create a foldout for each one
                        foreach (IAssetTypeComponent atc in assetTypeComponentsOnDataHolder)
                        {
                            // Check to see if the Source Asset prefab still has this ATC, if it does not, a developer
                            // has changed the source since this scenario was last saved
                            if (sourceAssetPrefab.GetComponent(atc.GetType()) == null)
                            {
                                // Inform the user that this has happened, they will also see the data removed via Git
                                Debug.Log($"The source asset for {presetAssetId} does not have the Asset Type Component {atc.GetType()}. " +
                                    $"This ATC will be removed from the prefab data source.");

                                if (prefabInstance == null)
                                {
                                    prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAssetWithData);
                                }

                                PrefabUtility.ApplyRemovedComponent(prefabInstance, (Component)atc, InteractionMode.AutomatedAction);

                                continue;
                            }

                            addedATCs.Add(atc.GetType(), atc);

                            Foldout atcFoldout = CreateATCFoldout(atc, presetAssetId);

                            assetFoldout.Add(atcFoldout);
                        }

                        // Now that the known ATCs on the data holder have been added, check the source for any missing
                        IAssetTypeComponent[] assetTypeComponentsOnSourcePrefab = sourceAssetPrefab.GetComponents<IAssetTypeComponent>();

                        foreach (var sourceATC in assetTypeComponentsOnSourcePrefab)
                        {
                            if (addedATCs.TryGetValue(sourceATC.GetType(), out var atc))
                            {
                                // ensure that stage data is initialized on new properties
                                // this is a bit of brute force. room for improvement, but it serves immediate needs.
                                EnsureStageData(property, atc);
                                continue;
                            }

                            if (prefabInstance == null)
                            {
                                prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAssetWithData);
                            }

                            string assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefabInstance);
                            Component addedComponent = prefabInstance.AddComponent(sourceATC.GetType());

                            PrefabUtility.ApplyAddedComponent(addedComponent, assetPath, InteractionMode.AutomatedAction);

                            // Since this is a new component, set up the data
                            SetupStageData(property, presetAssetId);

                            // Add the visual elements for the new ATC
                            Foldout atcFoldout = CreateATCFoldout((IAssetTypeComponent)prefabAssetWithData.GetComponent(sourceATC.GetType()), presetAssetId);

                            assetFoldout.Add(atcFoldout);
                        }

                        EditorUtility.SetDirty(prefabAssetWithData);
                    }

                    // If a prefab instance was created to add/remove a component, remove it now
                    if (prefabInstance != null)
                    {
                        GameObject.DestroyImmediate(prefabInstance);
                    }
                }
            }
        }

        private Foldout CreateATCFoldout(IAssetTypeComponent atc, string presetAssetId)
        {
            Foldout atcFoldout = new Foldout();
            atcFoldout.text = atc.GetAssetTypeName();
            atcFoldout.value = false;

            //BaseAssetType<> is generic, so we can't cast directly to that, but Component will allow us the same thing
            var serializedAssetProperty = new SerializedObject((Component)atc);
            var assetDataProperty = serializedAssetProperty.FindProperty("assetData");

            foreach (var assetPropertyDefinition in atc.GetAllPropertyDefinition())
            {
                var propertyFoldout = new Foldout();
                propertyFoldout.text = assetPropertyDefinition.PropertyName;
                propertyFoldout.value = false;

                // We only want the end users to edit the runtime data in the Editor
                var propertySerialized = assetDataProperty.FindPropertyRelative(assetPropertyDefinition.PropertyName).FindPropertyRelative("runtimeData");

                // See AssetPropertyRuntimeData<> for these data points
                var sharedValueProperty = propertySerialized.FindPropertyRelative("sharedValue");

                var stageDataProperty = propertySerialized.FindPropertyRelative("stageAssetProperties");

                var stageDataList = new ListView();

                stageDataList.showBoundCollectionSize = false;
                stageDataList.bindingPath = stageDataProperty.propertyPath;
                stageDataList.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
                stageDataList.style.flexGrow = 1;

                stageDataList.selectionType = SelectionType.None;

                stageDataList.showAddRemoveFooter = false;
                stageDataList.showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly;
                stageDataList.Bind(serializedAssetProperty);

                var sharedValueField = new PropertyField(sharedValueProperty);
                sharedValueField.Bind(serializedAssetProperty);

                propertyFoldout.Add(sharedValueField);
                propertyFoldout.Add(stageDataList);

                atcFoldout.Add(propertyFoldout);

                if (assetPropertyDefinition.PropertyName == nameof(PositionAssetData.position))
                {
                    mappedPropertiesToPresetAsset.Add((serializedAssetProperty, assetPropertyDefinition.PropertyName), presetAssetId);
                    sharedValueField.TrackSerializedObjectValue(serializedAssetProperty, CheckPositionChange);
                }
                else if (assetPropertyDefinition.PropertyName == nameof(RotationAssetData.rotation))
                {
                    mappedPropertiesToPresetAsset.Add((serializedAssetProperty, assetPropertyDefinition.PropertyName), presetAssetId);
                    sharedValueField.TrackSerializedObjectValue(serializedAssetProperty, CheckRotationChange);
                }
                else if (assetPropertyDefinition.PropertyName == nameof(ScaleAssetData.scale))
                {
                    mappedPropertiesToPresetAsset.Add((serializedAssetProperty, assetPropertyDefinition.PropertyName), presetAssetId);
                    sharedValueField.TrackSerializedObjectValue(serializedAssetProperty, CheckScaleChange);
                }
            }

            return atcFoldout;
        }

        void CheckPositionChange(SerializedObject serializedObject)
        {
            var stageProperty = serializedObject.FindProperty("assetData")
                                                .FindPropertyRelative("position")
                                                .FindPropertyRelative("runtimeData")
                                                .FindPropertyRelative("stageAssetProperties");

            var sharedValue = serializedObject.FindProperty("assetData")
                                              .FindPropertyRelative("position")
                                              .FindPropertyRelative("runtimeData")
                                              .FindPropertyRelative("sharedValue");

            PresetScenarioEditor.MoveAsset(mappedPropertiesToPresetAsset[(serializedObject, "position")], stageProperty, sharedValue.vector3Value);
        }

        void CheckRotationChange(SerializedObject serializedObject)
        {
            var stageProperty = serializedObject.FindProperty("assetData")
                                                .FindPropertyRelative("rotation")
                                                .FindPropertyRelative("runtimeData")
                                                .FindPropertyRelative("stageAssetProperties");

            var sharedValue = serializedObject.FindProperty("assetData")
                                              .FindPropertyRelative("rotation")
                                              .FindPropertyRelative("runtimeData")
                                              .FindPropertyRelative("sharedValue");

            PresetScenarioEditor.RotateAsset(mappedPropertiesToPresetAsset[(serializedObject, "rotation")], stageProperty, sharedValue.quaternionValue);
        }

        void CheckScaleChange(SerializedObject serializedObject)
        {
            var stageProperty = serializedObject.FindProperty("assetData")
                                                .FindPropertyRelative("scale")
                                                .FindPropertyRelative("runtimeData")
                                                .FindPropertyRelative("stageAssetProperties");

            var sharedValue = serializedObject.FindProperty("assetData")
                                              .FindPropertyRelative("scale")
                                              .FindPropertyRelative("runtimeData")
                                              .FindPropertyRelative("sharedValue");

            PresetScenarioEditor.ScaleAsset(mappedPropertiesToPresetAsset[(serializedObject, "scale")], stageProperty, sharedValue.vector3Value);
        }

        public class SerializedAssetData2
        {
            [JsonConstructor]
            public SerializedAssetData2(int v, Dictionary<string, AssetDataHolder> assetTypes)
            {
                version = v;

                // Set the dictionary to ignore case with a StringComparer since the JSON serializer will change the case of classes
                assetData = new Dictionary<string, JObject>(StringComparer.OrdinalIgnoreCase);

                if (assetTypes == null)
                {
                    return;
                }

                // Convert the Dictionary for JSON serialization
                foreach (var t in assetTypes)
                {
                    assetData.Add(t.Key, JObject.FromObject(t.Value));
                }
            }

            public SerializedAssetData2(Dictionary<string, JObject> assetTypes)
            {
                version = 0;
                assetData = assetTypes;
            }

            // We want our dictionary keys to be case insensitive, but the deserialization process causes the dictionary's comparer
            // to be reset, so make sure it is set after deserialization happens
            // http://markusgreuel.net/blog/loosing-the-comparer-when-de-serializing-a-dictionary-with-the-datacontractserializer
            [OnDeserialized]
            private void OnDeserialized(StreamingContext context)
            {
                assetData = new Dictionary<string, JObject>(assetData, StringComparer.OrdinalIgnoreCase);
            }

            public JObject GetProperty(string propertyName)
            {
                var property = propertyName.ToPascalCase();

                if (assetData.ContainsKey(property))
                {
                    return assetData[property];
                }

                return null;
            }

            public int version;
            public Dictionary<string, JObject> assetData;
        }

        public class MegaAssetData
        {
            public Dictionary<string, SerializedAssetData2> assetDataByTypeName;

            public JObject GetProperty(string atcName, string propertyName)
            {
                var atc = atcName.ToPascalCase();

                if (assetDataByTypeName.ContainsKey(atc))
                {
                    return assetDataByTypeName[atc].GetProperty(propertyName);
                }

                return null;
            }
        }

        public MegaAssetData DeserializeJson(string json)
        {
            var jObject = JObject.Parse(json);

            // When working with the data here, we do not need the version
            if (jObject.ContainsKey("version"))
            {
                jObject.Remove("version");
            }

            var t = jObject.ToObject<MegaAssetData>(DefaultNewtonsoftJsonConfiguration.JsonSerializer);

            return t;
        }

        // Copy of SerializedPropertyExtensions from the Addressable package, because it's inaccessible
        public T GetActualObjectForSerializedProperty<T>(SerializedProperty property, FieldInfo field, ref string label)
        {
            try
            {
                if (property == null || field == null)
                    return default(T);
                var serializedObject = property.serializedObject;
                if (serializedObject == null)
                {
                    return default(T);
                }

                var targetObject = serializedObject.targetObject;

                if (property.depth > 0)
                {
                    var slicedName = property.propertyPath.Split('.').ToList();
                    List<int> arrayCounts = new List<int>();
                    for (int index = 0; index < slicedName.Count; index++)
                    {
                        arrayCounts.Add(-1);
                        var currName = slicedName[index];
                        if (currName.EndsWith("]"))
                        {
                            var arraySlice = currName.Split('[', ']');
                            if (arraySlice.Length >= 2)
                            {
                                arrayCounts[index - 2] = Convert.ToInt32(arraySlice[1]);
                                slicedName[index] = string.Empty;
                                slicedName[index - 1] = string.Empty;
                            }
                        }
                    }

                    while (string.IsNullOrEmpty(slicedName.Last()))
                    {
                        int i = slicedName.Count - 1;
                        slicedName.RemoveAt(i);
                        arrayCounts.RemoveAt(i);
                    }

                    if (property.propertyPath.EndsWith("]"))
                    {
                        var slice = property.propertyPath.Split('[', ']');
                        if (slice.Length >= 2)
                            label = "Element " + slice[slice.Length - 2];
                    }

                    return DescendHierarchy<T>(targetObject, slicedName, arrayCounts, 0);
                }

                var obj = field.GetValue(targetObject);
                return (T)obj;
            }
            catch
            {
                return default(T);
            }
        }

        T DescendHierarchy<T>(object targetObject, List<string> splitName, List<int> splitCounts, int depth)
        {
            if (depth >= splitName.Count)
                return default(T);

            var currName = splitName[depth];

            if (string.IsNullOrEmpty(currName))
                return DescendHierarchy<T>(targetObject, splitName, splitCounts, depth + 1);

            int arrayIndex = splitCounts[depth];

            var newField = targetObject.GetType().GetField(currName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (newField == null)
            {
                Type baseType = targetObject.GetType().BaseType;
                while (baseType != null && newField == null)
                {
                    newField = baseType.GetField(currName,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    baseType = baseType.BaseType;
                }
            }

            var newObj = newField.GetValue(targetObject);
            if (depth == splitName.Count - 1)
            {
                T actualObject = default(T);
                if (arrayIndex >= 0)
                {
                    if (newObj.GetType().IsArray && ((System.Array)newObj).Length > arrayIndex)
                        actualObject = (T)((System.Array)newObj).GetValue(arrayIndex);

                    var newObjList = newObj as IList;
                    if (newObjList != null && newObjList.Count > arrayIndex)
                    {
                        actualObject = (T)newObjList[arrayIndex];

                        //if (actualObject == null)
                        //    actualObject = new T();
                    }
                }
                else
                {
                    actualObject = (T)newObj;
                }

                return actualObject;
            }
            else if (arrayIndex >= 0)
            {
                if (newObj is IList)
                {
                    IList list = (IList)newObj;
                    newObj = list[arrayIndex];
                }
                else if (newObj is System.Array)
                {
                    System.Array a = (System.Array)newObj;
                    newObj = a.GetValue(arrayIndex);
                }
            }

            return DescendHierarchy<T>(newObj, splitName, splitCounts, depth + 1);
        }
    }
}