using Cysharp.Threading.Tasks;
using GIGXR.Platform.Scenarios.GigAssets.Data;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using GIGXR.Platform.ScenarioBuilder.Data;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GIGXR.Platform.Scenarios.GigAssets
{
    /// <summary>
    /// The base behavior for an asset type that provides networked, serialized asset properties.
    ///
    /// Usage:
    /// 1. Subclass BaseAssetData, e.g.,
    ///     <c>public class BlockAssetData : BaseAssetData {}</c>
    /// 2. Subclass this class, e.g,.
    ///    <c>public class BlockAssetTypeComponent : BaseAssetTypeComponent{BlockAssetData} {}</c>
    /// 3. Add this subclass to your prefab.
    /// </summary>
    /// <typeparam name="TBaseAssetData"></typeparam>
    [DisallowMultipleComponent]
    [SelectionBaseAttribute]
    public abstract class BaseAssetTypeComponent<TBaseAssetData> : AssetTypeComponent,
        IAssetTypeComponent where TBaseAssetData : BaseAssetData
    {
        /// <summary>
        /// The version of this Asset Type component.
        /// </summary>
        /// <remarks>
        /// This is not used at this time (2021/10/13) and is reserved for future use. Semantic versioning does not seem
        /// to be needed for this as we only need to track breaking changes so a simple int will do.
        ///
        /// This would be used by first deserializing the data to a special "version" subclass to extract the version,
        /// and then extract to the real type after you know what version to use.
        ///
        /// The flow might look something like this:
        ///
        /// 1. Deserialize this data to the special version subclass.
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
        [HideInInspector]
        [Min(0)]
        [SerializeField]
        [Tooltip("Unused at this time. Reserved for future use.")]
        protected int version;

        /// <summary>
        /// Set true on instances. Should be left false in ALL prefabs. Allows editor validate updates to work in PSB without modifying prefabs.
        /// </summary>
        private bool isRuntimeInstance;

        public bool IsRuntimeInstance
        {
            get => isRuntimeInstance;
            set => isRuntimeInstance = value;
        }

        [JsonIgnore]
        public TBaseAssetData AssetData => assetData;

        [SerializeField]
        protected TBaseAssetData assetData;

        protected IAssetMediator attachedInteractable;
        protected bool hasPositionATC;
        protected bool hasRotationATC;
        protected bool hasScaleATC;

        protected Dictionary<MethodInfo, List<Delegate>> subscribedDelegates
            = new Dictionary<MethodInfo, List<Delegate>>();

        protected Dictionary<string, Func<object, (object, bool)>> propertyValidatorDelegates
            = new Dictionary<string, Func<object, (object, bool)>>();

        public event EventHandler PropertyChanged;

        public event EventHandler OnMounted;
        public event EventHandler OnAssetMounted;
        public event EventHandler OnAllAssetsMounted;
        public event EventHandler OnAwakened;

        // Added so components attached to ATs could trigger their own initialization steps
        public event EventHandler OnAssetInitialized;

        public int Version => version;

        public bool IsMounted => _isMounted && AssetDataRegistrationComplete;

        private bool _isMounted = false;

        // Set to true after calling Setup, and false after tear down.
        // Prevents double initialization since OnAssetMounted is called more than once. 
        protected bool IsInitialized = false;

        public bool AssetDataRegistrationComplete
            => attachedInteractable?.AssetDataRegistrationComplete ?? false;

        #region UnityEvents

        protected virtual void OnEnable()
        {
            if (attachedInteractable == null)
            {
                attachedInteractable = GetComponent<IAssetMediator>();
                hasPositionATC = GetComponent<PositionAssetTypeComponent>() != null;
                hasRotationATC = GetComponent<RotationAssetTypeComponent>() != null;
                hasScaleATC = GetComponent<ScaleAssetTypeComponent>() != null;
            }

            assetData.Setup<TBaseAssetData>(this);

            RegisterAllPropertyChangeAttributes(type => RegisterAllPropertyValidationAttributeForType(type), attachedInteractable);
        }

        protected virtual void OnDisable()
        {
            UnregisterAllPropertyChangeAttributes();

            // unregister all property value validators
            foreach (var method in propertyValidatorDelegates)
            {
                UnregisterPropertyValidator(method.Key, method.Value);
            }

            propertyValidatorDelegates.Clear();

            if (IsInitialized)
            {
                Teardown();
                IsInitialized = false;
            }

            assetData.TearDown<TBaseAssetData>();
        }

#if UNITY_EDITOR

        protected virtual void OnValidate()
        {
            if (assetData != null && isRuntimeInstance)
            {
                assetData.ValidateForEditor();
            }
        }

        protected virtual void Reset()
        {
            if (assetData != null)
            {
                SetEditorValues();
            }
            else
            {
                // On the first reset, the asset data will be null so delay setting
                // the initial editor values until Unity has created it
                EditorApplication.update += OnEditorUpdate;
            }
        }

        protected virtual void OnEditorUpdate()
        {
            try
            {
                SetEditorValues();
            }
            catch
            {
                // Do nothing
            }

            if (assetData != null)
            {
                EditorApplication.update -= OnEditorUpdate;
            }
        }
#endif

        protected virtual void DelayedSetupAction()
        {
            assetData.Setup<TBaseAssetData>(this);
        }

        #endregion

        #region EventHandlers

        /// <summary>
        /// Register all methods in the specified type marked with the <c>RegisterPropertyValidatorAttribute</c> as property validators.
        /// </summary>
        /// <param name="type"></param>
        protected void RegisterAllPropertyValidationAttributeForType(Type type)
        {
            var methodAttributes = type
                                   .GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                   .Where(method => method.GetCustomAttribute<RegisterPropertyValidatorAttribute>() != null);

            if (methodAttributes != null && methodAttributes.Count() != 0)
            {
                foreach (var methodInfo in methodAttributes)
                {
                    var attribute = methodInfo.GetCustomAttribute<RegisterPropertyValidatorAttribute>();

                    Func<object, (object, bool)> func = (Func<object, (object, bool)>)Delegate.CreateDelegate(typeof(Func<object, (object, bool)>), this, methodInfo);
                    propertyValidatorDelegates.Add(attribute.PropertyName, func);
                    RegisterPropertyValidator(attribute.PropertyName, func);
                }
            }
        }

        /// <summary>
        /// Register a validator for the specified property.
        /// It will block new property updates at runtime if the validator returns false.
        /// </summary>
        /// <param name="validator"></param>
        /// <param name="propertyName"></param>
        protected void RegisterPropertyValidator
        (
            string assetPropertyName,
            Func<object, (object, bool)> lambda
        )
        {
            assetData.RegisterPropertyValidator(assetPropertyName, lambda);
        }

        /// <summary>
        /// Unregister a registered validator for the specified property.
        /// </summary>
        /// <param name="validator"></param>
        /// <param name="propertyName"></param>
        protected void UnregisterPropertyValidator
        (
            string assetPropertyName,
            Func<object, (object, bool)> lambda
        )
        {
            assetData.UnregisterPropertyValidator(assetPropertyName, lambda);
        }

        #endregion

        #region IAssetTypeComponentImplementation

        public string GetAssetTypeName()
        {
            return GetType().Name;
        }

        public Type GetAssetType()
        {
            return GetType();
        }

        public string[] GetAllEventNames()
        {
            return Array.ConvertAll
                (
                    GetType().GetEvents(BindingFlags.Public | BindingFlags.Instance),
                    assetEvent => assetEvent.Name
                );
        }

        public List<IAssetPropertyDefinition> GetAllPropertyDefinition()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return assetData.GetAllPropertyDefinition(this, typeof(TBaseAssetData));
            }
#endif
            return assetData.GetAllPropertyDefinition(this);
        }

        public List<string> GetAllPropertyDefinitionNames()
        {
#if UNITY_EDITOR
            if(!Application.isPlaying)
            {
                return assetData.GetAllAssetPropertyNamesEditor(this, typeof(TBaseAssetData));
            }
#endif
            return assetData.GetAllAssetPropertyNamesEditor(this);
        }

        public void RegisterEvent<L>
        (
            L listener,
            string eventName,
            EventHandler eventHandler
        )
        {
            EventInfo assetEvent = GetType().GetEvent(eventName);

            if (assetEvent != null &&
                eventHandler != null)
            {
                var _handler = Delegate.CreateDelegate
                    (
                        assetEvent.EventHandlerType,
                        listener,
                        eventHandler.Method
                    );

                if (subscribedDelegates.ContainsKey(eventHandler.Method))
                {
                    subscribedDelegates[eventHandler.Method].Add(_handler);
                }
                else
                {
                    subscribedDelegates.Add(eventHandler.Method, new List<Delegate>() { _handler });
                }

                assetEvent.AddEventHandler(this, _handler);
            }
        }

        public void UnregisterEvent
        (
            string eventName,
            EventHandler eventHandler
        )
        {
            EventInfo assetEvent = GetType().GetEvent(eventName);

            if (assetEvent != null &&
                subscribedDelegates.ContainsKey(eventHandler.Method))
            {
                if (subscribedDelegates[eventHandler.Method].Contains(eventHandler))
                {
                    assetEvent.RemoveEventHandler(this, eventHandler);

                    subscribedDelegates[eventHandler.Method].Remove(eventHandler);
                }
            }
        }

        public string[] GetAllMethodNames()
        {
            return Array.ConvertAll
                (
                    GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance),
                    method => method.Name
                );
        }

        public void CallMethod
        (
            string methodName,
            object[] parameters
        )
        {
            CallMethod<object>(methodName, parameters);
        }

        public T CallMethod<T>
        (
            string methodName,
            object[] parameters
        )
        {
            MethodInfo method = GetType().GetMethod(methodName);

            if (method != null)
            {
                if (parameters != null)
                {
                    // Hacks. Handle nested JObjects and strings from deserialization.
                    ParameterInfo[] paramInfo = method.GetParameters();

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        Type argType = paramInfo[i].ParameterType;
                        Type providedType = parameters[i].GetType();

                        if (providedType == typeof(JObject))
                        {
                            parameters[i] = ((JObject)parameters[i]).ToObject(argType);
                        }
                        else if (providedType == typeof(string) &&
                                 argType != providedType)
                        {
                            parameters[i] = JsonConvert.DeserializeObject
                                ((string)parameters[i], argType);
                        }
                    }
                }

                return (T)method.Invoke(this, parameters);
            }

            return default;
        }

        public string[] GetAllStateNames()
        {
            return Array.ConvertAll
                (
                    GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance),
                    property => property.Name
                );
        }

        public object ReturnState
        (
            string stateName
        )
        {
            var property = GetType().GetProperty(stateName);

            if (property != null)
            {
                return property.GetValue(this);
            }
            else
            {
                return default;
            }
        }

        public T ReturnState<T>
        (
            string stateName
        )
        {
            var property = GetType().GetProperty(stateName);

            if (property != null)
            {
                return (T)property.GetValue(this);
            }
            else
            {
                return default;
            }
        }

        public void SetPropertyValue
        (
            string propertyName,
            object newValue
        )
        {
            assetData.SetAssetProperty(propertyName, newValue);
        }

        public void SetPropertyValue
        (
            string propertyName,
            byte[] newValue
        )
        {
            assetData.SetAssetProperty(propertyName, newValue);
        }

        public object GetPropertyValue
        (
            string propertyName
        )
        {
            return assetData.GetAssetProperty(propertyName);
        }

        public byte[] GetPropertyValueByteArray
        (
            string propertyName
        )
        {
            return assetData.GetAssetPropertyByteArray(propertyName);
        }

        public object GetPropertyValueAtStage(Guid stageId, string propertyName, bool returnInitialData = true)
        {
            return assetData.GetPropertyValueAtStage(stageId, propertyName, returnInitialData);
        }

        public void SetStage(Guid stageId)
        {
            assetData.SetStage(stageId);
        }

        public void ResetAllAssetPropertyDefintionSharedValueToDefault()
        {
            assetData.SetupEditor<TBaseAssetData>(this);

            assetData.ResetAllAssetPropertyDefintionSharedValueToDefault();
        }

        public void AddStageData(string stageId, string? assetProperty = null)
        {
            // Currently we are assuming this add stage is only used by the Editor
            assetData.SetupEditor<TBaseAssetData>(this);

            assetData.AddStage(stageId, assetProperty);
        }

        public void AddKnownStageData(string stageId, string? assetProperty, RuntimeStageInput stageData)
        {
            // Currently we are assuming this add stage is only used by the Editor
            assetData.SetupEditor<TBaseAssetData>(this);

            assetData.AddKnownStage(stageId, assetProperty, stageData);
        }

        public void RemoveStageData(string stageId)
        {
            // Currently we are assuming this remove stage is only used by the Editor
            assetData.SetupEditor<TBaseAssetData>(this);

            assetData.RemoveStage(stageId);
        }

        public void RemoveStageData(int stageIndex)
        {
            // Currently we are assuming this remove stage is only used by the Editor
            assetData.SetupEditor<TBaseAssetData>(this);

            assetData.RemoveStage(stageIndex);
        }

        public string[] GetAssetPropertiesWithRuntimeStageValues()
        {
            if(assetData != null)
            {
                return assetData.GetAssetPropertiesWithRuntimeStageValues();
            }
            else
            {
                Debug.LogError($"Asset Data is null when checking runtime values");
            }

            return null;
        }

        public void ClearRuntimeStageDataValues()
        {
            assetData.ClearRuntimeStageDataValues();
        }

        /// <summary>
        /// Called only once after asset is mounted. Use this to initialize the asset type.
        /// </summary>
        protected abstract void Setup();

        /// <summary>
        /// Called when asset is destroyed. Use this to de-initialize the asset type.
        /// </summary>
        protected abstract void Teardown();

        // The concrete implementation must set it's own default values for the Editor
        public abstract void SetEditorValues();

        public async UniTask SendAssetData
        (
            IAssetMediator assetMediator
        )
        {
            await assetMediator.AddAssetData(this, assetData);
        }

        // Forwards a property change through the asset mediator's property change event
        public void HandlePropertyChange<T>
        (
            AssetPropertyRuntimeData<T> sender,
            AssetPropertyChangeEventArgs e
        )
        {
            attachedInteractable?.OnPropertyChanged(this, e);

            PropertyChanged?.Invoke(this, e);
        }

        // Lifecycle hooks

        public void OnMount()
        {
            _isMounted = true;

            OnMounted?.Invoke(this, System.EventArgs.Empty);
        }

        public void OnAssetMount()
        {
            OnAssetMounted?.Invoke(this, System.EventArgs.Empty);

            if (!IsInitialized)
            {
                try
                {
                    Setup();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    Debug.LogWarning($"[BaseAssetTypeComponent] {name} had an exception that occurred while calling the Setup function. Continuing on with scenario...");

                    return;
                }

                IsInitialized = true;
                OnAssetInitialized?.Invoke(this, System.EventArgs.Empty);
            }
        }

        public void OnAwake()
        {
            OnAwakened?.Invoke(this, System.EventArgs.Empty);
        }

        #endregion
    }
}
