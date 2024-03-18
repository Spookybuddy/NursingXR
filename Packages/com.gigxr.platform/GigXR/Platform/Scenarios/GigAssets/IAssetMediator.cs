using Cysharp.Threading.Tasks;
using GIGXR.Platform.Scenarios.GigAssets.Data;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GIGXR.Platform.Scenarios.GigAssets
{
    /// <summary>
    /// Helper class that provides the GIGXR Asset system a common way to query assets.
    /// </summary>
    public interface IAssetMediator
    {
        public event EventHandler<AssetPropertyChangeEventArgs> PropertyChanged;

        GameObject AttachedGameObject { get; }

        string AssetTypeId { get; }

        string PresetAssetId { get; }

        Guid AssetId { get; }

        void OnPropertyChanged<T>(BaseAssetTypeComponent<T> assetTypeComponent, AssetPropertyChangeEventArgs e) where T : BaseAssetData;

        void RegisterPropertyChange(string assetPropertyName, Action<AssetPropertyChangeEventArgs> lambda);

        void UnregisterPropertyChange(string assetPropertyName);

        void UnregisterPropertyChange(string assetPropertyName, Action<AssetPropertyChangeEventArgs> lambda);

        void RegisterPropertyValidator(string assetPropertyName, Func<object, (object, bool)> validator);

        void UnregisterPropertyValidator(string assetPropertyName, Func<object, (object, bool)> validator);

        UniTask AddAssetData(IAssetTypeComponent component, BaseAssetData assetComponentData);

        T GetAssetTypeComponent<T>() where T : AssetTypeComponent;

        bool AssetDataRegistrationComplete { get; }

        // TODO Improvement: Find better way than passing named string
        void RegisterWithAssetEvent<L>(L listener, string eventName, EventHandler eventHandler);

        void UnregisterWithAssetEvent(string eventName, EventHandler eventHandler);

        T GetAssetPropertyDefinition<T>(string propertyName);

        T GetAssetData<T>(string propertyName) where T : BaseAssetData;

        /// <summary>
        /// Returns all the states with the given name across all asset type components.
        /// </summary>
        /// <typeparam name="T">The return type of the state</typeparam>
        /// <param name="stateName">The name of the state of interest</param>
        /// <returns>Since multiple states may exist, a list is returned with the given state and the asset 
        /// type component interface holding that state</returns>
        (T, IAssetTypeComponent)[] GetAssetState<T>(string stateName);

        void CallAssetMethod(string methodName, object[] parameters);

        /// <summary>
        /// Fires a command that targets this Asset.
        /// </summary>
        /// <param name="command">A concrete implementation of a command</param>
        void ExecuteCommand(BaseAssetCommand command);

        /// <summary>
        /// Calls and returns the value from any method with the given name and parameters on any
        /// asset type component.
        /// </summary>
        /// <typeparam name="T">The return value of interest</typeparam>
        /// <param name="methodName">The name of the method</param>
        /// <param name="parameters">Any parameters for the method call</param>
        /// <returns>Since multiple methods may be called and give multiple results, a list is returned 
        /// with the given return value with the interface of the component that called the method</returns>
        (T, IAssetTypeComponent)[] CallAssetMethod<T>(string methodName, object[] parameters);

        UniTask DeserializeFromJson(string json, IAssetTypeComponent[] assetTypeComponents);

        string SerializeToJson();

        string SerializeAssetTypeComponent(string assetTypeComponentName, string assetProperty = null, Newtonsoft.Json.Formatting? formatting = null);

        void SetAssetProperty(string propertyName, object newValue);

        void SetAssetProperty(string propertyName, byte[] newValue);

        object GetAssetProperty(string propertyName);

        byte[] GetAssetPropertyByteArray(string propertyName);

        IAssetTypeComponent GetAssetTypeComponent(string propertyName);

        void SetStage(Guid stageId);

        void SetRuntimeID(Guid assetID, string presetAssetId);

        IUniTaskAsyncEnumerable<(MonoBehaviour, MethodInfo)> GetAllInjectableDependenciesAsync();

        /// <summary>
        /// Syncs all the asset type components data that is not synced over GMS, but over the Network layer (i.e. Photon)
        /// </summary>
        void SyncRuntimeRoomData();

        // Lifecycle Hook

        // Called after the AssetMediator's data has been loaded
        void OnAssetMounted();

        // Called when a scenario is started
        void OnAssetAwake();

        IEnumerable<string> AssetTypeDependencyIds { get; }

        // Used to get all registered properties. Should only be used for design-time convenience methods.
        IEnumerable<IAssetPropertyDefinition> GetAllKnownAssetProperties();

        IEnumerable<IAssetTypeComponent> GetAllKnownAssetTypes();
    }
}