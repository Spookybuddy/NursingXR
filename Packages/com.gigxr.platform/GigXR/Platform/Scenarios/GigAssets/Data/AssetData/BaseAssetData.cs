namespace GIGXR.Platform.Scenarios.GigAssets.Data
{
    using Cysharp.Threading.Tasks;
    using GIGXR.Platform.ScenarioBuilder.Data;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;

    /// <summary>
    /// Asset properties that are common across all platform assets.
    ///
    /// Extend this class to define a asset type data for a specific type. Must mark the inherited class as [Serializable] as well.
    /// </summary>
    [Serializable]
    public abstract class BaseAssetData
    {
        /// <summary>
        /// Human-readable name for this asset.
        /// </summary>
        public AssetMetadataDefinition<string> name;

        /// <summary>
        /// Human-readable description for this asset.
        /// </summary>
        public AssetMetadataDefinition<string> description;

        /// <summary>
        /// All references between this asset data's property names and their property definition interface
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, IAssetPropertyDefinition> AllAssetPropertyDefinitions
        {
            get { return assetPropertyDefinitions; }
        }

        [JsonIgnore]
        public bool IsSetup { get; private set; } = false;

        private Dictionary<string, IAssetPropertyDefinition> assetPropertyDefinitions = new Dictionary<string, IAssetPropertyDefinition>();
        private Dictionary<string, FieldInfo> attachedAssetPropertyFields = new Dictionary<string, FieldInfo>();

        public List<IAssetPropertyDefinition> GetAllPropertyDefinition(IAssetTypeComponent atc, Type assetDataType = null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && assetDataType != null)
            {
                List<IAssetPropertyDefinition> result = new List<IAssetPropertyDefinition>();

                // Get all fields of the specified class.
                FieldInfo[] myFields = assetDataType.GetFields();

                // Naive solution, iterate through all Fields and look for the one that implements the IAssetPropertyDefinition interface
                foreach (FieldInfo currentFieldInfo in myFields)
                {
                    try
                    {
                        var currentFieldObject = currentFieldInfo.GetValue(this);

                        // Found an AssetPropertyDefinition with the interface, so set it up now with the info about the TypeComponent
                        if (currentFieldObject is IAssetPropertyDefinition assetPropertyDefinition)
                        {
                            result.Add(assetPropertyDefinition);

                            assetPropertyDefinition.SetupAssetPropertyEditorChanges(atc, currentFieldInfo.Name, currentFieldInfo.FieldType.GenericTypeArguments[0]);
                        }
                    }
                    catch (Exception exception)
                    {
                        Debug.LogException(exception);
                    }
                }

                return result;
            }
#endif

            return null;
        }

        public List<string> GetAllAssetPropertyNamesEditor(IAssetTypeComponent atc, Type assetDataType = null)
        {
#if UNITY_EDITOR
            if (assetDataType != null)
            {
                List<string> result = new List<string>();

                // Get all fields of the specified class.
                FieldInfo[] myFields = assetDataType.GetFields();

                // Naive solution, iterate through all Fields and look for the one that implements the IAssetPropertyDefinition interface
                foreach (FieldInfo currentFieldInfo in myFields)
                {
                    try
                    {
                        var currentFieldObject = currentFieldInfo.GetValue(this);

                        // Found an AssetPropertyDefinition with the interface, so set it up now with the info about the TypeComponent
                        if (currentFieldObject is IAssetPropertyDefinition assetPropertyDefinition)
                        {
                            result.Add(currentFieldInfo.Name);

                            assetPropertyDefinition.SetupAssetPropertyEditorChanges(atc, currentFieldInfo.Name, currentFieldInfo.FieldType.GenericTypeArguments[0]);
                        }
                    }
                    catch (Exception exception)
                    {
                        Debug.LogException(exception);
                    }
                }

                return result;
            }
#endif
            return attachedAssetPropertyFields.Keys.ToList();
        }

        public void Setup<TBaseAssetData>(IAssetTypeComponent assetTypeComponent) where TBaseAssetData : BaseAssetData
        {
            // Use reflections to find all AssetPropertyDefinition that would be implemented in a concrete class
            // Get the type handle of a specified class.
            Type myType = typeof(TBaseAssetData);

            // Get all fields of the specified class.
            FieldInfo[] myFields = myType.GetFields();

            // Naive solution, iterate through all Fields and look for the one that implements the IAssetPropertyDefinition interface
            foreach (FieldInfo currentFieldInfo in myFields)
            {
                try
                {
                    var currentFieldObject = currentFieldInfo.GetValue(this);

                    // Found an AssetPropertyDefinition with the interface, so set it up now with the info about the TypeComponent
                    if (currentFieldObject is IAssetPropertyDefinition assetPropertyDefinition)
                    {
                        attachedAssetPropertyFields.Add(currentFieldInfo.Name, currentFieldInfo);

                        assetPropertyDefinitions.Add(currentFieldInfo.Name, assetPropertyDefinition);

                        assetPropertyDefinition.SetupAssetPropertyOnRunTimeChanges(assetTypeComponent, currentFieldInfo.Name, currentFieldInfo.FieldType.GenericTypeArguments[0]);

                        assetPropertyDefinition.SetAssetPropertyDefinitionName(currentFieldInfo.Name);
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }

            IsSetup = true;
        }

        public void SetupEditor<TBaseAssetData>(IAssetTypeComponent assetTypeComponent) where TBaseAssetData : BaseAssetData
        {
            attachedAssetPropertyFields.Clear();
            assetPropertyDefinitions.Clear();

            // Use reflections to find all AssetPropertyDefinition that would be implemented in a concrete class
            // Get the type handle of a specified class.
            Type myType = typeof(TBaseAssetData);

            // Get all fields of the specified class.
            FieldInfo[] myFields = myType.GetFields();

            // Naive solution, iterate through all Fields and look for the one that implements the IAssetPropertyDefinition interface
            foreach (FieldInfo currentFieldInfo in myFields)
            {
                try
                {
                    var currentFieldObject = currentFieldInfo.GetValue(this);

                    // Found an AssetPropertyDefinition with the interface, so set it up now with the info about the TypeComponent
                    if (currentFieldObject is IAssetPropertyDefinition assetPropertyDefinition)
                    {
                        attachedAssetPropertyFields.Add(currentFieldInfo.Name, currentFieldInfo);

                        assetPropertyDefinitions.Add(currentFieldInfo.Name, assetPropertyDefinition);

                        assetPropertyDefinition.SetupAssetPropertyEditorChanges(assetTypeComponent, currentFieldInfo.Name, currentFieldInfo.FieldType.GenericTypeArguments[0]);
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        public async UniTask LoadAssetProperty(string propertyName, JToken assetRuntimeData)
        {
            if (assetPropertyDefinitions.ContainsKey(propertyName))
            {
                await assetPropertyDefinitions[propertyName].UpdateInternalRuntimeData(assetRuntimeData);
            }
            else
            {
                Debug.LogError($"{propertyName} was not found in the dictionary of known asset properties while loading new data {assetRuntimeData}.");
            }
        }

        public void TearDown<TBaseAssetData>() where TBaseAssetData : BaseAssetData
        {
            foreach (var assetPropertyDefinition in assetPropertyDefinitions.Values)
            {
                assetPropertyDefinition.TearDownAssetPropertyOnRunTimeChanges();
            }

            assetPropertyDefinitions.Clear();
            attachedAssetPropertyFields.Clear();
        }

        public void SetAssetProperty(string propertyName, object newValue)
        {
            if (attachedAssetPropertyFields.ContainsKey(propertyName))
            {
                var currentPropertyDefinition = (IAssetPropertyDefinition)attachedAssetPropertyFields[propertyName].GetValue(this);

                currentPropertyDefinition.SetPropertyValue(propertyName, newValue);
            }
        }

        public void SetAssetProperty(string propertyName, byte[] newValue)
        {
            if (attachedAssetPropertyFields.ContainsKey(propertyName))
            {
                var currentPropertyDefinition = (IAssetPropertyDefinition)attachedAssetPropertyFields[propertyName].GetValue(this);

                currentPropertyDefinition.SetPropertyValue(propertyName, newValue);
            }
        }

        public object GetAssetProperty(string propertyName)
        {
            if (attachedAssetPropertyFields.ContainsKey(propertyName))
            {
                var currentPropertyDefinition = (IAssetPropertyDefinition)attachedAssetPropertyFields[propertyName].GetValue(this);

                return currentPropertyDefinition.GetRuntimePropertyValue();
            }

            return null;
        }

        public byte[] GetAssetPropertyByteArray(string propertyName)
        {
            if (attachedAssetPropertyFields.ContainsKey(propertyName))
            {
                var currentPropertyDefinition = (IAssetPropertyDefinition)attachedAssetPropertyFields[propertyName].GetValue(this);

                return currentPropertyDefinition.GetRuntimePropertyValueByteArray();
            }

            return null;
        }

        public object GetPropertyValueAtStage(Guid stageId, string propertyName, bool returnInitialData = true)
        {
            if (attachedAssetPropertyFields.ContainsKey(propertyName))
            {
                var currentPropertyDefinition = (IAssetPropertyDefinition)attachedAssetPropertyFields[propertyName].GetValue(this);

                return currentPropertyDefinition.GetPropertyValueAtStage(stageId, returnInitialData);
            }

            return null;
        }

        /// <summary>
        /// Like GetPropertyValueAtStage but does not assume to have all the Asset Properties attached and will
        /// search for all them. Useful for in Editor use only.
        /// </summary>
        /// <param name="stageId"></param>
        /// <param name="propertyName"></param>
        /// <param name="returnInitialData"></param>
        /// <returns></returns>
        public object GetPropertyValueAtStageSlow(Guid stageId, string propertyName, bool returnInitialData = true)
        {
            foreach (FieldInfo currentFieldInfo in GetType().GetFields())
            {
                try
                {
                    var currentFieldObject = currentFieldInfo.GetValue(this);

                    // Found an AssetPropertyDefinition with the interface, so set it up now with the info about the TypeComponent
                    if (currentFieldObject is IAssetPropertyDefinition assetPropertyDefinition)
                    {
                        if (propertyName == currentFieldInfo.Name)
                        {
                            var currentPropertyDefinition = (IAssetPropertyDefinition)currentFieldInfo.GetValue(this);

                            return currentPropertyDefinition.GetPropertyValueAtStage(stageId, returnInitialData);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }

            return null;
        }

        public void SetStage(Guid stageId)
        {
            foreach (IAssetPropertyDefinition assetPropertyDefinition in assetPropertyDefinitions.Values)
            {
                assetPropertyDefinition.SetStageValue(stageId);
            }
        }

        public void ResetAllAssetPropertyDefintionSharedValueToDefault()
        {
            foreach (IAssetPropertyDefinition assetPropertyDefinition in assetPropertyDefinitions.Values)
            {
                assetPropertyDefinition.SetSharedRuntimeValueToDefault();
            }
        }

        public void AddStage(string stageId, string? assetProperty = null)
        {
            if (string.IsNullOrEmpty(assetProperty))
            {
                foreach (IAssetPropertyDefinition assetPropertyDefinition in assetPropertyDefinitions.Values)
                {
                    assetPropertyDefinition.AddStageValue(stageId);
                }
            }
            else
            {
                var assetProp = assetPropertyDefinitions.Values.Where(property => property.PropertyName == assetProperty).FirstOrDefault();

                if (assetProp != null)
                {
                    assetProp.AddStageValue(stageId);
                }
            }
        }

        public void AddKnownStage(string stageId, string assetProperty, RuntimeStageInput stageData)
        {
            var assetProp = assetPropertyDefinitions.Values.Where(property => property.PropertyName == assetProperty).FirstOrDefault();

            if (assetProp != null)
            {
                assetProp.AddKnownStageValue(stageId, stageData);
            }
        }

        public void RemoveStage(string stageId)
        {
            foreach (IAssetPropertyDefinition assetPropertyDefinition in assetPropertyDefinitions.Values)
            {
                assetPropertyDefinition.RemoveStage(stageId);
            }
        }

        public void RemoveStage(int stageIndex)
        {
            foreach (IAssetPropertyDefinition assetPropertyDefinition in assetPropertyDefinitions.Values)
            {
                assetPropertyDefinition.RemoveStage(stageIndex);
            }
        }

        /// <summary>
        /// Register a validator for the specified property.
        /// It will block new property updates at runtime if the validator returns false.
        /// </summary>
        /// <param name="validator"></param>
        /// <param name="propertyName"></param>
        public void RegisterPropertyValidator(string propertyName, Func<object, (object, bool)> lambda)
        {
            if (assetPropertyDefinitions.TryGetValue(propertyName, out var propertyDefinition))
            {
                propertyDefinition.RegisterValidator(lambda);
            }
        }

        /// <summary>
        /// Unregister a registered validator for the specified property.
        /// </summary>
        /// <param name="validator"></param>
        /// <param name="propertyName"></param>
        public void UnregisterPropertyValidator(string propertyName, Func<object, (object, bool)> lambda)
        {
            if (assetPropertyDefinitions.TryGetValue(propertyName, out var propertyDefinition))
            {
                propertyDefinition.UnregisterValidator(lambda);
            }
        }

        public void SetupAllInitialValues()
        {
            foreach (var assetPropertyDefinition in AllAssetPropertyDefinitions.Values)
            {
                assetPropertyDefinition.SetupInitialValues();
            }
        }

        public void SetupInitialValue(string propertyDefinitionName)
        {
            if (AllAssetPropertyDefinitions.ContainsKey(propertyDefinitionName))
            {
                AllAssetPropertyDefinitions[propertyDefinitionName].SetupInitialValues();
            }
            else
            {
                Debug.LogError($"[BaseAssetData] Could not set initial value on property definition {propertyDefinitionName} since it does not" +
                    $" exist in the dictionary of known asset property definitions.");
            }
        }

        public string[] GetAssetPropertiesWithRuntimeStageValues()
        {
            // Use reflections to find all AssetPropertyDefinition that would be implemented in a concrete class
            FieldInfo[] myFields = GetType().GetFields();
            List<string> assetProperties = new List<string>();

            // Naive solution, iterate through all Fields and look for the one that implements the IAssetPropertyDefinition interface
            foreach (FieldInfo currentFieldInfo in myFields)
            {
                var currentFieldObject = currentFieldInfo.GetValue(this);

                // Found an AssetPropertyDefinition with the interface, so set it up now with the info about the TypeComponent
                if (currentFieldObject is IAssetPropertyDefinition assetPropertyDefinition)
                {
                    // Return the property name that has any stage data
                    if (assetPropertyDefinition.GetRuntimeStagePropertyCount() != 0)
                        assetProperties.Add(currentFieldInfo.Name);
                }
            }

            return assetProperties.ToArray();
        }

        public void ClearRuntimeStageDataValues()
        {
            // Use reflections to find all AssetPropertyDefinition that would be implemented in a concrete class
            FieldInfo[] myFields = GetType().GetFields();

            // Naive solution, iterate through all Fields and look for the one that implements the IAssetPropertyDefinition interface
            foreach (FieldInfo currentFieldInfo in myFields)
            {
                var currentFieldObject = currentFieldInfo.GetValue(this);

                // Found an AssetPropertyDefinition with the interface, so set it up now with the info about the TypeComponent
                if (currentFieldObject is IAssetPropertyDefinition assetPropertyDefinition)
                {
                    assetPropertyDefinition.ClearRuntimeStageDataValues();
                }
            }
        }

        public string SerializeAssetPropertyData(string propertyName, Formatting format)
        {
            if (assetPropertyDefinitions.ContainsKey(propertyName))
            {
                return JsonConvert.SerializeObject(assetPropertyDefinitions[propertyName], format);
            }
            else
            {
                Debug.LogWarning($"[AssetData] {name} Asset Data does not have property {propertyName}.");

                return null;
            }
        }

        public void ValidateForEditor()
        {
#if UNITY_EDITOR
            foreach (IAssetPropertyDefinition currentPropertyDefintion in assetPropertyDefinitions.Values)
            {
                try
                {
                    currentPropertyDefintion.ValidateForEditor();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[AssetData] {name} failed to validate {currentPropertyDefintion.PropertyName}. See exception below.");
                    Debug.LogException(ex);
                }
            }
#endif
        }
    }
}