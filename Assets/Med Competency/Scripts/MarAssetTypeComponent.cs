using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MarAssetTypeComponent : BaseAssetTypeComponent<MarAssetData>
{
    [Tooltip("The text which will display the patients needed med, dosage, and route.")]
    [SerializeField] private TMP_Text patientMedText;

    //Scenario Manager
    private IScenarioManager scenarioManager;

    private int medIndex;

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

    }

    protected override void Teardown()
    {

    }
    #endregion

    private void Start()
    {
        //Chooses a random med to be assigned for the patient to need to take in the MAR
        medIndex = Random.Range(0, assetData.medList.GetLength(0));

        //Assigns the med as chosen from the med list
        assetData.patientMed = assetData.medList[medIndex, 0];

        //Displays the text of the med info in the proper format
        patientMedText.text = assetData.medList[medIndex,0] + "\n" +
            assetData.medList[medIndex, 1] + " mg " + assetData.medList[medIndex, 2] + "\n" +
            assetData.medList[medIndex, 3] + "\n\n" + 
            "Last Given: " + assetData.medList[medIndex, 4] + " hours ago";
    }

}
