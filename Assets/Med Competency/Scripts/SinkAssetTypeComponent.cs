using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Scenarios;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;

public class SinkAssetTypeComponent : BaseAssetTypeComponent<SinkAssetData>
{
    [SerializeField] private GameObject handsWashedText;
    [SerializeField] private StepManagerMedCompAssetTypeComponent stepManager;
    [SerializeField] private RoomChangeAssetTypeComponent roomChange;

    private bool handsWashedStep1;
    private bool handsWashedStep4;
    private bool handsWashedStep8;

    #region Dependencies

    private IScenarioManager scenarioManager;

    [InjectDependencies]
    public void InjectDependencies(IScenarioManager injectScenarioManager)
    {
        scenarioManager = injectScenarioManager;
    }

    #endregion

    #region BaseAssetTypeComponent overrides

    public override void SetEditorValues()
    {
        throw new System.NotImplementedException();
    }

    protected override void Setup()
    {
        throw new System.NotImplementedException();
    }

    protected override void Teardown()
    {
        throw new System.NotImplementedException();
    }

    #endregion

    public void OnHandsWashed()
    {
        stepManager = GameObject.Find("stepManager (Step Manager(Clone))").GetComponent<StepManagerMedCompAssetTypeComponent>();
        //roomChange = GameObject.Find(" ").GetComponent<RoomChangeAssetTypeComponent>();

        if (roomChange.AssetData.inPatientRoom.runtimeData.Value == false)
        {
            stepManager.AssetData.stepCompletedBool[0] = true;
            handsWashedStep1 = true;
        }

        if (roomChange.AssetData.inPatientRoom.runtimeData.Value == true && handsWashedStep1 == true)
        {
            stepManager.AssetData.stepCompletedBool[3] = true;
            handsWashedStep4 = true;
        }

        if (roomChange.AssetData.inPatientRoom.runtimeData.Value == true && handsWashedStep4 == true)
        {
            stepManager.AssetData.stepCompletedBool[7] = true;
            handsWashedStep8 = true;
        }
    }

    public void ResetHandWash()
    {
        handsWashedText.SetActive(false);
    }
}
