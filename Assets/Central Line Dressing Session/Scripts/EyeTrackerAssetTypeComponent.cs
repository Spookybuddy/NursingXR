using GIGXR.Platform.CommonAssetTypes;
using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using Microsoft.MixedReality.Toolkit;
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

public class EyeTrackerAssetTypeComponent : BaseAssetTypeComponent<EyeTrackerAssetData>
{
    [SerializeField] private GameObject gazePositionObj;
    
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
        
    }

    protected override void Teardown()
    {

    }

    #endregion

    public void Update()
    {
        Debug.Log("Eyetracking enabled: " + CoreServices.InputSystem.EyeGazeProvider.IsEyeTrackingEnabledAndValid);
        gazePositionObj.transform.position = CoreServices.InputSystem.EyeGazeProvider.HitPosition;
    }

}
