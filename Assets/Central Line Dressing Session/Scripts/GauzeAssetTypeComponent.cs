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


public class GauzeAssetTypeComponent : BaseAssetTypeComponent<GauzeAssetData>
{
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

    public void CheckStep4()
    {
        stepManager.CheckStep4();
    }
}
