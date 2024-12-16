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
using GIGXR.Platform.Mobile.WebView.EventBus.UnityToWebView.Events;

public class TegadermAssetTypeComponent : BaseAssetTypeComponent<TegadermAssetData>
{
    private StepManagerAssetTypeComponent stepManager;
    private CatheterSiteAssetTypeComponent catheterSite;
    [SerializeField] private GameObject fullSideCoverSlider, outlineCoverSlider;
    [SerializeField] private TegadermCollisionDetector collisionDetector;
    [SerializeField] private GameObject tegaderm, deformedTegaderm, deformedOutlineCover;
    [SerializeField] private SkinnedMeshRenderer tegadermCoverMesh, tegadermOutlineCoverMesh;
    [SerializeField] private MeshRenderer fullCoverSliderArrow, outlineCoverSliderArrow;
    private SkinnedMeshRenderer deformedOutlineCoverMesh;

    private bool notCompletedStep5 = true, notCompletedStep6 = true, notAddedStep6Order = true, notCompletedStep7 = true;

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

        deformedOutlineCoverMesh = deformedOutlineCover.GetComponent<SkinnedMeshRenderer>();
    }

    protected override void Teardown()
    {

    }

    #endregion

    #region Getters & Setters

    public GameObject[] GetSliders()
    {
        return new GameObject[2] {fullSideCoverSlider, outlineCoverSlider};
    }

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
            notCompletedStep6 = false;
            stepManager.CheckStep6();
            collisionDetector.CompletedStep6();
            Destroy(GetComponent<ManipulationAssetTypeComponent>());
            Destroy(gameObject.GetComponentInChildren<ObjectManipulator>());
            tegaderm.SetActive(false);
            transform.position = other.transform.position;
            transform.localRotation = Quaternion.Euler(new Vector3(290, 90, 180));
            outlineCoverSlider.transform.localPosition = new Vector3(
                                                        -outlineCoverSlider.transform.localPosition.x,
                                                         outlineCoverSlider.transform.localPosition.y,
                                                        -outlineCoverSlider.transform.localPosition.z);
            outlineCoverSlider.transform.localRotation = Quaternion.Euler(0, 0, 0);
            deformedTegaderm.SetActive(true);
            deformedOutlineCover.SetActive(true);
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

        tegadermCoverMesh.SetBlendShapeWeight(0, (float)args.AssetPropertyValue * 100);
        tegadermCoverMesh.SetBlendShapeWeight(1, (float)args.AssetPropertyValue * 100);
        tegadermCoverMesh.SetBlendShapeWeight(2, (float)args.AssetPropertyValue * 100);

        if (Mathf.Abs((float)args.AssetPropertyValue - 1) < .001f && notCompletedStep5)
        {
            fullSideCoverSlider.SetActive(false);
            notCompletedStep5 = false;
            StartCoroutine(Step5Anim());
            stepManager.CheckStep5();
            stepManager.AddNextOrder5();
        }
    }

    IEnumerator Step5Anim()
    {
        for (int i = 0; i < 60; i++)
        {
            tegadermCoverMesh.SetBlendShapeWeight(3, 10f * i / 60);
            yield return new WaitForSeconds(1 / 60f);
        }
        yield return new WaitForSeconds(.25f);
        tegadermCoverMesh.enabled = false;
    }

    [RegisterPropertyChange(nameof(TegadermAssetData.outlineCoverSliderValue))]
    private void OnOutlineCoverSliderValueChanged(AssetPropertyChangeEventArgs args)
    {
        if (!IsInitialized)
        {
            return;
        }

        deformedOutlineCoverMesh.SetBlendShapeWeight(0, (float)args.AssetPropertyValue * 100);

        if (notCompletedStep6)
        {
            tegadermOutlineCoverMesh.SetBlendShapeWeight(0, (float)args.AssetPropertyValue * 100);
        }

        if (Mathf.Abs((float)args.AssetPropertyValue - 1) < .001f && notCompletedStep7)
        {
            outlineCoverSlider.SetActive(false);
            notCompletedStep7 = false;
            StartCoroutine(Step7Anim());
            if (notCompletedStep6)
            {
                StartCoroutine(Step7EarlyAnim());
            }
            stepManager.CheckStep7();
            stepManager.AddNextOrder7();
        }
    }

    IEnumerator Step7EarlyAnim()
    {
        for (int i = 0; i < 30; i++)
        {
            tegadermOutlineCoverMesh.SetBlendShapeWeight(1, 100f * i / 30);
            yield return new WaitForSeconds(1 / 60f);
        }
        for (int i = 0; i < 30; i++)
        {
            tegadermOutlineCoverMesh.SetBlendShapeWeight(2, 100f * i / 30);
            yield return new WaitForSeconds(1 / 60f);
        }
        for (int i = 0; i < 30; i++)
        {
            tegadermOutlineCoverMesh.SetBlendShapeWeight(3, 10f * i / 30);
            yield return new WaitForSeconds(1 / 60f);
        }
        yield return new WaitForSeconds(.25f);
        tegadermOutlineCoverMesh.enabled = false;
    }

    IEnumerator Step7Anim()
    {
        for (int i = 0; i < 60; i++)
        {
            deformedOutlineCoverMesh.SetBlendShapeWeight(1, 100f * i / 60);
            yield return new WaitForSeconds(1 / 60f);
        }
        for (int i = 0; i < 30; i++)
        {
            deformedOutlineCoverMesh.SetBlendShapeWeight(2, 10f * i / 30);
            yield return new WaitForSeconds(1 / 60f);
        }
        yield return new WaitForSeconds(.25f);
        deformedOutlineCoverMesh.enabled = false;

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

    #region Slider Arrow Visibility Functions

    public void ShowFullCoverArrow()
    {
        fullCoverSliderArrow.enabled = true;
    }

    public void HideFullCoverArrow()
    {
        fullCoverSliderArrow.enabled = false;
    }

    public void ShowOutlineCoverArrow()
    {
        outlineCoverSliderArrow.enabled = true;
    }

    public void HideOutlineCoverArrow()
    {
        outlineCoverSliderArrow.enabled = false;
    }


    #endregion

}
