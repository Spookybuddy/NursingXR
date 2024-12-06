using GIGXR.Platform.CommonAssetTypes;
using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.WSA;

public class TegadermAssetTypeComponent : BaseAssetTypeComponent<TegadermAssetData>
{
    private StepManagerAssetTypeComponent stepManager;
    private CatheterSiteAssetTypeComponent catheterSite;
    [SerializeField] private GameObject fullSideCoverSlider, outlineCoverSlider;
    [SerializeField] private TegadermCollisionDetector collisionDetector;

    private bool notCompletedStep5 = true, notAddedStep6Order = true, notCompletedStep7 = true;

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
        StepManagerAssetTypeComponent[] tempManagers = new StepManagerAssetTypeComponent[0];
        do
        {
            tempManagers = GameObject.FindObjectsByType<StepManagerAssetTypeComponent>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        } while (tempManagers.Length == 0);
        stepManager = tempManagers[0];

        CatheterSiteAssetTypeComponent[] tempCatheterSites = new CatheterSiteAssetTypeComponent[0];
        do
        {
            tempCatheterSites = GameObject.FindObjectsByType<CatheterSiteAssetTypeComponent>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        } while (tempCatheterSites.Length == 0);
        catheterSite = tempCatheterSites[0];
    }

    protected override void Teardown()
    {

    }

    #endregion

    #region Getters & Setters

    public void SetFullSideCoverSliderValue(SliderEventData eventData)
    {
        assetData.fullSideCoverSliderValue.runtimeData.Value = eventData.NewValue;
    }

    public void SetOutlineCoverSliderValue(SliderEventData eventData)
    {
        assetData.outlineCoverSliderValue.runtimeData.Value = eventData.NewValue;
    }

    #endregion

    #region Check Step Functions

    public void CheckStep6(Collider other)
    {
        if (!notCompletedStep5)
        {
            stepManager.CheckStep6();
            collisionDetector.CompletedStep6();
            Destroy(GetComponent<ManipulationAssetTypeComponent>());
            Destroy(gameObject.GetComponentInChildren<ObjectManipulator>());
            transform.position = other.transform.position;
            transform.localRotation = Quaternion.Euler(new Vector3(290, 90, 180));
        }
        if (notAddedStep6Order)
        {
            stepManager.AddNextOrder6();
            notAddedStep6Order = false;
        }
    }

    #endregion

    #region Property Change Handlers

    [RegisterPropertyChange(nameof(TegadermAssetData.fullSideCoverSliderValue))]
    private void OnFullSideCoverSliderValueChanged(AssetPropertyChangeEventArgs args)
    {
        if (!IsInitialized)
        {
            return;
        }
        
        if (Mathf.Abs((float)args.AssetPropertyValue - 1) < .001f && notCompletedStep5)
        {
            fullSideCoverSlider.SetActive(false);
            notCompletedStep5 = false;
            stepManager.CheckStep5();
            stepManager.AddNextOrder5();
        }
    }

    [RegisterPropertyChange(nameof(TegadermAssetData.outlineCoverSliderValue))]
    private void OnOutlineCoverSliderValueChanged(AssetPropertyChangeEventArgs args)
    {
        if (!IsInitialized)
        {
            return;
        }

        if (Mathf.Abs((float)args.AssetPropertyValue - 1) < .001f && notCompletedStep7)
        {
            outlineCoverSlider.SetActive(false);
            notCompletedStep7 = false;
            stepManager.CheckStep7();
            stepManager.AddNextOrder7();
        }
    }

    #endregion

    #region Asset Property Validators

    //Object returned is the update value, bool returned indicates if we want to allow the property change
    [RegisterPropertyValidator(nameof(TegadermAssetData.fullSideCoverSliderValue))]
    public (object, bool) ValidateFullSideCoverSliderValue(object value)
    {
        if (!IsInitialized)
        {
            return (value, true);
        }

        if (notCompletedStep5)
        {
            return (value, true);
        }

        return (value, false);
    }

    [RegisterPropertyValidator(nameof(TegadermAssetData.outlineCoverSliderValue))]
    public (object, bool) ValidateOutlineCoverSliderValue(object value)
    {
        if (!IsInitialized)
        {
            return (value, true);
        }

        if (notCompletedStep7)
        {
            return (value, true);
        }

        return (value, false);
    }

    #endregion

}
