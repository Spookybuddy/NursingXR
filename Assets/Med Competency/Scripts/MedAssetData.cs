using System;
using GIGXR.Platform.Scenarios.GigAssets.Data;
using UnityEngine;

[Serializable]
public class MedAssetData : BaseAssetData
{
    // Name of the medicine
    public string MedName;

    //Dosage that the maedicine has
    public float MedDosage;

    //Expiration of the medicine, both the year and month
    //NOt USED YET: ADD WHEN NURSE WILL HAVE TO CHECK TO SEE IF THE MEDS ARE EXPIRED 
    public AssetPropertyDefinition<int> MedExpirationMonth;
    public AssetPropertyDefinition<int> MedExpirationYear;

    //Route of the medicine
    public string MedRoute;

    //Name of the patient these meds are intended for
    public string PatientName;
}
