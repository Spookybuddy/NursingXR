using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.GigAssets;
using Microsoft.MixedReality.Toolkit.UI;
using GIGXR.Platform.Scenarios.Data;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using System;

public class MedCabinetAssetTypeComponent : BaseAssetTypeComponent<MedCabinetAssetData>
{
    //Buttons
    [Tooltip("The MRTK interactible used to select on the med cabinet - selects the patient and then slects and dispenses the meds")]
    [SerializeField] private Interactable selectButton;

    [Tooltip("The MRTK interactible used to scroll up through selection lists")]
    [SerializeField] private Interactable upButton;
    
    [Tooltip("The MRTK interactible used to scroll down through selection lists")]
    [SerializeField] private Interactable downButton;

    //Text Displays
    [Tooltip("The TMP text box that displays the information about the current medicine")]
    [SerializeField] private TMP_Text medDisplayText;

    //Scenarios
    private IScenarioManager scenarioManager;

    [SerializeField] private GameObject tempMeds;
    [SerializeField] private MedAssetTypeComponent tempMedAssetTypeComponent;

    private int menuIndex = 0;
    private int menuLength = 0;

    #region Dependencies

    [InjectDependencies]
    public void InjectDependencies(IScenarioManager injectScenarioManager)
    {
        scenarioManager = injectScenarioManager;
    }

    #endregion

    #region BaseAssetTypeComponent overrides
    public override void SetEditorValues()
    {

    }

    protected override void Setup()
    {
        //Adds listeners to the buttons
        selectButton.OnClick.AddListener(OnSelect);

        upButton.OnClick.AddListener(OnUpClicked);
        downButton.OnClick.AddListener(OnDownClicked);

        //Makes the med cabinet text the beginning of the patient list
        medDisplayText.text = "Please Log In";
    }

    protected override void Teardown()
    {
        selectButton.OnClick.RemoveListener(OnSelect);
        upButton.OnClick.RemoveListener(OnUpClicked);
        downButton.OnClick.RemoveListener(OnDownClicked);
    }

    #endregion

    #region Button Listeners

    private void OnSelect()
    {
        if (scenarioManager.ScenarioStatus != ScenarioStatus.Playing) return;

        switch (assetData.menuOrder[menuIndex])
        {
            case "Login":
                //Increases the menu index (moves to next menu)
                menuIndex++;

                //Changes the display text to the default of the next menu
                medDisplayText.text = assetData.patientList[0];

                //Changes the menu maximum to the next menu's length
                menuLength = assetData.patientList.Length - 1;
                break;
            case "Patient Selection":              
                menuIndex++;
                
                medDisplayText.text = assetData.medList[0, 0] + "\n" +
                assetData.medList[0, 1] + " mg " + assetData.medList[0, 2] + "\n" +
                assetData.medList[0, 3] + "\n\n" +
                "Last: " + assetData.medList[0, 4] + " hours ago";
                
                menuLength = assetData.medList.GetLength(0) - 1;
                break;
            case "Med Selection":
                menuIndex++;
                
                OnDispenseMeds();

                medDisplayText.text = "Meds Dispensed: Please Log Out";
                
                menuLength = 0;
                break;
            case "Done":

                //Might make closing the cabinet this
                medDisplayText.text = "Logged Out";

                break;
        }
    }

    private void OnDispenseMeds()
    {
        if (scenarioManager.ScenarioStatus != ScenarioStatus.Playing) return;

        //Finds gameobject currently acting as the "medicine"
        tempMeds = GameObject.Find("temp-med (TempMeds(Clone))");
        tempMedAssetTypeComponent = tempMeds.GetComponent<MedAssetTypeComponent>();

        assetData.medsDispensed.runtimeData.Value = true;
            
    }

    private void OnUpClicked()
    {
        //If the scenario is not playing, then nothing happens
        if (scenarioManager.ScenarioStatus != ScenarioStatus.Playing) return;

        if (assetData.medIndex.runtimeData.Value > 0)
        {
            assetData.medIndex.runtimeData.Value--;
        }
        else
        {
            assetData.medIndex.runtimeData.Value = menuLength;
        }
    }

    private void OnDownClicked()
    {
        //If the scenario is not playing, then nothing happens
        if (scenarioManager.ScenarioStatus != ScenarioStatus.Playing) return;

        if (assetData.medIndex.runtimeData.Value < menuLength)
        {
            assetData.medIndex.runtimeData.Value++;
        }
        else
        {
            assetData.medIndex.runtimeData.Value = 0;
        }
    }

    #endregion

    #region Property Change Handlers

    [RegisterPropertyChange(nameof(MedCabinetAssetData.medsDispensed))]
    private void OnDispenseMeds(AssetPropertyChangeEventArgs args)
    {
        bool medValue = (bool)args.AssetPropertyValue;

        if (medValue == true)
        {     
            Debug.Log("Meds Dispensed");

            tempMeds.transform.position = new Vector3(0.2f,-0.45f,0.75f);

            tempMedAssetTypeComponent.AssetData.MedName = assetData.medList[assetData.medIndex.runtimeData.Value, 0];
            tempMedAssetTypeComponent.AssetData.MedDosage = float.Parse(assetData.medList[assetData.medIndex.runtimeData.Value, 1]);
            tempMedAssetTypeComponent.AssetData.MedRoute = assetData.medList[assetData.medIndex.runtimeData.Value, 2];
        }
    }


    [RegisterPropertyChange(nameof(MedCabinetAssetData.medIndex))]
    private void OnIndexChange(AssetPropertyChangeEventArgs args)
    {
        //The argument is a generic variable so this line assigns it to its proper variable type (int in this case)
        int newValue = (int)args.AssetPropertyValue;

        switch (assetData.menuOrder[menuIndex])
        {
            case "Patient Selection":
                medDisplayText.text = assetData.patientList[newValue];
                break;
            case "Med Selection":
                medDisplayText.text = assetData.medList[newValue, 0] + "\n" +
                assetData.medList[newValue, 1] + " mg " + assetData.medList[newValue, 2] + "\n" +
                assetData.medList[newValue, 3] + "\n\n" + 
                "Last: " + assetData.medList[newValue, 4] + " hours ago";
                break;
        }
    }

    #endregion
}
