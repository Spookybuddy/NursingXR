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

public class CatheterSiteAssetTypeComponent : BaseAssetTypeComponent<CatheterSiteAssetData>
{
    [SerializeField] private GameObject oldTegadermSlider;
    private bool notCompletedStep0 = true;
    
    private StepManagerAssetTypeComponent stepManager;

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

    [RegisterPropertyChange(nameof(TegadermAssetData.fullSideCoverSliderValue))]
    private void OnFullSideCoverSliderValueChanged(AssetPropertyChangeEventArgs args)
    {
        if (!IsInitialized)
        {
            return;
        }

        if (notCompletedStep0)
        {
            if (assetData.notHoldingDownCatheter.runtimeData.Value)
            {
                stepManager.CheckStep1();
            }

            if ((float)args.AssetPropertyValue - 1 < .001f && notCompletedStep0)
            {
                oldTegadermSlider.SetActive(false);
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

    #endregion
}
