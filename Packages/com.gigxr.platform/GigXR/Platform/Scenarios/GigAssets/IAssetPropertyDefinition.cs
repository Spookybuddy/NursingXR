namespace GIGXR.Platform.Scenarios.GigAssets
{
    using Validation;
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using Cysharp.Threading.Tasks;
    using Data;

    /// <summary>
    /// Interface for AssetPropertyDefintions that provides 
    /// </summary>
    public interface IAssetPropertyDefinition
    {
        string PropertyName { get; }

        /// <summary>
        /// Holds the Type of the data that is serialized for a given
        /// property. 
        /// </summary>
        Type SpecifiedType { get; }

        /// <summary>
        /// Holds the design time value that is set by a developer for the default value of the
        /// given Asset Property Definition.
        /// </summary>
        object DefaultValue { get; }

        /// <summary>
        /// Holds the reference to the ATC this property is 
        /// </summary>
        IAssetTypeComponent AttachedAssetTypeComponent { get; }

        /// <summary>
        /// Allows an AssetPropertyDefinition to setup any necessary connections.
        /// </summary>
        void SetupAssetPropertyOnRunTimeChanges(IAssetTypeComponent assetTypeComponent, string propertyName, Type type);
        
        void SetupAssetPropertyEditorChanges(IAssetTypeComponent assetTypeComponent, string propertyName, Type type);

        /// <summary>
        /// Allows an AssetPropertyDefinition to clean up any necessary connections it has made.
        /// </summary>
        void TearDownAssetPropertyOnRunTimeChanges();

        /// <summary>
        /// Passes the asset's name property to the AssetPropertyDefinition.
        /// </summary>
        /// <param name="assetProperty">The name of the AssetPropertyDefintion</param>
        void SetAssetPropertyDefinitionName(string assetProperty);

        /// <summary>
        /// Sets the AssetPropertyDefinition runtime value
        /// </summary>
        /// <param name="propertyName">The AssetPropertyDefinition's variable name (e.g. position)</param>
        /// <param name="newValue">The value the property should be set to as an object</param>
        void SetPropertyValue(string propertyName, object newValue);

        /// <summary>
        /// Sets the AssetPropertyDefinition runtime value
        /// </summary>
        /// <param name="propertyName">The AssetPropertyDefinition's variable name (e.g. position)</param>
        /// <param name="newValue">The value the property should be set to as an byte[]</param>
        void SetPropertyValue(string propertyName, byte[] newValue);

        /// <summary>
        /// Returns the associated runtime value of the AssetPropertyDefinition
        /// </summary>
        /// <returns>The runtime value of the AssetPropertyDefinition</returns>
        object GetRuntimePropertyValue();

        /// <summary>
        /// Returns the associated runtime value as a byte array that can be serialized by any system
        /// </summary>
        /// <returns></returns>
        byte[] GetRuntimePropertyValueByteArray();

        /// <summary>
        /// Returns the associated design value of the AssetPropertyDefinition
        /// </summary>
        /// <returns></returns>
        object GetDesignPropertyValue();

        void SetStageValue(Guid stageId);

        /// <summary>
        /// Sets the Runtime Shared Value to the value set in the default value specified 
        /// in the Design Time data.
        /// </summary>
        void SetSharedRuntimeValueToDefault();

        void AddStageValue(string stageId);

        void AddKnownStageValue(string stageId, RuntimeStageInput stageData);

        void RemoveStage(string stageId);

        void RemoveStage(int stageIndex);

        void SetupInitialValues();

        /// <summary>
        /// Register a validator, which will return an updated value and a bool specifying whether to apply the update.
        /// </summary>
        /// <param name="validator"></param>
        void RegisterValidator(Func<object, (object, bool)> lambda);

        /// <summary>
        /// Unregister a registered validator.
        /// </summary>
        /// <param name="validator"></param>
        void UnregisterValidator(Func<object, (object, bool)> lambda);

        UniTask UpdateInternalRuntimeData(JToken newData);

        int GetRuntimeStagePropertyCount();

        void ClearRuntimeStageDataValues();

        /// <summary>
        /// Control whether changes to stage values persist through stage changes.
        /// Default is for changes to persist. Call with "false" to cause stage values to reset on stage entry.
        /// </summary>
        void SetValuePersistance(bool persist);

        object GetPropertyValueAtStage(Guid stageId, bool returnInitialData = true);

        /// <summary>
        /// For editor-only use, to allow property updates to go through from editor-serialized changes via OnValidate.
        /// </summary>
        void ValidateForEditor();
    }
}