namespace GIGXR.Platform.CommonAssetTypes.CommonComponents.ShapeshifterAssetTypeComponent
{
    using GIGXR.Platform.Utilities.SerializableDictionary.Example.Example;
    using Scenarios.GigAssets;
    using Scenarios.GigAssets.Data;
    using Scenarios.GigAssets.EventArgs;
    using System;
    using UnityEngine;
    using Object = UnityEngine.Object;

    // [Serializable]
    // public class EnumObjectDictionary<TEnum> : SerializableDictionary<TEnum, UnityEngine.Object>
    // {
    // }

    [Serializable]
    public class GenericShapeshifterAssetData<TModelTypes> : BaseAssetData
    {
        public AssetPropertyDefinition<TModelTypes> modelType;
    }

    public class GenericShapeshifterAssetTypeComponent<TAssetData, TModelTypes> : BaseAssetTypeComponent<TAssetData>
        where TAssetData : GenericShapeshifterAssetData<TModelTypes>
    {
        [SerializeField]
        protected EnumObjectDictionary<TModelTypes> modelMap;

        [SerializeField]
        [Header("If empty, new object is placed beneath DisplayRoot.")]
        protected Transform instanceRoot;

        private Object loadedModel;

        [RegisterPropertyChange(nameof(GenericShapeshifterAssetData<TModelTypes>.modelType))]
        private void HandleModelTypeChange(AssetPropertyChangeEventArgs e)
        {
            // Delete the old loaded model, if one exists
            if (loadedModel)
                Destroy(loadedModel);

            // Check if the model type is set to None.
            var modelType = assetData.modelType.runtimeData.Value;

            if (modelType.ToString() == "None")
                return;

            // See if dictionary item is mapped
            var model = modelMap[modelType];

            if (model)
            {
                loadedModel = AssetManager.Instantiate(new PrefabInstantiationArgs((GameObject)model, false, instanceRoot ? instanceRoot : transform.GetChild(0)));
            }

            OnModelChanged(loadedModel);
        }

        protected override void Setup()
        {
            // Set instanceRoot to DisplayRoot if it is empty
            if (!instanceRoot)
                instanceRoot = transform.GetChild(0);
        }

        protected override void Teardown()
        {
        }

        public override void SetEditorValues()
        {
        }

        public virtual void OnModelChanged(Object model)
        {
        }
    }
}