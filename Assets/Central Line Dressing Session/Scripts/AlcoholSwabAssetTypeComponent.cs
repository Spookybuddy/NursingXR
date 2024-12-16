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
using ExitGames.Client.Photon;


public class AlcoholSwabAssetTypeComponent : BaseAssetTypeComponent<AlcoholSwabAssetData>
{
    private bool notOpenedPacket = true;
    private Vector3 newColliderCenter = new Vector3(-0.07029252f, 1.024455e-08f, 0);
    private Vector3 newColliderSize = new Vector3(0.1439853f, 0.01800332f, 0.01800332f);


    [SerializeField] private GameObject packet, swab, reference, slideDirection, tearSlider, manipulationCollider;
    [SerializeField] private MeshRenderer sliderArrow;
    [SerializeField] private SkinnedMeshRenderer packetMesh;
    
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

    public GameObject GetSlider()
    {
        return tearSlider;
    }

    public void SetTearSliderValue(SliderEventData eventData)
    {
        assetData.tearSliderValue.runtimeData.Value = eventData.NewValue;
    }

    #endregion

    #region Property Change Handlers

    [RegisterPropertyChange(nameof(AlcoholSwabAssetData.tearSliderValue))]
    private void OnTearSliderValueChanged(AssetPropertyChangeEventArgs args)
    {
        if (!IsInitialized)
        {
            return;
        }

        packetMesh.SetBlendShapeWeight(0, 100 * (float)args.AssetPropertyValue);

        if (Mathf.Abs((float)args.AssetPropertyValue - 1) < .001f && notOpenedPacket)
        {
            tearSlider.SetActive(false);
            notOpenedPacket = false;
            StartCoroutine(OpenPacketAnimation());
        }
    }

    IEnumerator OpenPacketAnimation()
    {
        yield return new WaitForSeconds(.5f);
        for (int i = 0; i < 120; i++)
        {
            swab.transform.position += (slideDirection.transform.position - reference.transform.position) * .2f / 120;
            yield return new WaitForSeconds(1 / 60f);
        }

        yield return new WaitForSeconds(.25f);
        packet.SetActive(false);
        manipulationCollider.transform.position = swab.transform.position;
        manipulationCollider.GetComponent<BoxCollider>().center = newColliderCenter;
        manipulationCollider.GetComponent<BoxCollider>().size = newColliderSize;
        swab.GetComponent<BoxCollider>().enabled = !swab.GetComponent<BoxCollider>().enabled;
    }

    #endregion

    #region Asset Property Validators

    //Object returned is the update value, bool returned indicates if we want to allow the property change
    [RegisterPropertyValidator(nameof(AlcoholSwabAssetData.tearSliderValue))]
    public (object, bool) ValidateFullSideCoverSliderValue(object value)
    {
        if (!IsInitialized)
        {
            return (value, true);
        }

        if (notOpenedPacket)
        {
            return (value, true);
        }

        return (value, false);
    }

    #endregion

    #region Collision & Trigger Functions

    public void SwabHitBoxTriggered()
    {
        Debug.Log("Checking Alcohol 3");
        stepManager.CheckStep3();
        stepManager.AddNextOrder3();
        stepManager.MarkOtherMistake1();
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
