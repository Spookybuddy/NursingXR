namespace GIGXR.Platform.Scenarios.GigAssets.Data
{
    using Cysharp.Threading.Tasks;
    using GIGXR.Platform.ScenarioBuilder.Data;
    using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
    using GIGXR.Utilities;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    /// <summary>
    /// Represents the runtime data for an asset property.
    /// </summary>
    /// <typeparam name="TSerializable">Any type that can be serialized by Unity's built-in serialization.</typeparam>
    [Serializable]
    public class AssetPropertyRuntimeData<TSerializable> : IAssetPropertyRuntime<TSerializable>
    {
        private IAssetPropertyDefinition assetPropertyDefinition;

        private const bool DefaultUseSharedValue = true;

        private readonly object valueLock = new object();

        [SerializeField]
        private TSerializable sharedValue;

        [JsonIgnore]
        public TSerializable SharedValue { get { return sharedValue; } }

        [SerializeField]
        private List<StageAssetProperty<TSerializable>> stageAssetProperties =
            new List<StageAssetProperty<TSerializable>>();

        [JsonIgnore]
        public List<StageAssetProperty<TSerializable>> StageAssetProperties { get { return stageAssetProperties; } }

        private Guid stageId;

        //private IAssetPropertyDataValidator<TSerializable> validator;

        public delegate void PropertyChangeEventHandler(AssetPropertyRuntimeData<TSerializable> runtimeData, AssetPropertyChangeEventArgs e);

        private List<Func<object, (object, bool)>> validators = new List<Func<object, (object, bool)>>();

        /// <summary>
        /// An event raised when the value of this property was changed. This includes if the value changes itself, or
        /// the use shared or stage parameters are changed as well.
        /// </summary>
        public event PropertyChangeEventHandler PropertyChanged;

        /// <inheritdoc cref="IAssetPropertyRuntime{TSerializable}.StageId"/>
        [JsonIgnore]
        public Guid StageId
        {
            get => stageId;
        }

        private StageAssetProperty<TSerializable> LastSetStage
        {
            get
            {
                if (_lastSetStage == null)
                    _lastSetStage = GetOrInitializeStageAssetProperty();

                return _lastSetStage;
            }
            set
            {
                _lastSetStage = value;
            }
        }

        private StageAssetProperty<TSerializable> _lastSetStage;

        public void SetStageId(Guid newId)
        {
            stageId = newId;
            LastSetStage = GetOrInitializeStageAssetProperty(newId.ToString());

            // reset stage value if needed
            if (LastSetStage.resetValueOnStageChange)
            {
                LastSetStage.localValue = LastSetStage.initialValue;
            }

            OnPropertyChanged(new AssetPropertyChangeEventArgs(PropertyAssetName,
                                                               LastSetStage.useShared ? nameof(StageId) : StageId.ToString(),
                                                               LastSetStage.useShared ? sharedValue : LastSetStage.localValue,
                                                               AssetPropertyChangeOrigin.StageChange));
        }

        public void AddStageId(string stageId)
        {
            InitializeStageAssetProperty(stageId);
        }

        public void AddStage(string stageId, bool useShared, bool resetValueOnStageChange, TSerializable value)
        {
            StageAssetProperty<TSerializable> property = new StageAssetProperty<TSerializable>
            {
                stageId = stageId,
                useShared = useShared,
                localValue = value,
                resetValueOnStageChange = resetValueOnStageChange
            };

            stageAssetProperties.Add(property);
        }

        public void SetSharedValueToDefault()
        {
            sharedValue = (TSerializable)assetPropertyDefinition.DefaultValue;
        }

        /// <summary>
        /// Provide a reference to the asset property definition the runtime is attached to, allowing
        /// access to design related data.
        /// </summary>
        /// <param name="assetProperty"></param>
        public void SetAssetPropertyDefinition(IAssetPropertyDefinition assetProperty)
        {
            assetPropertyDefinition = assetProperty;
        }

        public bool RemoveStage(string stageId)
        {
            foreach (var stage in stageAssetProperties)
            {
                if (stage.stageId == stageId)
                {
                    stageAssetProperties.Remove(stage);

                    return true;
                }
            }

            return false;
        }

        public bool RemoveStage(int stageIndex)
        {
            if(stageIndex < 0 || stageIndex >= stageAssetProperties.Count)
                return false;

            stageAssetProperties.RemoveAt(stageIndex);

            return true;
        }

        /// <inheritdoc cref="IAssetPropertyRuntime{TSerializable}.UseShared"/>
        [JsonIgnore]
        public bool UseShared
        {
            get
            {
                if (StageId == default)
                {
                    // Uninitialized stage, shouldn't really happen in real use.
                    return false;
                }

                return LastSetStage.useShared;
            }
            set
            {
                if (StageId == default)
                {
                    // Uninitialized stage, shouldn't really happen in real use.
                    OnPropertyChanged();
                    return;
                }

                LastSetStage.useShared = value;
                OnPropertyChanged();
            }
        }

        /// <inheritdoc cref="IAssetPropertyRuntime{TSerializable}.Value"/>
        [JsonIgnore]
        public TSerializable Value
        {
            get
            {
                lock (valueLock)
                {
                    if (StageId == default)
                    {
                        // Uninitialized stage, shouldn't really happen in real use.
                        return sharedValue;
                    }

                    if (LastSetStage.useShared)
                    {
                        return sharedValue;
                    }

                    return LastSetStage.localValue;
                }
            }
            set
            {
                SetValueInternal(value);
            }
        }

        private void SetValueInternal(TSerializable value, AssetPropertyChangeOrigin origin = AssetPropertyChangeOrigin.ValueSet)
        {
            lock (valueLock)
            {
                (object, bool) validation = ValidateValue(value);
                if (validation.Item2)
                {
                    if (StageId == default)
                    {
                        // Uninitialized stage, shouldn't really happen in real use.
                        sharedValue = (TSerializable)validation.Item1;
                        OnPropertyChanged(propertyName: nameof(Value));
                        return;
                    }

                    if (LastSetStage.useShared)
                    {
                        // If the serialized property is a primitive (meaning we know how to compare it here) and the value is being set to the same thing,
                        // then don't bother setting the new value and invoking the property change event
                        if (LastSetStage.GetType().IsPrimitive && sharedValue.Equals(value))
                            return;

                        sharedValue = (TSerializable)validation.Item1;
                        OnPropertyChanged(propertyName: nameof(Value));

                        return;
                    }

                    // If the serialized property is a primitive (meaning we know how to compare it here) and the value is being set to the same thing,
                    // then don't bother setting the new value and invoking the property change event
                    if (LastSetStage.GetType().IsPrimitive && LastSetStage.Equals(value))
                        return;

                    LastSetStage.localValue = (TSerializable)validation.Item1;

                    OnPropertyChanged(propertyName: nameof(Value));
                }
            }
        }

        [JsonIgnore]
        private string PropertyAssetName { get; set; }

        protected StageAssetProperty<TSerializable> GetOrInitializeStageAssetProperty(string id = null)
        {
            return GetStageAssetProperty(id) ?? InitializeStageAssetProperty();
        }

        public StageAssetProperty<TSerializable> GetStageAssetProperty(string id)
        {
            return stageAssetProperties.FirstOrDefault(prop => prop.stageId == id);
        }

        protected StageAssetProperty<TSerializable> InitializeStageAssetProperty(string existingId = null)
        {
            // if a stage asset property already exists with the specified id, simply return it instead of creating a new one.
            if (existingId != null)
            {
                foreach (var stageProperty in stageAssetProperties)
                {
                    if (stageProperty.stageId == existingId)
                    {
                        return stageProperty;
                    }
                }
            }

            StageAssetProperty<TSerializable> property = new StageAssetProperty<TSerializable>
            {
                stageId = existingId != null ? existingId : stageId.ToString(),
                useShared = DefaultUseSharedValue,
                localValue = (TSerializable)assetPropertyDefinition.DefaultValue
            };

            stageAssetProperties.Add(property);
            return property;
        }

        protected virtual void OnPropertyChanged(AssetPropertyChangeEventArgs customChangeEvent = null, [CallerMemberName] string propertyName = null, AssetPropertyChangeOrigin origin = AssetPropertyChangeOrigin.ValueSet)
        {
            if (customChangeEvent == null)
            {
                customChangeEvent = new AssetPropertyChangeEventArgs(PropertyAssetName,
                                                                     propertyName,
                                                                     LastSetStage.useShared ? sharedValue : LastSetStage.localValue,
                                                                     origin);
            }

            PropertyChanged?.Invoke(this, customChangeEvent);
        }

        public void SetAssetPropertyDefinitionName(string assetName)
        {
            PropertyAssetName = assetName;
        }

        public void UpdateValue(TSerializable newValue, AssetPropertyChangeOrigin origin = AssetPropertyChangeOrigin.ValueSet)
        {
            lock (valueLock)
            {
                SetValueInternal(newValue, origin);
            }
        }

        public void UpdateValueLocally(TSerializable newValue)
        {
            lock (valueLock)
            {
                (object, bool) validation = ValidateValue(newValue);
                if (validation.Item2)
                {
                    if (LastSetStage.useShared)
                    {
                        sharedValue = (TSerializable)validation.Item1;
                        return;
                    }

                    LastSetStage.localValue = (TSerializable)validation.Item1;
                }
            }
        }

        internal UniTask UpdateInternalValues(JToken newData)
        {
            lock (valueLock)
            {
                try
                {
                    // The expected data will have the values nested in the runtime data, so use the path to find those properties
                    var runtimeDataToken = newData.SelectToken($"{nameof(AssetPropertyDefinition<int>.runtimeData)}.{nameof(sharedValue)}");

                    sharedValue = runtimeDataToken.ToObject<TSerializable>(DefaultNewtonsoftJsonConfiguration.JsonSerializer);

                    var stageAssetData = newData.SelectToken($"{nameof(AssetPropertyDefinition<int>.runtimeData)}.{nameof(stageAssetProperties)}");
                    stageAssetProperties = stageAssetData.ToObject<List<StageAssetProperty<TSerializable>>>(DefaultNewtonsoftJsonConfiguration.JsonSerializer);

                    OnPropertyChanged(new AssetPropertyChangeEventArgs(PropertyAssetName,
                                                                       nameof(UpdateInternalValues),
                                                                       LastSetStage.useShared ? sharedValue : LastSetStage.localValue,
                                                                       AssetPropertyChangeOrigin.Initialization));

                    // Set up all stage asset properties to reset on stage change, if needed
                    foreach (var stageAssetProperty in stageAssetProperties)
                    {
                        if (stageAssetProperty.resetValueOnStageChange)
                        {
                            stageAssetProperty.initialValue = stageAssetProperty.localValue;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log($"[AssetPropertyRuntimeData] Failed while setting up internal value: {e}");
                }
            }

            return UniTask.CompletedTask;
        }

        /// <summary>
        /// Register a validator, which will block new property updates at runtime if the validator returns false.
        /// </summary>
        /// <param name="validator"></param>
        public void RegisterValidator(Func<object, (object, bool)> validator)
        {
            validators.Add(validator);
        }

        /// <summary>
        /// Unregister a registered validator.
        /// </summary>
        /// <param name="validator"></param>
        public void UnregisterValidator(Func<object, (object, bool)> validator)
        {
            validators.Remove(validator);
        }

        /// <summary>
        /// Check a candidate value against all validators.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private (object, bool) ValidateValue(object value)
        {
            bool applyUpdate = true;

            // pass through validators once to allow each validator to update or block
            foreach (var lambda in validators)
            {
                (value, applyUpdate) = lambda(value);

                // if the validator blocked the update entirely, we're done here
                if (!applyUpdate)
                {
                    return (value, applyUpdate);
                }
            }

            // pass through validators once more to ensure all validators agree
            object validated = value;
            foreach (var lambda in validators)
            {
                (validated, applyUpdate) = lambda(validated);

                // if the validator blocked the update now or the value changed in this second pass, validator conflict.
                if (!applyUpdate || (validated != null && !validated.Equals(value)) || (validated == null && value != null))
                {
                    Debug.LogError($"[AssetPropertyRuntimeData] update blocked for {PropertyAssetName} due to validator conflict. Ensure that {value.GetType()} overrides Equals.");
                    return (value, applyUpdate);
                }
            }

            return (value, applyUpdate);
        }

        public void SetValuePersistance(bool persist)
        {
            foreach (var stageProperty in stageAssetProperties)
            {
                stageProperty.resetValueOnStageChange = !persist;
            }
        }
    }
}