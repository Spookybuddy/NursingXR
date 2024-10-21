using GIGXR.Platform.CommonAssetTypes;
using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AutopsyBodyPartAssetTypeComponent : BaseAssetTypeComponent<AutopsyBodyPartAssetData>
{
    private Rigidbody rb;
    private Collider hitBox;
    private GameObject bodyPartObject;
    private AutopsyScaleAssetTypeComponent autopsyScaleAssetTypeComponent;
    private PositionAssetTypeComponent positionAssetTypeComponent;
    private RotationAssetTypeComponent rotationAssetTypeComponent;
    private ManipulationAssetTypeComponent manipulationAssetTypeComponent;
    private bool onScaleButInactive = false;

    private IScenarioManager scenarioManager;

    #region Dependencies

    [InjectDependencies]
    public void InjectDependencies(IScenarioManager injectedScenarioManager)
    {
        scenarioManager = injectedScenarioManager;
    }

    #endregion

    #region BaseAssetTypeComponent overrides

    public override void SetEditorValues()
    {
        
    }

    protected override void Setup()
    {
        manipulationAssetTypeComponent = GetComponent<ManipulationAssetTypeComponent>();
        rb = GetComponentInChildren<Rigidbody>(true);
        hitBox = GetComponentInChildren<Collider>(true);
        bodyPartObject = rb.gameObject;
        manipulationAssetTypeComponent.OnManipulationStarted.AddListener(OnSelection);
        manipulationAssetTypeComponent.OnManipulationEnded.AddListener(OnRelease);
    }

    protected override void Teardown()
    {
        manipulationAssetTypeComponent.OnManipulationStarted.RemoveListener(OnSelection);
        manipulationAssetTypeComponent.OnManipulationEnded.RemoveListener(OnRelease);
    }

    #endregion

    #region Getters and Setters

    public BodySection GetBodySection()
    {
        return assetData.bodySection.runtimeData.Value;
    }

    public BodySystem GetBodySystem()
    {
        return assetData.bodySystem.runtimeData.Value;
    }

    public int GetSystemLayer()
    {
        return assetData.systemLayer.runtimeData.Value;
    }

    public float GetWeight()
    {
        return assetData.weight.runtimeData.Value;
    }

    public bool IsOnScaleButInactive()
    {
        return onScaleButInactive;
    }

    public void SetOnScaleButInactive(bool input)
    {
        onScaleButInactive = input;
    }

    #endregion

    public void ResetObject()
    {
        bodyPartObject.transform.localPosition = Vector3.zero;
        bodyPartObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
        hitBox.isTrigger = true;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        if (autopsyScaleAssetTypeComponent != null)
        {
            autopsyScaleAssetTypeComponent.RemoveObjectFromScale(this);
        }
    }

    #region Manipulation Functions

    // Called when the object is selected by the user
    public void OnSelection()
    {
        ObeyPhysics(false);
    }

    // Called when the object is released by the user
    public void OnRelease()
    {
        rb.velocity = Vector3.zero;
        StartCoroutine(ResetVel());
        if (assetData.inGravityZone.runtimeData.Value)
        {
            StartCoroutine(DelayObeyPhysics(true));
        }
    }

    IEnumerator ResetVel()
    {
        yield return new WaitForEndOfFrame();
        rb.velocity = Vector3.zero;
    }

    IEnumerator DelayObeyPhysics(bool isObeying)
    {
        yield return new WaitForEndOfFrame();
        ObeyPhysics(isObeying);
    }

    public void ObeyPhysics(bool isObeying)
    {
        hitBox.isTrigger = !isObeying;
        rb.useGravity = isObeying;
        if (!isObeying)
        {
            rb.velocity = Vector3.zero;
        }
    }

    #endregion

    #region Collision and Trigger Functions

    public void EnterGravityZone(Collider other)
    {
        if (autopsyScaleAssetTypeComponent == null)
        {
            autopsyScaleAssetTypeComponent = other.transform.parent.parent.GetComponent<AutopsyScaleAssetTypeComponent>();
        }
        assetData.inGravityZone.runtimeData.Value = true;
        autopsyScaleAssetTypeComponent.AddObjectToScale(this);
    }

    public void ExitGravityZone(Collider other)
    {
        assetData.inGravityZone.runtimeData.Value = false;
        autopsyScaleAssetTypeComponent.RemoveObjectFromScale(this);
        ObeyPhysics(false);
    }

    #endregion

}