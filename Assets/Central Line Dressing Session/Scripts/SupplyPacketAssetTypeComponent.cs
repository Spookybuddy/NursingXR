using GIGXR.Platform.CommonAssetTypes;
using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using GIGXR.Platform.Scenarios.GigAssets.Data;
using GIGXR.Platform.Scenarios.Data;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.WSA;

using GIGXR.Platform.Scenarios.EventArgs;

public class SupplyPacketAssetTypeComponent : BaseAssetTypeComponent<SupplyPacketAssetData>
{
    /* Chlorhexadine
     * 
     * 
     * 
     * 
     * 
     * 
     */
    
    
    [SerializeField] private GameObject slider, tempObjectParent;
    [SerializeField] private SkinnedMeshRenderer mesh;
    [SerializeField] private MeshRenderer sliderArrow;

    private bool notOpenedPacket = true, wasPlayingBefore = false, notSetObjectPositions = true;
    private GameObject alcoholSlider;
    private GameObject[] tegadermSliders;
    private ObjectManipulator[] allObjManipulators = new ObjectManipulator[5];
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
        ObjectManipulator[] tempManipulators = new ObjectManipulator[0];
        do
        {
            tempManipulators = GameObject.FindObjectsByType<ObjectManipulator>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        } while (tempManipulators.Length == 0);
        int objManipulatorsIndex = 0;
        foreach (ObjectManipulator objectManipulator in tempManipulators)
        {
            if (objectManipulator.HostTransform.name == "cld-chlorhexadine (Chlorhexadine Applicator_Individual0.2(Clone))" ||
                objectManipulator.HostTransform.name == "cld-tegaderm (Tegaderm(Clone))" ||
                objectManipulator.HostTransform.name == "gauze-large (Gauze Large_Individual0.2(Clone))" ||
                objectManipulator.HostTransform.name == "gauze-small (Gauze Small_Individual0.2(Clone))" ||
                objectManipulator.HostTransform.name == "cld-alcohol-swab (Alcohol Swab Package_Individual(Clone))")
            {
                allObjManipulators[objManipulatorsIndex] = objectManipulator;
                objManipulatorsIndex++;
                if (objectManipulator.HostTransform.name == "cld-tegaderm (Tegaderm(Clone))")
                {
                    tegadermSliders = objectManipulator.HostTransform.GetComponent<TegadermAssetTypeComponent>().GetSliders();
                    foreach (GameObject slider in tegadermSliders)
                    {
                        slider.SetActive(false);
                    }
                }
                if (objectManipulator.HostTransform.name == "cld-alcohol-swab (Alcohol Swab Package_Individual(Clone))")
                {
                    alcoholSlider = objectManipulator.HostTransform.GetComponent<AlcoholSwabAssetTypeComponent>().GetSlider();
                    alcoholSlider.SetActive(false);
                }
                objectManipulator.HostTransform.transform.parent = tempObjectParent.transform;
                objectManipulator.enabled = false;
            }
        }
    }

    public void Update()
    {
        if (scenarioManager != null)
        {   
            if (scenarioManager.ScenarioStatus == ScenarioStatus.Playing && !wasPlayingBefore && notOpenedPacket)
            {
                wasPlayingBefore = true;
                StartCoroutine(TurnOffObjectManipulators());
            }
            else if (scenarioManager.ScenarioStatus != ScenarioStatus.Playing && wasPlayingBefore && notOpenedPacket)
            {
                wasPlayingBefore = false;
                StartCoroutine(TurnOffObjectManipulators());
            }
        }
    }

    IEnumerator TurnOffObjectManipulators()
    {
        yield return new WaitForEndOfFrame();
        foreach (ObjectManipulator objectManipulator in allObjManipulators)
        {
            objectManipulator.enabled = false;
        }
    }

    IEnumerator SetObjectPositions()
    {
        yield return new WaitForSeconds(.1f);
        foreach (ObjectManipulator objectManipulator in allObjManipulators)
        {
            switch (objectManipulator.HostTransform.name)
            {
                case "cld-chlorhexadine (Chlorhexadine Applicator_Individual0.2(Clone))":
                    objectManipulator.HostTransform.localPosition = new Vector3(.072f, .0349f, -.0344f);
                    objectManipulator.HostTransform.localRotation = Quaternion.Euler(270, 296.34f, 0);
                    break;
                case "cld-tegaderm (Tegaderm(Clone))":
                    objectManipulator.HostTransform.localPosition = new Vector3(-.0203f, .004f, 0);
                    objectManipulator.HostTransform.localRotation = Quaternion.Euler(0, 90, 180);
                    break;
                case "gauze-large (Gauze Large_Individual0.2(Clone))":
                    objectManipulator.HostTransform.localPosition = new Vector3(-.0516f, .0554f, 0);
                    objectManipulator.HostTransform.localRotation = Quaternion.Euler(0, 0, 0);
                    break;
                case "gauze-small (Gauze Small_Individual0.2(Clone))":
                    objectManipulator.HostTransform.localPosition = new Vector3(-.0558f, .0448f, 0);
                    objectManipulator.HostTransform.localRotation = Quaternion.Euler(0, 0, 0);
                    break;
                case "cld-alcohol-swab (Alcohol Swab Package_Individual(Clone))":
                    objectManipulator.HostTransform.localPosition = new Vector3(-.0701f, .0175f, -.0236f);
                    objectManipulator.HostTransform.localRotation = Quaternion.Euler(0, 346.4f, 180);
                    break;
            }
        }
    }

    protected override void Teardown()
    {

    }

    #endregion

    #region Getters & Setters

    public void SetSliderValue(SliderEventData eventData)
    {
        assetData.sliderValue.runtimeData.Value = eventData.NewValue;
    }

    #endregion

    #region Property Change Handlers

    [RegisterPropertyChange(nameof(SupplyPacketAssetData.sliderValue))]
    private void OnOldTegadermSliderValueChanged(AssetPropertyChangeEventArgs args)
    {
        if (!IsInitialized || scenarioManager.ScenarioStatus != ScenarioStatus.Playing)
        {
            return;
        }

        if (notOpenedPacket)
        {
            mesh.SetBlendShapeWeight(0, (float)args.AssetPropertyValue * 100);
            mesh.SetBlendShapeWeight(1, (float)args.AssetPropertyValue * 100);

            if (Mathf.Abs((float)args.AssetPropertyValue - 1) < .001f && notOpenedPacket)
            {
                slider.SetActive(false);
                notOpenedPacket = false;
                foreach (ObjectManipulator objectManipulator in allObjManipulators)
                {
                    objectManipulator.HostTransform.parent = GameObject.Find("Content Marker Root").transform;
                    objectManipulator.enabled = true;
                }
                foreach (GameObject slider in tegadermSliders)
                {
                    slider.SetActive(true);
                }
                alcoholSlider.SetActive(true);
                gameObject.SetActive(false);
            }
        }
    }

    #endregion

    #region Asset Property Validators

    //Object returned is the update value, bool returned indicates if we want to allow the property change
    [RegisterPropertyValidator(nameof(SupplyPacketAssetData.sliderValue))]
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

[Serializable]
public class SupplyPacketAssetData : BaseAssetData
{
    public AssetPropertyDefinition<float> sliderValue;
}

