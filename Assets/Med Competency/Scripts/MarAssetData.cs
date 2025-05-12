using GIGXR.Platform.Scenarios.GigAssets.Data;
using System;
using UnityEngine;

[Serializable]
public class MarAssetData : BaseAssetData
{
    //Information about the medicine that the patient needs to be administered
    public string patientMed;
    //public AssetPropertyDefinition<string> patientMed;

    //Lists of possible medicines
    //Each column is a type of medicine, and the rows list its name, dosage, route, instructions, and last time given respectively
    public string[,] medList =
    {
        { "ondansetron", "8" , "PO", "Q12H PRN", "6"} ,
        { "acetominophen", "650" , "PO", "Q6H PRN", "12" }
    };
}
