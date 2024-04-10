using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.Data;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using GIGXR.Platform.Utilities;
using UnityEngine;

public class ProcedureFieldAssetTypeComponent : BaseAssetTypeComponent<ProcedureFieldAssetData>
{
    [Tooltip("Is this script the manager?")]
    [SerializeField] public bool isManager;

    //Manager specific variables
    [HideInInspector]
    public int arraySize;

    [HideInInspector]
    public GameObject[] medicalSupplies;

    //Non-Manager variables
    [HideInInspector]
    public ProcedureFieldAssetTypeComponent manager;

    [HideInInspector]
    public int usedOnStep;

    [HideInInspector]
    public int usedBy;

    [HideInInspector]
    public bool isInUse;

    [HideInInspector]
    public bool onCorrectStep;

    private IScenarioManager scenarioManager;

    [InjectDependencies]
    public void InjectDependencies(IScenarioManager injectedScenarioManager)
    {
        scenarioManager = injectedScenarioManager;
    }

    public override void SetEditorValues()
    {

    }

    protected override void Setup()
    {

    }

    protected override void Teardown()
    {

    }


}
