using GIGXR.Platform.CommonAssetTypes;
using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using GIGXR.Platform.Scenarios.Data;
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

public class CatheterSiteAssetTypeComponent : BaseAssetTypeComponent<CatheterSiteAssetData>
{
    [SerializeField] private GameObject oldTegadermSlider, oldTegaderm;
    [SerializeField] private MeshRenderer sliderArrow;
    private bool notCompletedStep0 = true, notCompletedStep1 = true;
    
    private StepManagerAssetTypeComponent stepManager;
    private SkinnedMeshRenderer oldTegadermMesh;

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
        oldTegadermMesh = oldTegaderm.GetComponent<SkinnedMeshRenderer>();
    }

    protected override void Teardown()
    {

    }

    #endregion

    #region Getters & Setters

    public void SetOldTegadermSliderValue(SliderEventData eventData)
    {
        assetData.oldTegadermSliderValue.runtimeData.Value = eventData.NewValue;
        Debug.Log("Old Tegaderm Sliderval " + eventData.NewValue);
    }

    public void SetNotHoldingDownCatheter(bool notHolding)
    {
        assetData.notHoldingDownCatheter.runtimeData.Value = notHolding;
    }

    #endregion

    #region Property Change Handlers

    [RegisterPropertyChange(nameof(CatheterSiteAssetData.oldTegadermSliderValue))]
    private void OnOldTegadermSliderValueChanged(AssetPropertyChangeEventArgs args)
    {
        if (!IsInitialized || scenarioManager.ScenarioStatus != ScenarioStatus.Playing)
        {
            return;
        }

        if (notCompletedStep0)
        {
            if (assetData.notHoldingDownCatheter.runtimeData.Value && notCompletedStep1)
            {
                stepManager.CheckStep1();
                notCompletedStep1 = false;
            }
            else
            {
                notCompletedStep1 = false;
            }

            oldTegadermMesh.SetBlendShapeWeight(0, (float)args.AssetPropertyValue * 100);
            oldTegadermMesh.SetBlendShapeWeight(1, (float)args.AssetPropertyValue * 100);
            oldTegadermMesh.SetBlendShapeWeight(2, (float)args.AssetPropertyValue * 100);

            if (Mathf.Abs((float)args.AssetPropertyValue - 1) < .001f && notCompletedStep0)
            {
                oldTegadermSlider.SetActive(false);
                oldTegaderm.SetActive(false);
                notCompletedStep0 = false;
                stepManager.CheckStep0();
                stepManager.AddNextOrder0();
            }
        }
    }

    #endregion

    #region Asset Property Validators

    //Object returned is the update value, bool returned indicates if we want to allow the property change
    [RegisterPropertyValidator(nameof(CatheterSiteAssetData.oldTegadermSliderValue))]
    public (object, bool) ValidateFullSideCoverSliderValue(object value)
    {
        if (!IsInitialized)
        {
            return (value, true);
        }

        if (notCompletedStep0)
        {
            return (value, true);
        }

        return (value, false);
    }

    [RegisterPropertyValidator(nameof(CatheterSiteAssetData.notHoldingDownCatheter))]
    public (object, bool) ValidateNotHoldingDownCatheterValue(object value)
    {
        if (!IsInitialized)
        {
            return (value, true);
        }

        bool objectValue = (bool)value;

        if (assetData.notHoldingDownCatheter.runtimeData.Value != objectValue)
        {
            return (value, true);
        }

        return (value, false);
    }

    #endregion

    #region Slider Arrow Visibility Functions

    public void ShowSliderArrow()
    {
        sliderArrow.enabled = true;
    }

    public void HideSliderArrow()
    {
        sliderArrow.enabled = false;
    }

    #endregion
}
