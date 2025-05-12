using System;
using GIGXR.Platform.Scenarios.GigAssets.Data;
using GIGXR.Platform.Scenarios.GigAssets;
using UnityEngine;

[Serializable]
public class MedCabinetAssetData : BaseAssetData
{
    //Determines whether the patient has been chosen in the menu
    //public AssetPropertyDefinition<bool> patientChosen;

    //Determmines where the pyxis is in menu systems
    public string[] menuOrder =
    {
        "Login", "Patient Selection", "Med Selection", "Log Out", "Done"
    };

    //Determines whether medicine has been dispensed from the med cabinet
    public AssetPropertyDefinition<bool> medsDispensed;


    public string[,] medList =
    {
        { "ondansetron", "8" , "PO", "Q12H PRN", "6"} ,
        { "acetominophen", "650" , "PO", "Q6H PRN", "12" }
    };

    public string[] patientList =
    {
        "Fake Namerson", "Cindy Lake", "Charles Mann"
    };

    //Index of meds displayed in the list on the med cabinet 
    public AssetPropertyDefinition<int> medIndex;

    public AssetPropertyDefinition<string> medDisplay;

}
