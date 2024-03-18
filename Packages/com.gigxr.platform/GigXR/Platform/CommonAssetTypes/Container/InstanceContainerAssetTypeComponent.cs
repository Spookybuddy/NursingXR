using System;
using UnityEngine;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Scenarios.GigAssets.Data;

namespace GIGXR.Platform.CommonAssetTypes.Container
{
    /// <summary>
    /// Generic container for multiple instances of GameObjects which can be
    /// instantiated and initialized. Intended to dynamic instantiation of
    /// contained prefabs from scenario data (specified in the asset's runtime data).
    /// 
    /// All contained elements must be pre-specified in the asset's runtime data when
    /// the scenario is loaded. Adjustments will not be made to contained instances after
    /// Setup time. This does NOT allow for instantiation of assets after the scenario has
    /// started; it only allows for instantiation at Setup time.
    /// 
    /// Instantiation is done locally (<c>GameObject.Instantiate</c>, not <c>GigAssetManager.Instantiate</c>).
    /// Any data that needs to match across networked participants should be specified in
    /// <c>TInitializationArgs</c> and applied to the prefab instance in <c>TContained.InitializeInstance</c>.
    /// <c></c>
    /// </summary>
    /// <typeparam name="TAssetData"></typeparam>
    /// <typeparam name="TContained"></typeparam>
    /// <typeparam name="TInitializationArgs"></typeparam>
    public class
        InstanceContainerAssetTypeComponent<TAssetData, TContained, TInitializationArgs> : BaseAssetTypeComponent<TAssetData>
        where TContained : IInstantiable<TInitializationArgs> where TAssetData : InstanceContainerAssetData<TInitializationArgs>
    {
        [SerializeField]
        private GameObject instancePrefab;

        private Transform instanceRoot;

        public void Awake()
        {
            Debug.Assert
                (
                    instancePrefab.GetComponent<TContained>() != null,
                    $"{GetType()} managed instance prefab must have a {typeof(TContained)} instance on the root object."
                );
        }

        public override void SetEditorValues()
        {
        }

        protected override void Setup()
        {
            // create a shared parent for instances
            instanceRoot        = new GameObject("InstanceRoot").transform;
            instanceRoot.parent = transform.GetChild(0); // displayRoot is always child 0

            // displayRoot should never not have these settings, and if we want to check for that, it should be done in isEnabled or something -J
            //
            // paranoiac: parent has same transform as display root 
            instanceRoot.localPosition = Vector3.zero;
            instanceRoot.localRotation = Quaternion.identity;
            instanceRoot.localScale    = Vector3.one;

            // instantiate and initialize all contained instances
            foreach (TInitializationArgs args in assetData.instanceSpecifications.runtimeData.Value)
            {
                GameObject.Instantiate(instancePrefab, instanceRoot).GetComponent<TContained>().InitializeInstance(args);
            }
        }

        protected override void Teardown()
        {
        }
    }

    [Serializable]
    public class InstanceContainerAssetData<TInitializationArgs> : BaseAssetData
    {
        public AssetPropertyDefinition<TInitializationArgs[]> instanceSpecifications;
    }
}