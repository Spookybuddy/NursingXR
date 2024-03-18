using Cysharp.Threading.Tasks;
using GIGXR.Platform.ScenarioBuilder.Data;
using GIGXR.Platform.Scenarios.GigAssets.Data;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using System;
using System.Collections.Generic;

namespace GIGXR.Platform.Scenarios.GigAssets
{
    /// <summary>
    /// Basic contract for an AssetTypeComponent
    /// </summary>
    public interface IAssetTypeComponent
    {
        public int Version { get; }

        /// <summary>
        /// Set true on runtime instances. Should be left false in ALL prefabs. Allows editor validate updates to work in PSB without modifying prefabs.
        /// </summary>
        bool IsRuntimeInstance { get; set; }

        IAssetMediator AttachedAssetMediator { get; }

        UniTask SendAssetData(IAssetMediator mediator);

        bool AssetDataRegistrationComplete { get; }

        List<IAssetPropertyDefinition> GetAllPropertyDefinition();

        List<string> GetAllPropertyDefinitionNames();

        void HandlePropertyChange<T>(AssetPropertyRuntimeData<T> sender, AssetPropertyChangeEventArgs e);

        string[] GetAllEventNames();

        string GetAssetTypeName();

        Type GetAssetType();

        void RegisterEvent<L>
        (
            L listener,
            string eventName,
            EventHandler eventHandler
        );

        void UnregisterEvent
        (
            string eventName,
            EventHandler eventHandler
        );

        string[] GetAllMethodNames();

        void CallMethod
        (
            string methodName,
            object[] parameters
        );

        T CallMethod<T>
        (
            string methodName,
            object[] parameters
        );

        string[] GetAllStateNames();

        object ReturnState
        (
            string stateName
        );

        T ReturnState<T>
        (
            string stateName
        );

        void SetPropertyValue
        (
            string propertyName,
            object newValue
        );

        void SetPropertyValue
        (
            string propertyName,
            byte[] newValue
        );

        object GetPropertyValue(string propertyName);

        byte[] GetPropertyValueByteArray(string propertyName);

        object GetPropertyValueAtStage(Guid stageId, string propertyName, bool returnInitialData = true);

        void SetStage(Guid stageId);

        void AddStageData(string stageId, string? assetPropertyName = null);

        void AddKnownStageData(string stageId, string assetProperty, RuntimeStageInput stageData);

        void RemoveStageData(string stageId);

        void RemoveStageData(int stageIndex);

        // Editor Helpers

        void SetEditorValues();

        string[] GetAssetPropertiesWithRuntimeStageValues();

        void ClearRuntimeStageDataValues();

        /// <summary>
        /// Sets all the associated asset property definition's runtime shared value
        /// to the design time value.
        /// </summary>
        void ResetAllAssetPropertyDefintionSharedValueToDefault();

        // Lifecycle Hooks

        /// <summary>
        /// True after the initial value is set, subscribe to the OnMounted event to find out when this occurs
        /// </summary>
        bool IsMounted { get; }

        event EventHandler OnMounted;
        event EventHandler OnAssetMounted;
        event EventHandler OnAllAssetsMounted;
        event EventHandler OnAwakened;

        // Added so components attached to ATs could trigger their own initialization steps
        event EventHandler OnAssetInitialized;

        /// <summary>
        /// Called after the asset type for this particular AssetTypeComponent is loaded
        /// </summary>
        void OnMount();

        /// <summary>
        /// Called after all asset data types on the AssetMediator's data has been loaded
        /// </summary>
        void OnAssetMount();

        /// <summary>
        /// Called when a scenario is started
        /// </summary>
        void OnAwake();
    }
}