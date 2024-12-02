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

public class HandDetectorAssetTypeComponent : BaseAssetTypeComponent<HandDetectorAssetData>
{
    [SerializeField] private GameObject leftThumbLocObj, leftIndexLocObj, rightThumbLocObj, rightIndexLocObj;
    
    private ChlorhexadineAssetTypeComponent chlorhexadine;
    private CatheterSiteAssetTypeComponent catheterArea;
    private MixedRealityPose handPose;

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
        ChlorhexadineAssetTypeComponent[] tempChlorhexadines = new ChlorhexadineAssetTypeComponent[0];
        do
        {
            tempChlorhexadines = GameObject.FindObjectsByType<ChlorhexadineAssetTypeComponent>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        } while (tempChlorhexadines.Length == 0);
        chlorhexadine = tempChlorhexadines[0];

        CatheterSiteAssetTypeComponent[] tempCatheterAreas = new CatheterSiteAssetTypeComponent[0];
        do
        {
            tempCatheterAreas = GameObject.FindObjectsByType<CatheterSiteAssetTypeComponent>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        } while (tempCatheterAreas.Length == 0);
        catheterArea = tempCatheterAreas[0];
    }

    protected override void Teardown()
    {

    }

    #endregion

    #region MonoBehavior Functions

    private void Update()
    {
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbTip, Handedness.Left, out handPose))
        {
            leftThumbLocObj.transform.position = handPose.Position;
            leftThumbLocObj.SetActive(true);
        }
        else
        {
            leftThumbLocObj.SetActive(false);
        }

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, Handedness.Left, out handPose))
        {
            leftIndexLocObj.transform.position = handPose.Position;
            leftIndexLocObj.SetActive(true);
        }
        else
        {
            leftIndexLocObj.SetActive(false);
        }

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbTip, Handedness.Right, out handPose))
        {
            rightThumbLocObj.transform.position = handPose.Position;
            rightThumbLocObj.SetActive(true);
        }
        else
        {
            rightThumbLocObj.SetActive(false);
        }

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, Handedness.Right, out handPose))
        {
            rightIndexLocObj.transform.position = handPose.Position;
            rightIndexLocObj.SetActive(true);
        }
        else
        {
            rightIndexLocObj.SetActive(false);
        }
    }

    #endregion

    #region Getters & Setters

    public ChlorhexadineAssetTypeComponent GetChlorhexadine()
    {
        return chlorhexadine;
    }

    public CatheterSiteAssetTypeComponent GetCatheterArea()
    {
        return catheterArea;
    }

    #endregion

}