using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedVerifier : MonoBehaviour
{
    public MarAssetTypeComponent mar;
    private MedAssetTypeComponent meds;

    public void Start()
    {
        mar = GameObject.Find("marClipboard (MARClipboard(Clone))").GetComponent<MarAssetTypeComponent>();
    }

    public void OralMedCheck()
    {
        meds = GameObject.Find("temp-med (TempMeds(Clone))").GetComponent<MedAssetTypeComponent>();

        Debug.Log("MAR med is " + mar.AssetData.patientMed + " woop");
        Debug.Log("Provide med is " + meds.AssetData.MedName + " woop");

        if (mar.AssetData.patientMed == meds.AssetData.MedName)
        {
            Debug.Log("SUCCESS WOOP");
        }
        else
        {
            Debug.Log("Failure :[ woop");
        }
    }
}
