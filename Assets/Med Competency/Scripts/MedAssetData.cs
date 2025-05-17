using System;
using GIGXR.Platform.Scenarios.GigAssets.Data;
using UnityEngine;

[Serializable]
public class MedAssetData : BaseAssetData
{
    // Name of the medicine
    public AssetPropertyDefinition<string> MedName;

    //Dosage that the medicine has
    //NOT USED YET: ADD WHEN NURSE WILL HAVE TO CHECK TO SEE IF THE MEDS ARE EXPIRED 
    public float MedDosage;

    //Expiration of the medicine, both the year and month
    //NOT USED YET: ADD WHEN NURSE WILL HAVE TO CHECK TO SEE IF THE MEDS ARE EXPIRED 
    public AssetPropertyDefinition<int> MedExpirationMonth;
    public AssetPropertyDefinition<int> MedExpirationYear;

    //Route of the medicine
    //NOT USED YET: ADD WHEN NURSE WILL HAVE TO CHECK TO SEE IF THE MEDS ARE EXPIRED 
    public string MedRoute;

    //Name of the patient these meds are intended for
    public AssetPropertyDefinition<string> PatientName;
}
