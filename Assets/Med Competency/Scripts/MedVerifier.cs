using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedVerifier : MonoBehaviour
{
    private bool medsChecked = false;
    
    private StepManagerMedCompAssetTypeComponent stepManager;
    
    private MarAssetTypeComponent mar;
    private MedAssetTypeComponent meds;

    public void Start()
    {
        mar = GameObject.Find("marClipboard (MARClipboard(Clone))").GetComponent<MarAssetTypeComponent>();
    }

    public void OralMedCheck()
    {
        if (medsChecked == false)
        {
            medsChecked = true;
            
            stepManager = GameObject.Find("stepManager (Step Manager(Clone))").GetComponent<StepManagerMedCompAssetTypeComponent>();
            meds = GameObject.Find("temp-med (TempMeds(Clone))").GetComponent<MedAssetTypeComponent>();

            Debug.Log("woop Steps = " + stepManager.AssetData.stepCompletedBool.Length);
            
            stepManager.AssetData.stepCompletedBool[4] = true;

            if (mar.AssetData.patientName.runtimeData.Value == meds.AssetData.PatientName.runtimeData.Value)
            {
                stepManager.AssetData.stepCompletedBool[1] = true;
            }

            if (mar.AssetData.patientMed == meds.AssetData.MedName.runtimeData.Value)
            {
                stepManager.AssetData.stepCompletedBool[2] = true;
            }
        }
    }
}
