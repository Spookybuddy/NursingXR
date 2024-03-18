namespace GIGXR.Platform.Scenarios.GigAssets
{
    using Data;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using EventArgs;
    using System.Runtime.Serialization;
    using Cysharp.Threading.Tasks;
    using GIGXR.Utilities;
    using System.Linq;
    using Utilities;
    using UnityEngine.AddressableAssets;
    using System.Reflection;
    using Cysharp.Threading.Tasks.Linq;
    using Logger = GIGXR.Platform.Utilities.Logger;

    /// <summary>
    /// Provides the linkage to an instance of a GIGXR Asset and its AssetTypeComponents and data.
    /// </summary>
    [RequireComponent(typeof(MetaDataAssetTypeComponent))]
    public class AssetMediator : MonoBehaviour, IAssetMediator
    {
        #region PublicAPI

        public static readonly int Version = 0;

        public bool AssetDataRegistrationComplete
        {
            get
            {
                return assetDataRegistrationStatus == AssetDataRegistrationStatus.Registered;
            }
        }

        #endregion

        #region Private Variables

        [Header("The Asset Type ID, e.g., \"content-window\"")] [SerializeField]
        private string assetTypeId;

        [SerializeField] [GIGXR.Utilities.ReadOnly] 
        private Guid assetId;

        // For viewing in the Editor
        [JsonIgnore] [SerializeField] [GIGXR.Utilities.ReadOnly]
        private string presetAssetId;

        [SerializeField] 
        private List<AssetReference> assetTypeDependencyIds;

        public IEnumerable<string> AssetTypeDependencyIds
        {
            get
            {
                return assetTypeDependencyIds.Select(reference => reference.RuntimeKey.ToString());
            }
        }

        /// <summary>
        /// The version of the Asset Type represented by this mediator.
        /// </summary>
        /// <remarks>
        /// This is not used at this time (2021/10/13) and is reserved for future use. Semantic versioning does not seem
        /// to be needed for this as we only need to track breaking changes so a simple int will do.
        ///
        /// The flow might look something like this:
        ///
        /// 1. Deserialize this data to get the version.
        /// 2. Check the version against the version on the AssetType.
        /// 3. If it's the same, deserialize the rest.
        /// 4. If it's different, run some sort of migration to upgrade the data.
        ///
        /// For #4, as an example if you were to have version 1 of the asset data and you needed to create a breaking
        /// change so you wanted to create version 2, you would be required to create a migration to go from 1 to 2.
        ///
        /// If we used a simple incrementing int we could enforce the creation of these migrations. E.g., if version 2
        /// is created but there are any missing migrations for less than 2 then it would throw an Exception and fail
        /// fast.
        /// </remarks>
        [HideInInspector] [Min(0)] [SerializeField] [Tooltip("Unused at this time. Reserved for future use.")]
        private int version;

        private Dictionary<string, List<Action<AssetPropertyChangeEventArgs>>> propertyChangeActions =
            new Dictionary<string, List<Action<AssetPropertyChangeEventArgs>>>();

        private readonly Dictionary<string, AssetDataHolder> assetDataByTypeName = new Dictionary<string, AssetDataHolder>();

        private readonly HashSet<IAssetTypeComponent> knownAssetTypeComponents = new HashSet<IAssetTypeComponent>();

        private readonly Dictionary<string, (IAssetPropertyDefinition, IAssetTypeComponent, BaseAssetData)>
            knownAssetPropertiesMappings =
                new Dictionary<string, (IAssetPropertyDefinition, IAssetTypeComponent, BaseAssetData)>();

        // Allows duplicate state names and returns the results for all
        private readonly Dictionary<string, HashSet<IAssetTypeComponent>> knownAssetStateProperties =
            new Dictionary<string, HashSet<IAssetTypeComponent>>();

        // Allows duplicate method names and will trigger all with the same name
        private readonly Dictionary<string, HashSet<IAssetTypeComponent>> knownAssetMethods =
            new Dictionary<string, HashSet<IAssetTypeComponent>>();

        // Allows duplicate method names and will trigger all with the same name
        private readonly Dictionary<string, HashSet<IAssetTypeComponent>> knownAssetEvents =
            new Dictionary<string, HashSet<IAssetTypeComponent>>();

        // Keep track of the asset type components that need to be synced
        private readonly HashSet<IRuntimeSyncable> knownSyncables = new HashSet<IRuntimeSyncable>();

        // Keep track of the asset type components that do not send out property changes, useful for some functions
        private readonly HashSet<LocalAssetTypeComponent> knownLocalAssetTypeComponent = new HashSet<LocalAssetTypeComponent>();

        public event EventHandler<AssetPropertyChangeEventArgs> PropertyChanged;

        private AssetDataRegistrationStatus assetDataRegistrationStatus = AssetDataRegistrationStatus.Unregistered;
        private Dictionary<string, PropertyUpdateData> cachedPropertyUpdates = new Dictionary<string, PropertyUpdateData>();

        #endregion

        #region IAssetMediator Implementation

        public GameObject AttachedGameObject => gameObject;

        public string AssetTypeId => assetTypeId;

        public string PresetAssetId => presetAssetId;

        public Guid AssetId => assetId;

        public T GetAssetTypeComponent<T>() where T : AssetTypeComponent
        {
            // TODO Cache results
            return (T)GetComponent(typeof(T));
        }

        /// <summary>
        /// Map asset data, properties, events, states, methods by name for the given asset type component.
        /// Called by the BaseAssetTypeComponent downstream of RegisterAssetDataAsync
        /// </summary>
        /// <param name="assetTypeComponent"></param>
        /// <param name="assetComponentData"></param>
        /// <returns></returns>
        public async UniTask AddAssetData(IAssetTypeComponent assetTypeComponent, BaseAssetData assetComponentData)
        {
            if (assetDataByTypeName.ContainsKey(assetTypeComponent.GetType().Name))
            {
                assetDataByTypeName[assetTypeComponent.GetType().Name] =
                    new AssetDataHolder(assetTypeComponent, assetComponentData);
            }
            else
            {
                assetDataByTypeName.Add
                    (assetTypeComponent.GetType().Name, new AssetDataHolder(assetTypeComponent, assetComponentData));
            }

            if (!knownAssetTypeComponents.Contains(assetTypeComponent))
            {
                knownAssetTypeComponents.Add(assetTypeComponent);
            }

            await UniTask.WaitUntil(() => assetComponentData.IsSetup);

            // All of these mappings do not use any Main Thread resources, so push it off to another thread so it does not affect the frame rate
            await UniTask.Create
            (
                () =>
                {
                    // Build up the mapping of known asset properties to their respective interface and data holding classes
                    foreach (var assetPropertyDefinition in assetComponentData.AllAssetPropertyDefinitions)
                    {
                        if (knownAssetPropertiesMappings.ContainsKey(assetPropertyDefinition.Key))
                        {
                            // Check to see if the same property was added from the same Asset Type Component, this can occur when the component is enabled/disabled
                            if (knownAssetPropertiesMappings[assetPropertyDefinition.Key].Item2 != assetTypeComponent)
                            {
                                Logger.Error
                                (
                                    $"[Asset Mediator] Duplicate Asset Property Key Found: {assetPropertyDefinition.Key}",
                                    "AssetMediator"
                                );
                            }
                        }
                        else
                        {
                            knownAssetPropertiesMappings.Add
                            (
                                assetPropertyDefinition.Key,
                                (assetPropertyDefinition.Value, assetTypeComponent, assetComponentData)
                            );
                        }
                    }

                    // Build up the mapping of every state names per asset type component so that it can easily be called
                    foreach (var assetState in assetTypeComponent.GetAllStateNames())
                    {
                        if (knownAssetStateProperties.ContainsKey(assetState))
                        {
                            knownAssetStateProperties[assetState].Add(assetTypeComponent);
                        }
                        else
                        {
                            knownAssetStateProperties.Add(assetState, new HashSet<IAssetTypeComponent>() { assetTypeComponent });
                        }
                    }

                    // Build up the mapping of every method name per asset type component so that it can easily be called
                    foreach (var assetMethod in assetTypeComponent.GetAllMethodNames())
                    {
                        if (knownAssetMethods.ContainsKey(assetMethod))
                        {
                            knownAssetMethods[assetMethod].Add(assetTypeComponent);
                        }
                        else
                        {
                            knownAssetMethods.Add(assetMethod, new HashSet<IAssetTypeComponent>() { assetTypeComponent });
                        }
                    }

                    // Build up the mapping of every event per asset type component so they can easily be called
                    foreach (var eventName in assetTypeComponent.GetAllEventNames())
                    {
                        if (knownAssetEvents.ContainsKey(eventName))
                        {
                            knownAssetEvents[eventName].Add(assetTypeComponent);
                        }
                        else
                        {
                            knownAssetEvents.Add(eventName, new HashSet<IAssetTypeComponent>() { assetTypeComponent });
                        }
                    }

                    return UniTask.CompletedTask;
                }
            );
        }

        private UniTask AddLocalData(LocalAssetTypeComponent localAssetTypeComponent)
        {
            if (!knownLocalAssetTypeComponent.Contains(localAssetTypeComponent))
            {
                knownLocalAssetTypeComponent.Add(localAssetTypeComponent);
            }

            return UniTask.CompletedTask;
        }

        enum AssetDataRegistrationStatus
        {
            Unregistered,
            InProgress,
            Registered
        }

        /// <summary>
        /// Initialize maps for data holders, properties, and events for all attached
        /// ATCs. See <c>AddAssetData</c>.
        /// </summary>
        private async void RegisterAssetDataAsync()
        {
            // If data has already been registered (or this process has already started), do nothing
            if (assetDataRegistrationStatus != AssetDataRegistrationStatus.Unregistered)
                return;

            assetDataRegistrationStatus = AssetDataRegistrationStatus.InProgress;

            // go through all local asset type components and map their data.
            LocalAssetTypeComponent[] localAssetTypeComponents = GetComponents<LocalAssetTypeComponent>();

            List<UniTask> localAssetDataTasks = new List<UniTask>();
            for (int i = 0; i < localAssetTypeComponents.Length; i++)
            {
                LocalAssetTypeComponent localAssetTypeComponent = localAssetTypeComponents[i];
                localAssetDataTasks.Add(AddLocalData(localAssetTypeComponent));
            }

            await UniTask.WhenAll(localAssetDataTasks);

            IAssetTypeComponent[] assetTypeComponents = GetComponents<IAssetTypeComponent>();

            // go through all attached asset type components and map their data.
            List<UniTask> assetDataTasks = new List<UniTask>();
            for (int i = 0; i < assetTypeComponents.Length; i++)
            {
                IAssetTypeComponent assetTypeComponent = assetTypeComponents[i];
                assetDataTasks.Add(assetTypeComponent.SendAssetData(this));
            }

            await UniTask.WhenAll(assetDataTasks);

            // when finished, apply all property updates which were received before they could be applied
            ApplyCachedPropertyUpdates();

            assetDataRegistrationStatus = AssetDataRegistrationStatus.Registered;
        }

        public void RegisterWithAssetEvent<L>(L listener, string eventName, EventHandler eventHandler)
        {
            if (knownAssetEvents.ContainsKey(eventName))
            {
                foreach (var assetTypeComponent in knownAssetEvents[eventName])
                {
                    assetTypeComponent.RegisterEvent(listener, eventName, eventHandler);
                }
            }
            else
            {
                Logger.Warning
                    ($"Asset {name} does not have any asset type components with the event: {eventName}", "AssetMediator");
            }
        }

        public void UnregisterWithAssetEvent(string eventName, EventHandler eventHandler)
        {
            if (!knownAssetEvents.ContainsKey(eventName))
            {
                return;
            }

            foreach (var assetTypeComponent in knownAssetEvents[eventName])
            {
                assetTypeComponent.UnregisterEvent(eventName, eventHandler);
            }
        }

        public void RegisterPropertyChange(string assetPropertyName, Action<AssetPropertyChangeEventArgs> lambda)
        {
            if (!propertyChangeActions.ContainsKey(assetPropertyName))
            {
                propertyChangeActions.Add(assetPropertyName, new List<Action<AssetPropertyChangeEventArgs>>() { lambda });
            }
            else
            {
                propertyChangeActions[assetPropertyName].Add(lambda);
            }
        }

        /// <summary>
        /// Unregister all property change handlers for a specified property.
        /// </summary>
        /// <param name="assetPropertyName"></param>
        public void UnregisterPropertyChange(string assetPropertyName)
        {
            if (propertyChangeActions.ContainsKey(assetPropertyName))
            {
                propertyChangeActions.Remove(assetPropertyName);
            }
            else
            {
                Logger.Warning
                (
                    $"Trying to remove property name {assetPropertyName} that does not exist in the Property/Action Dictionary.",
                    "AssetMediator"
                );
            }
        }

        /// <summary>
        /// Unregister a single property change handler.
        /// </summary>
        /// <param name="assetPropertyName"></param>
        /// <param name="lambda"></param>
        public void UnregisterPropertyChange(string assetPropertyName, Action<AssetPropertyChangeEventArgs> lambda)
        {
            if (propertyChangeActions.TryGetValue(assetPropertyName, out var lambdas))
            {
                if (lambdas.Remove(lambda))
                {
                    if (lambdas.Count == 0)
                    {
                        propertyChangeActions.Remove(assetPropertyName);
                    }
                }
                else
                {
                    Logger.Warning
                    (
                        $"Trying to remove property change handler for {assetPropertyName} that does not exist in the list of handlers.",
                        "AssetMediator"
                    );
                }
            }
            else
            {
                Logger.Warning
                (
                    $"Trying to remove property change handler for {assetPropertyName} that does not exist in the Property/Action Dictionary.",
                    "AssetMediator"
                );
            }
        }

        public void RegisterPropertyValidator(string assetPropertyName, Func<object, (object, bool)> validator)
        {
            if (knownAssetPropertiesMappings.TryGetValue(assetPropertyName, out var propertyDefinition))
            {
                propertyDefinition.Item1.RegisterValidator(validator);
            }
            else
            {
                Logger.Warning($"Trying to register validator for unregistered property {assetPropertyName}", "AssetMediator");
            }
        }

        public void UnregisterPropertyValidator(string assetPropertyName, Func<object, (object, bool)> validator)
        {
            if (knownAssetPropertiesMappings.TryGetValue(assetPropertyName, out var propertyDefinition))
            {
                propertyDefinition.Item1.UnregisterValidator(validator);
            }
            else
            {
                Logger.Warning($"Trying to register validator for unregistered property {assetPropertyName}", "AssetMediator");
            }
        }

        public IAssetTypeComponent GetAssetTypeComponent(string propertyName)
        {
            if (assetDataByTypeName.ContainsKey(propertyName))
            {
                return assetDataByTypeName[propertyName].attachedAssetTypeComponent;
            }
            else
            {
                Logger.Warning($"AssetMediator {name} does not have the property name {propertyName}.", "AssetMediator");
                return null;
            }
        }

        public T GetAssetPropertyDefinition<T>(string propertyName)
        {
            if (knownAssetPropertiesMappings.ContainsKey(propertyName))
            {
                return (T)knownAssetPropertiesMappings[propertyName].Item1;
            }

            return default;
        }

        public T GetAssetData<T>(string propertyName) where T : BaseAssetData
        {
            if (knownAssetPropertiesMappings.ContainsKey(propertyName))
            {
                return (T)knownAssetPropertiesMappings[propertyName].Item3;
            }

            return default;
        }

        public (T, IAssetTypeComponent)[] GetAssetState<T>(string stateName)
        {
            if (knownAssetStateProperties.ContainsKey(stateName))
            {
                List<(T, IAssetTypeComponent)> allResultsList = new List<(T, IAssetTypeComponent)>();

                foreach (var assetTypeComponent in knownAssetStateProperties[stateName])
                {
                    allResultsList.Add((assetTypeComponent.ReturnState<T>(stateName), assetTypeComponent));
                }

                return allResultsList.ToArray();
            }

            return null;
        }

        public void ExecuteCommand(BaseAssetCommand command)
        {
            Logger.Info($"Executing {command.GetType()}:{command}", "AssetMediator", true, gameObject);

            command.SetAsset(this);

            command.Execute();
        }

        public void CallAssetMethod(string methodName, object[] parameters = null)
        {
            if (knownAssetMethods.ContainsKey(methodName))
            {
                foreach (var assetTypeComponent in knownAssetMethods[methodName])
                {
                    assetTypeComponent.CallMethod(methodName, parameters);
                }
            }
            else
            {
                // TODO probably remove
                Logger.Warning
                (
                    $"Asset {name} does not have any asset type components with the method: {methodName} and {parameters?.Length} parameters",
                    "AssetMediator"
                );
            }
        }

        public (T, IAssetTypeComponent)[] CallAssetMethod<T>(string methodName, object[] parameters)
        {
            return !knownAssetMethods.ContainsKey(methodName)
                ? null
                : knownAssetMethods[methodName]
                    .Select(assetTypeComponent => (assetTypeComponent.CallMethod<T>(methodName, parameters), assetTypeComponent))
                    .ToArray();
        }

        public string SerializeToJson()
        {
            // Serialize the version number and the dictionary of asset data types
            return JsonConvert.SerializeObject(new SerializedAssetData(version, assetDataByTypeName), Formatting.None);
        }

        public string SerializeAssetTypeComponent(string assetTypeComponentName, string assetProperty = null, Formatting? formatting = null)
        {
            if(assetDataByTypeName.ContainsKey(assetTypeComponentName))
            {
                var format = formatting ?? Formatting.None;

                // Return string will all properties if none is specified
                if (string.IsNullOrEmpty(assetProperty))
                {
                    return JsonConvert.SerializeObject(assetDataByTypeName[assetTypeComponentName], format);
                }
                else
                {
                    return assetDataByTypeName[assetTypeComponentName].assetData.SerializeAssetPropertyData(assetProperty, format);
                }
            }
            else
            {
                Debug.LogWarning($"[AssetMediator] {name} does not have the Asset Type Component {assetTypeComponentName}.");

                return null;
            }
        }

        public void SetAllInitialValues()
        {
            foreach (var assetDataHolder in assetDataByTypeName.Values)
            {
                assetDataHolder.assetData.SetupAllInitialValues();
            }
        }

        public async UniTask DeserializeFromJson(string json, IAssetTypeComponent[] allAssetTypeComponents)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                SetAllInitialValues();

                return;
            }

            // TODO: More JSON validation

            SerializedAssetData serializedAsset;

            try
            {
                // This converts non int version values to an int. This can probably be removed later.
                var jObject = JObject.Parse(json);

                var stringVersion = jObject.Value<string>(nameof(SerializedAssetData.version));

                if (!int.TryParse(stringVersion, out int intVersion))
                {
                    Logger.Debug($"Unable to deserialize provided version \"{stringVersion}\", resetting to 0.", "AssetMediator");
                }

                jObject[nameof(SerializedAssetData.version)] = intVersion;

                serializedAsset = jObject.ToObject<SerializedAssetData>(DefaultNewtonsoftJsonConfiguration.JsonSerializer);

                if (serializedAsset == null)
                {
                    Logger.Error("Cannot deserialize AssetMediator JSON!", "AssetMediator");
                    return;
                }
            }
            catch (Exception e)
            {
                Logger.Error("Could not deserialize JSON to Object", "AssetMediator", e);

                var tempAsset = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(json);

                serializedAsset = new SerializedAssetData(tempAsset);
            }

            // TODO: Tie in version number serializedAsset.version when necessary
            version = serializedAsset.version;

            List<UniTask> loadAssetTypeTasks = new List<UniTask>();

            foreach (string assetTypeName in assetDataByTypeName.Keys)
            {
                loadAssetTypeTasks.Add
                (
                    UniTask.RunOnThreadPool
                    (
                        async () =>
                        {
                            var assetDataHolder = assetDataByTypeName[assetTypeName];

                            if (!serializedAsset.assetDataByTypeName.TryGetValue(assetTypeName, out var runtimeData))
                            {
                                Logger.Warning
                                    ($"No runtime data found for Asset Type Component {assetTypeName}.", "AssetMediator");

                                assetDataHolder.assetData.SetupAllInitialValues();

                                assetDataHolder.attachedAssetTypeComponent.OnMount();

                                // No runtime data found for this field.
                                return;
                            }

                            if (!runtimeData.TryGetValue(nameof(AssetDataHolder.assetData), out var assetDataJToken))
                            {
                                Logger.Warning
                                (
                                    $"Cannot find {nameof(AssetDataHolder.assetData)} under {assetTypeName}. Please check the JSON payload.",
                                    "AssetMediator"
                                );

                                assetDataHolder.assetData.SetupAllInitialValues();

                                assetDataHolder.attachedAssetTypeComponent.OnMount();

                                // Cannot find asset data
                                return;
                            }

                            // We need to iterate through all the properties, so take the JToken and make it into a JObject which functionally acts as a dictionary for us
                            var assetDataJObject = assetDataJToken.ToObject<JObject>
                                (DefaultNewtonsoftJsonConfiguration.JsonSerializer);

                            List<UniTask> loadAssetPropertyTasks = new List<UniTask>();

                            // Iterate through all asset property names on the current asset data, if there are additional asset data that is in the payload, it will be ignored,
                            // only the current properties of this class will matter
                            foreach (var atc in allAssetTypeComponents)
                            {
                                foreach (string propertyName in assetDataHolder.assetData.GetAllAssetPropertyNamesEditor(atc))
                                {
                                    loadAssetPropertyTasks.Add
                                    (
                                        UniTask.RunOnThreadPool
                                        (
                                            async () =>
                                            {
                                                JToken assetData = null;
                                                if (assetDataJObject != null && !assetDataJObject.TryGetValue
                                                        (propertyName, out assetData))
                                                {
                                                    Logger.Warning
                                                    (
                                                        $"Could not find {propertyName} in {assetTypeName}'s Asset Data JSON payload.",
                                                        "AssetMediator"
                                                    );

                                                    assetDataHolder.assetData.SetupInitialValue(propertyName);

                                                    return;
                                                }

                                                if (assetData == null) return;

                                                await assetDataHolder.assetData.LoadAssetProperty(propertyName, assetData);
                                            }
                                        )
                                    );
                                }
                            }

                            await UniTask.WhenAll(loadAssetPropertyTasks);

                            // Now that the data has loaded, inform the component
                            assetDataHolder.attachedAssetTypeComponent.OnMount();
                        }
                    )
                );
            }

            await UniTask.WhenAll(loadAssetTypeTasks);

            // OnAssetMounted calls Setup, which often does main thread stuffs!
            await UniTask.SwitchToMainThread();

            OnAssetMounted();
        }

        private class SerializedAssetData
        {
            [JsonConstructor]
            public SerializedAssetData(int v, Dictionary<string, AssetDataHolder> assetTypes)
            {
                version = v;

                // Set the dictionary to ignore case with a StringComparer since the JSON serializer will change the case of classes
                assetDataByTypeName = new Dictionary<string, JObject>(StringComparer.OrdinalIgnoreCase);

                if (assetTypes == null)
                {
                    return;
                }

                // Convert the Dictionary for JSON serialization
                foreach (var t in assetTypes)
                {
                    assetDataByTypeName.Add(t.Key, JObject.FromObject(t.Value));
                }
            }

            public SerializedAssetData(Dictionary<string, JObject> assetTypes)
            {
                version = 0;
                assetDataByTypeName = assetTypes;
            }

            // We want our dictionary keys to be case insensitive, but the deserialization process causes the dictionary's comparer
            // to be reset, so make sure it is set after deserialization happens
            // http://markusgreuel.net/blog/loosing-the-comparer-when-de-serializing-a-dictionary-with-the-datacontractserializer
            [OnDeserialized]
            private void OnDeserialized(StreamingContext context)
            {
                assetDataByTypeName = new Dictionary<string, JObject>(assetDataByTypeName, StringComparer.OrdinalIgnoreCase);
            }

            public int version;
            public Dictionary<string, JObject> assetDataByTypeName;
        }

        public void SetAssetProperty(string propertyName, object newValue)
        {
            // apply the update if the asset is prepared to do so.
            // otherwise, cache it for application later
            if (AssetDataRegistrationComplete)
            {
                ApplyAssetPropertyUpdate(propertyName, newValue);
            }
            else
            {
                CacheAssetPropertyUpdate(propertyName, newValue);
            }
        }

        public void SetAssetProperty(string propertyName, byte[] newValue)
        {
            // apply the update if the asset is prepared to do so.
            // otherwise, cache it for application later
            if (AssetDataRegistrationComplete)
            {
                ApplyAssetPropertyUpdate(propertyName, newValue);
            }
            else
            {
                CacheAssetPropertyUpdate(propertyName, newValue);
            }
        }

        private void ApplyAssetPropertyUpdate(string propertyName, object newValue)
        {
            if (knownAssetPropertiesMappings.ContainsKey(propertyName))
            {
                knownAssetPropertiesMappings[propertyName].Item2.SetPropertyValue(propertyName, newValue);
            }
            else
            {
                // TODO probably remove this
                Logger.Error($"Unknown asset property {propertyName} on Asset {name}", "AssetMediator");
            }
        }

        private void ApplyAssetPropertyUpdate(string propertyName, byte[] newValue)
        {
            if (knownAssetPropertiesMappings.ContainsKey(propertyName))
            {
                knownAssetPropertiesMappings[propertyName].Item2.SetPropertyValue(propertyName, newValue);
            }
            else
            {
                Logger.Error($"Unknown asset property {propertyName} on Asset {name}", "AssetMediator");
            }
        }

        private void ApplyAssetPropertyUpdate(string propertyName, PropertyUpdateData cachedUpdate)
        {
            if (cachedUpdate.Type == typeof(object))
            {
                ApplyAssetPropertyUpdate(propertyName, cachedUpdate.GetData<object>());
            }
            else if (cachedUpdate.Type == typeof(byte[]))
            {
                ApplyAssetPropertyUpdate(propertyName, cachedUpdate.GetData<byte[]>());
            }
            else
            {
                Logger.Error($"Unsupported type {cachedUpdate.Type} in asset property update.", "AssetMediator");
            }
        }

        /// <summary>
        /// Apply all cached property updates; updates which were made too early to be applied were stored here.
        /// </summary>
        private void ApplyCachedPropertyUpdates()
        {
            while (cachedPropertyUpdates.Count > 0)
            {
                Dictionary<string, PropertyUpdateData> cache = cachedPropertyUpdates;
                cachedPropertyUpdates = new Dictionary<string, PropertyUpdateData>();

                foreach (KeyValuePair<string, PropertyUpdateData> cachedUpdate in cache)
                {
                    ApplyAssetPropertyUpdate(cachedUpdate.Key, cachedUpdate.Value);
                }
            }
        }

        /// <summary>
        /// Store an asset property update which was set before the asset was ready to apply updates.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="newValue"></param>
        private void CacheAssetPropertyUpdate(string propertyName, object newValue)
        {
            if (cachedPropertyUpdates.ContainsKey(propertyName))
            {
                cachedPropertyUpdates[propertyName] = PropertyUpdateData.Create<object>(newValue);
            }
            else
            {
                cachedPropertyUpdates.Add(propertyName, PropertyUpdateData.Create<object>(newValue));
            }
        }

        /// <summary>
        /// Store an asset property update which was set before the asset was ready to apply updates.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="newValue"></param>
        private void CacheAssetPropertyUpdate(string propertyName, byte[] newValue)
        {
            if (cachedPropertyUpdates.ContainsKey(propertyName))
            {
                cachedPropertyUpdates[propertyName] = PropertyUpdateData.Create<byte[]>(newValue);
            }
            else
            {
                cachedPropertyUpdates.Add(propertyName, PropertyUpdateData.Create<byte[]>(newValue));
            }
        }

        public object GetAssetProperty(string propertyName)
        {
            return knownAssetPropertiesMappings.ContainsKey
                (propertyName)
                ? knownAssetPropertiesMappings[propertyName].Item2.GetPropertyValue(propertyName)
                : null;
        }

        public byte[] GetAssetPropertyByteArray(string propertyName)
        {
            if (knownAssetPropertiesMappings.ContainsKey(propertyName))
            {
                return knownAssetPropertiesMappings[propertyName].Item2.GetPropertyValueByteArray(propertyName);
            }

            return null;
        }

        public void SetStage(Guid stageId)
        {
            foreach (AssetDataHolder assetDataHolder in assetDataByTypeName.Values)
            {
                assetDataHolder.attachedAssetTypeComponent.SetStage(stageId);
            }
        }

        public void SetRuntimeID(Guid id, string newAssetId)
        {
            assetId = id;
            this.presetAssetId = newAssetId;
        }

        public void SyncRuntimeRoomData()
        {
            foreach (var currentSyncable in knownSyncables)
            {
                currentSyncable.Sync();
            }
        }

        public IUniTaskAsyncEnumerable<(MonoBehaviour, MethodInfo)> GetAllInjectableDependenciesAsync()
        {
            // writer(IAsyncWriter<T>) has `YieldAsync(value)` method.
            return UniTaskAsyncEnumerable.Create<(MonoBehaviour, MethodInfo)>
            (
                async (writer, token) =>
                {
                    foreach (var assetTypeComponent in knownAssetTypeComponents)
                    {
                        foreach (var injectableMethod in assetTypeComponent.GetInjectableDependencies())
                        {
                            await writer.YieldAsync(((MonoBehaviour)assetTypeComponent, injectableMethod));

                            await UniTask.Yield();
                        }
                    }

                    foreach (var localAsset in knownLocalAssetTypeComponent)
                    {
                        foreach(var method in localAsset.GetInjectableDependencies())
                        {
                            if (method == null)
                            {
                                continue;
                            }

                            await writer.YieldAsync((localAsset, method));

                            await UniTask.Yield();
                        }
                    }
                }
            );
        }

        public async void OnPropertyChanged<T>(BaseAssetTypeComponent<T> assetTypeComponent, AssetPropertyChangeEventArgs e)
            where T : BaseAssetData
        {
            // When an object is first instantiated, this will fire before the AssetID has been set, so delay until after then
            if (AssetId == Guid.Empty)
                return;

            // The AssetPropertyRuntime doesn't know the AssetID when this event is generated, so add it now
            e.SetAssetID(AssetId);

            // Due to the fact that will be invoked by reloading, we need to make sure the Asset Types downstream are on the main thread
            await UniTask.SwitchToMainThread();

            InvokeRegisteredPropertyChangeActions(e);

            PropertyChanged?.Invoke(assetTypeComponent, e);
        }

        private void InvokeRegisteredPropertyChangeActions(AssetPropertyChangeEventArgs e)
        {
            if (string.IsNullOrEmpty(e.AssetPropertyName))
            {
                return;
            }

            // If this is the initial StageId that is being set, do not trigger a property update
            if (!propertyChangeActions.ContainsKey(e.AssetPropertyName) ||
                e.PropertyName == nameof(AssetPropertyRuntimeData<int>.StageId))
            {
                return;
            }

            foreach (var currentAction in propertyChangeActions[e.AssetPropertyName].ToArray())
            {
                try
                {
                    currentAction.Invoke(e);
                }
                catch (Exception ex)
                {
                    Logger.Error
                    (
                        $"[AssetMediator] {name} Error when trying to apply action for {e.AssetPropertyName}:",
                        "AssetMediator",
                        ex
                    );
                }
            }
        }

        public void OnAssetMounted()
        {
            // Inform all AssetTypeComponents which manage their specific behaviors
            foreach (AssetDataHolder dataHolder in assetDataByTypeName.Values)
            {
                dataHolder.attachedAssetTypeComponent.OnAssetMount();
            }
        }

        public void OnAssetAwake()
        {
            // Inform all AssetTypeComponents which manage their specific behaviors
            foreach (AssetDataHolder dataHolder in assetDataByTypeName.Values)
            {
                dataHolder.attachedAssetTypeComponent.OnAwake();
            }
        }

        public IEnumerable<IAssetPropertyDefinition> GetAllKnownAssetProperties()
        {
            return knownAssetPropertiesMappings.Values.Select(tuple => tuple.Item1);
        }

        public IEnumerable<IAssetTypeComponent> GetAllKnownAssetTypes()
        {
            return knownAssetTypeComponents;
        }

        #endregion

        #region UnityFunctions

        private void OnEnable()
        {
            // Since local and network asset type components can be used, check all components that are attached to this mediator
            foreach (var currentComponent in GetComponents<Component>())
            {
                // Check to see if this Asset Type Component implements the IRuntimeSyncable interface
                var allInterface = currentComponent.GetType().GetInterfaces();

                if (!allInterface.Contains(typeof(IRuntimeSyncable)))
                {
                    continue;
                }

                knownSyncables.Add((IRuntimeSyncable)currentComponent);
            }

            RegisterAssetDataAsync();
        }

        #endregion
    }

    class PropertyUpdateData
    {
        public Type Type { get; }
        public object Data { get; }

        private PropertyUpdateData(object data, Type type)
        {
            Data = data;
            Type = type;
        }

        public static PropertyUpdateData Create<T>(object data)
        {
            return new PropertyUpdateData(data, typeof(T));
        }

        public T GetData<T>()
        {
            if (Type == typeof(T))
            {
                return (T)Data;
            }
            else
            {
                Logger.Error
                    ($"Incorrect type {typeof(T)} requested from {this.GetType().Name} with type {Type}", "AssetMediator");
                return default(T);
            }
        }
    }
}