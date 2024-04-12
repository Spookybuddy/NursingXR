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
    [HideInInspector] public int arraySize;

    private GameObject[] medicalSupplies;

    [HideInInspector] public int stepAmount;

    [HideInInspector] public string[] treatmentAtStep;

    //Non-Manager variables
    [HideInInspector] public ProcedureFieldAssetTypeComponent manager;

    [HideInInspector] public int usedOnStep;

    [HideInInspector] public int usedBy;

    [HideInInspector] public bool isInUse;

    [HideInInspector] public bool onCorrectStep;

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
        if (isManager) {
            medicalSupplies = GameObject.FindGameObjectsWithTag("Supplies");
            for (int i = 0; i < medicalSupplies.Length; i++) {
                Debug.LogWarning(medicalSupplies[i].name);
            }
        } else {
            manager = GameObject.FindWithTag("GameController").GetComponent<ProcedureFieldAssetTypeComponent>();
        }
    }

    protected override void Teardown()
    {

    }

    private void CheckStep()
    {
        if (!isManager) {

        }
    }

    [RegisterPropertyChange(nameof(ProcedureFieldAssetData.step))]
    private void OnStepChanged(AssetPropertyChangeEventArgs args)
    {
        if (!IsInitialized) return;

        
    }
}