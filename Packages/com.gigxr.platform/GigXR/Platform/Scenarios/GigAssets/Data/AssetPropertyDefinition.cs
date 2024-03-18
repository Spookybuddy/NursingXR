namespace GIGXR.Platform.Scenarios.GigAssets.Data
{
    using Cysharp.Threading.Tasks;
    using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
    using GIGXR.Platform.Utilities;
    using GIGXR.Utilities;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.ComponentModel;
    using UnityEngine;

    /// <summary>
    /// Represents data for a property on an asset.
    ///
    /// An asset property will have design data, and runtime data for each stage as well as a "shared" value that can be
    /// selected on a per-stage basis.
    /// </summary>
    /// <typeparam name="TSerializable">Any type that can be serialized by Unity's built-in serialization.</typeparam>
    [Serializable]
    public class AssetPropertyDefinition<TSerializable> : IAssetPropertyDefinition
    {
        /// <summary>
        /// The design-time data for an asset property.
        /// </summary>
        [JsonIgnore]
        [ShowInPlayerMode(UnityPlayerModes.EditMode)]
        public AssetPropertyDesignTimeData<TSerializable> designTimeData;

        /// <summary>
        /// The runtime data for an asset property.
        /// </summary>
        [DontShowInPrefabMode]
        public AssetPropertyRuntimeData<TSerializable> runtimeData;

        #region IAssetPropertyDefinitionImplementation

        public string PropertyName
        {
            get
            {
                return _propertyName;
            }
        }

        protected string _propertyName;

        [JsonIgnore]
        public Type SpecifiedType
        {
            get
            {
                return _specifiedType;
            }
        }

        protected Type _specifiedType;

        [JsonIgnore]
        public object DefaultValue 
        { 
            get
            {
                return designTimeData.defaultValue;
            }
        }

        protected IAssetTypeComponent attachedAssetTypeComponent;

        [JsonIgnore]
        public IAssetTypeComponent AttachedAssetTypeComponent 
        { 
            get
            {
                return attachedAssetTypeComponent;
            }
        }

        public void SetupAssetPropertyOnRunTimeChanges(IAssetTypeComponent assetTypeComponent, string propertyName, Type type)
        {
            _propertyName = propertyName;

            _specifiedType = type;

            runtimeData.SetAssetPropertyDefinition(this);

            SetupInitialValues();

            // Maintain reference to the IAttachedAssetTypeComponent which allows this data to trigger certain actions on the MonoBehavior
            attachedAssetTypeComponent = assetTypeComponent;

            // Start listening to when the property is changed during runtime
            runtimeData.PropertyChanged += RuntimeData_PropertyChanged;
        }

        public void SetupAssetPropertyEditorChanges(IAssetTypeComponent assetTypeComponent, string propertyName, Type type)
        {
            runtimeData.SetAssetPropertyDefinition(this);

            _propertyName = propertyName;

            _specifiedType = type;

            attachedAssetTypeComponent = assetTypeComponent;
        }

        public void TearDownAssetPropertyOnRunTimeChanges()
        {
            runtimeData.PropertyChanged -= RuntimeData_PropertyChanged;

            attachedAssetTypeComponent = null;
        }

        private void RuntimeData_PropertyChanged(AssetPropertyRuntimeData<TSerializable> runtimeData, AssetPropertyChangeEventArgs e)
        {
            // Forward to whatever AssetTypeComponent that uses this PropertyDefinition know that a property has changed
            attachedAssetTypeComponent?.HandlePropertyChange(runtimeData, e);
        }

        public void SetAssetPropertyDefinitionName(string assetProperty)
        {
            runtimeData.SetAssetPropertyDefinitionName(assetProperty);
        }

        public void SetPropertyValue(string propertyName, object newValue)
        {
            if (newValue.GetType() != typeof(TSerializable))
            {
                // HACK, when passing in data through the DDR, Vector3s do not get converted before this point, so check if a JObject was given here
                if (newValue is JObject t)
                {
                    newValue = t.ToObject<TSerializable>(DefaultNewtonsoftJsonConfiguration.JsonSerializer);
                }
                // HACK, ints are deserialized as Int64/long, so need to make a check here as well
                else if (newValue is long incorrectInt && typeof(TSerializable) == typeof(int))
                {
                    newValue = (int)incorrectInt;
                }
                else if (typeof(TSerializable) == typeof(Vector3) && newValue.GetType() == typeof(string))
                {
                    string valueString = (string)newValue;

                    string[] temp = valueString.Substring(1, valueString.Length - 2).Split(',');
                    float x = float.Parse(temp[0]);
                    float y = float.Parse(temp[1]);
                    float z = float.Parse(temp[2]);
                    newValue = new Vector3(x, y, z);
                }
                // HACK, for in Editor, we pass in a String through the Editor. This will only work for primitive types though.
                else if (newValue is string stringValue)
                {
                    try
                    {
                        TypeConverter typeConverter = TypeDescriptor.GetConverter(typeof(TSerializable));
                        newValue = typeConverter.ConvertFromString(stringValue);
                    }
                    catch (Exception ex)
                    {
                        GIGXR.Platform.Utilities.Logger.Error($"Asset: {attachedAssetTypeComponent.GetAssetTypeName()}, Exception occurred while setting property {propertyName}", "GigAssetManager", ex);
                        return;
                    }
                }
                else
                {
                    GIGXR.Platform.Utilities.Logger.Warning($"Asset: {attachedAssetTypeComponent.GetAssetTypeName()}, New value for {propertyName} is of type {newValue.GetType()}, not {typeof(TSerializable)} and could not be set.",
                        "GigAssetManager");
                    return;
                }
            }

            GIGXR.Platform.Utilities.Logger.Debug
            (
                $"Asset: {attachedAssetTypeComponent.GetAssetTypeName()}, Property: {propertyName}, Value: {(TSerializable)newValue}",
                "GigAssetManager"
            );

            // This will raise the AssetPropertyRuntimeData's PropertyChanged event
            runtimeData.Value = (TSerializable)newValue;
        }

        public void SetPropertyValue(string propertyName, byte[] newValue)
        {
            var value = SerializationUtilities.ByteArrayToObject<TSerializable>(newValue);

            GIGXR.Platform.Utilities.Logger.Debug
            (
                $"Asset: {attachedAssetTypeComponent.GetAssetTypeName()}, Property: {propertyName}, Value: {value}",
                "GigAssetManager"
            );

            runtimeData.Value = value;
        }

        public object GetPropertyValueAtStage(Guid stageId, bool returnInitialData = true)
        {
            var stageProperties = runtimeData.GetStageAssetProperty(stageId.ToString());

            if (returnInitialData)
            {
                return stageProperties.initialValue;
            }
            else
            {
                return stageProperties.useShared ? runtimeData.SharedValue : stageProperties.localValue;
            }

            // TODO designTimeData.defaultValue?
        }

        public object GetRuntimePropertyValue()
        {
            return runtimeData.Value;
        }

        public object GetRuntimePropertyValueAtStage(string stageId)
        {
            var stageProperty = runtimeData.GetStageAssetProperty(stageId);

            if (stageProperty.useShared)
            {
                return runtimeData.SharedValue;
            }
            else
            {
                return stageProperty.localValue;
            }
        }

        public byte[] GetRuntimePropertyValueByteArray()
        {
            return SerializationUtilities.ObjectToByteArray(runtimeData.Value);
        }

        public object GetDesignPropertyValue()
        {
            return designTimeData.defaultValue;
        }

        public void SetStageValue(Guid stageId)
        {
            runtimeData.SetStageId(stageId);
        }

        public void AddStageValue(string stageId)
        {
            runtimeData.AddStageId(stageId);
        }

        public void AddKnownStageValue(string stageId, RuntimeStageInput stageData)
        {
            runtimeData.AddStage(stageId, stageData.useShared, stageData.resetValueOnStageChange, (TSerializable)stageData.localValue);
        }

        public void SetSharedRuntimeValueToDefault()
        {
            runtimeData.SetSharedValueToDefault();
        }

        public void RemoveStage(string stageId)
        {
            if (!runtimeData.RemoveStage(stageId))
            {
                Debug.LogWarning($"Failed to remove {stageId} on Property {PropertyName}");
            }
        }

        public void RemoveStage(int stageIndex)
        {
            if (!runtimeData.RemoveStage(stageIndex))
            {
                Debug.LogWarning($"Failed to remove {stageIndex} on Property {PropertyName}");
            }
        }

        public void SetupInitialValues()
        {
            // Set up the values that were set by the designer
            runtimeData.UpdateValue(designTimeData.defaultValue, AssetPropertyChangeOrigin.Initialization);
        }

        /// <summary>
        /// Make sure that if the Application is in Edit Mode, do not allow the runtime data to be edited.
        /// Also only call if the parent BaseAssetTypeComponent is a runtime instance <see cref="BaseAssetTypeComponent{TBaseAssetData}.isRuntimeInstance"/>.
        /// This should NEVER be called on prefabs. This is intended to handle inspector-originated changes to runtime data.
        /// </summary>
        public virtual void ValidateForEditor()
        {
#if UNITY_EDITOR
            if (!attachedAssetTypeComponent.IsRuntimeInstance)
            {
                return;
            }

            if (!Application.isPlaying)
            {
                runtimeData = default;
            }
            else
            {
                runtimeData.Value = runtimeData.Value;
            }
#endif
        }

        #endregion

        public async UniTask UpdateInternalRuntimeData(JToken newRuntimeData)
        {
            await runtimeData.UpdateInternalValues(newRuntimeData);
        }

        /// <summary>
        /// Register a validator, which will block new property updates at runtime if the validator returns false.
        /// </summary>
        /// <param name="validator"></param>
        public void RegisterValidator(Func<object, (object, bool)> lambda)
        {
            runtimeData.RegisterValidator(lambda);
        }

        /// <summary>
        /// Unregister a registered validator.
        /// </summary>
        /// <param name="validator"></param>
        public void UnregisterValidator(Func<object, (object, bool)> lambda)
        {
            runtimeData.UnregisterValidator(lambda);
        }

        // This could have been a property, but then we would have to worry about whether it gets serialized or not, so
        // made it as a method to access it instead
        public int GetRuntimeStagePropertyCount()
        {
            return runtimeData.StageAssetProperties.Count;
        }

        public void ClearRuntimeStageDataValues()
        {
            runtimeData.StageAssetProperties.Clear();
        }

        public void SetValuePersistance(bool persist)
        {
            runtimeData.SetValuePersistance(persist);
        }
    }
}