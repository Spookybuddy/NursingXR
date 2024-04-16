using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.Data;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using GIGXR.Platform.Utilities;
using System.Collections;
using UnityEngine;

public class ProcedureFieldAssetTypeComponent : BaseAssetTypeComponent<ProcedureFieldAssetData>
{
    [Tooltip("Is this script the manager?")]
    [SerializeField] public bool isManager;

    //Manager specific variables
    [HideInInspector] public int arraySize;

    private ProcedureFieldAssetTypeComponent[] medicalSupplies;

    //Non-Manager variables
    [HideInInspector] public ProcedureFieldAssetTypeComponent manager;

    [HideInInspector] public int usedOnStep;

    [HideInInspector] public int usedBy;

    [HideInInspector] public bool isInUse;

    private int onStep;

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
            //Manager finds all supplies and gets their procedure scripts for communication later
            GameObject[] supplies = GameObject.FindGameObjectsWithTag("Supplies");
            medicalSupplies = new ProcedureFieldAssetTypeComponent[supplies.Length];
            for (int i = 0; i < supplies.Length; i++) medicalSupplies[i] = supplies[i].GetComponent<ProcedureFieldAssetTypeComponent>();
        } else {
            //Non-manager supplies finds the manager script
            GameObject managerObject = GameObject.FindWithTag("GameController");
            if (managerObject != null) {
                if (managerObject.TryGetComponent(out ProcedureFieldAssetTypeComponent script)) {
                    manager = script;
                }
            } else StartCoroutine(RetryFindManager());
        }
    }

    protected override void Teardown()
    {

    }

    //Grab Trigger
    public void OnManipulationStart()
    {
        isInUse = true;
    }

    //Place Trigger
    public void OnManipulationEnd()
    {
        isInUse = false;
    }

    //Enter Trigger
    public void OnTriggerEnter(Collider other)
    {
        Debug.LogWarning("Trigger Entered : " + other.name);
        if (other.CompareTag("Supplies")) {
            if (CheckStep()) {
                if (usedBy == 1 || usedBy == 2 || usedBy == 3) {

                }
            }
        }
    }

    //Stay Trigger
    public void OnTriggerStay(Collider other)
    {
        if (isInUse) {
            if (other.CompareTag("Supplies")) {
                if (CheckStep()) {
                    if (usedBy == 0) {

                    }
                }
            }
        }
    }

    //Exit Trigger
    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Supplies")) {
            if (CheckStep()) {
                if (usedBy != 0) {

                }
            }
        }
    }

    //Checks if the object step is on or manager step matches
    private bool CheckStep()
    {
        if (onCorrectStep || usedOnStep == manager.onStep) return true;
        else return false;
    }

    //When value is changed Manager will update other supplies and 
    [RegisterPropertyChange(nameof(ProcedureFieldAssetData.step))]
    private void OnStepChanged(AssetPropertyChangeEventArgs args)
    {
        if (!IsInitialized) return;

        onStep = (int)args.AssetPropertyValue;
        if (isManager) {
            //Manager updates all the steps in the supply list
            for (int i = 0;i < medicalSupplies.Length; i++) {
                medicalSupplies[i].UpdateStep(onStep);
            }
        } else {
            //Update the boolean for correct step
            onCorrectStep = (onStep == usedOnStep);

            //If the manager step number does not match this step number
            if (onStep != manager.onStep) manager.UpdateStep(onStep);
        }

        //assetData.step.runtimeData.Value++;
    }

    //Public method to update steps to given number
    public void UpdateStep(int step)
    {
        if (assetData.step.runtimeData.Value != step) assetData.step.runtimeData.Value = step;
    }

    private IEnumerator RetryFindManager()
    {
        yield return new WaitForSeconds(0.5f);
        if (GameObject.FindWithTag("GameController").TryGetComponent(out ProcedureFieldAssetTypeComponent script)) manager = script;
        else StartCoroutine(RetryFindManager());
    }
}