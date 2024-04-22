using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.ExtensionClasses;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.Data;
using GIGXR.Platform.Scenarios.EventArgs;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using GIGXR.Platform.Utilities;
using System.Collections;
using Unity.Mathematics;
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

    [HideInInspector] public Vector3 raycastScale;

    [HideInInspector] public float raycastDistance;

    private int onStep;

    [HideInInspector] public bool onCorrectStep;

    [Tooltip("The layer raycast checks are performed against.")]
    [SerializeField] private LayerMask interactionLayer;

    private Transform raycastOrigin;

    private GameObject particles;

    private float counter;

    private const float TouchTimer = 1.5f;
    private const float SprayTimer = 2.5f;
    private const float OintmentTimer = 2;
    private const float StayTimer = 5;
    private const float ExitTimer = 0.5f;
    private const float OtherTimer = 1;

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

            raycastOrigin = transform.FindChildOrGrandchild("Raycast");
            particles = transform.FindChildOrGrandchild("Particle").gameObject;
        }
    }

    protected override void Teardown()
    {

    }

    //Fixed update
    void FixedUpdate()
    {
        if (!isManager && isInUse) {
            if (CheckStep()) {
                //Only execute when on correct step and in use
                ProcedureFunctionSelection();
            }
        } 
    }

    //Grab Trigger
    public void OnManipulationStart()
    {
        isInUse = true;
        counter = 0;
    }

    //Place Trigger
    public void OnManipulationEnd()
    {
        isInUse = false;
        if (particles != null) particles.SetActive(false);
        counter = 0;
    }

    //Checks if the object step is on or manager step matches
    private bool CheckStep()
    {
        if (onCorrectStep || usedOnStep == manager.onStep) return true;
        else return false;
    }

    //Executes different code for each use case
    private void ProcedureFunctionSelection()
    {
        switch (usedBy) {
            case 0:
                //Nothing
                return;
            case 1:
                //Touch
                if (Physics.BoxCast(raycastOrigin.position, raycastScale, raycastOrigin.forward, out RaycastHit touch, quaternion.identity, raycastDistance, interactionLayer)) {
                    counter += Time.fixedDeltaTime;
                    if (counter > TouchTimer) UpdateStep(usedOnStep + 1);
                }
                return;
            case 2:
                //Spray
                if (Physics.BoxCast(raycastOrigin.position, raycastScale, raycastOrigin.forward, out RaycastHit spray, quaternion.identity, raycastDistance, interactionLayer)) {
                    particles.SetActive(true);
                    counter += Time.fixedDeltaTime;
                    if (counter > SprayTimer) UpdateStep(usedOnStep + 1);
                } else {
                    particles.SetActive(false);
                }
                return;
            case 3:
                //Ointment
                if (Physics.BoxCast(raycastOrigin.position, raycastScale, raycastOrigin.forward, out RaycastHit hit, quaternion.identity, raycastDistance, interactionLayer)) {
                    Debug.LogWarning(hit.transform.name);
                    counter += Time.fixedDeltaTime;
                    if (counter > OintmentTimer) UpdateStep(usedOnStep + 1);
                }
                return;
            case 4:
                //Stay
                if (Physics.BoxCast(raycastOrigin.position, raycastScale, raycastOrigin.forward, out RaycastHit stay, quaternion.identity, raycastDistance, interactionLayer)) {
                    Debug.LogWarning(stay.transform.name);
                    counter += Time.fixedDeltaTime;
                    if (counter > StayTimer) UpdateStep(usedOnStep + 1);
                }
                return;
            case 5:
                //Exit
                if (Physics.BoxCast(raycastOrigin.position, raycastScale, raycastOrigin.forward, out RaycastHit exit, quaternion.identity, raycastDistance, interactionLayer)) {
                    Debug.LogWarning(exit.transform.name);
                    counter += Time.fixedDeltaTime;
                } else {
                    if (counter > ExitTimer) UpdateStep(usedOnStep + 1);
                }
                return;
        }
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

    //Manager search loop
    private IEnumerator RetryFindManager()
    {
        Debug.LogWarning(gameObject.name + " cannot find the Medical Manager. Attempting search again...");
        yield return new WaitForSeconds(0.5f);
        if (GameObject.FindWithTag("GameController").TryGetComponent(out ProcedureFieldAssetTypeComponent script)) manager = script;
        else StartCoroutine(RetryFindManager());
    }
}