using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.GigAssets;
using Microsoft.MixedReality.Toolkit.UI;
using GIGXR.Platform.Scenarios.Data;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using UnityEngine;

public class MedAssetTypeComponent : BaseAssetTypeComponent<MedAssetData>
{
    private IScenarioManager scenarioManager;

    #region Dependencies
    [InjectDependencies]
    public void InjectDependencies (IScenarioManager injectScenarioManager)
    {
        scenarioManager = injectScenarioManager;
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



}
