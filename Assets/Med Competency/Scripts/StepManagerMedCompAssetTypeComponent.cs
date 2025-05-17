using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.GigAssets;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;

public class StepManagerMedCompAssetTypeComponent : BaseAssetTypeComponent<StepManagerMedCompAssetData>
{
    private bool hasFinished = false;
    [SerializeField] private TMP_Text stepCheckText;
    [SerializeField] private Interactable nextPageButton, prevPageButton, finishButton;

    #region Dependencies

    //Scenarios
    private IScenarioManager scenarioManager;

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
        finishButton.OnClick.AddListener(CheckSteps);
    }

    protected override void Teardown()
    {

    }

    #endregion

    public void Awake()
    {
        stepCheckText.text = "";
        
        assetData.stepCompletedBool = new bool[7];

        for (int i = 0; i < assetData.stepList.Length - 1; i++) 
        {
            assetData.stepCompletedBool[i] = false;
        }

        Debug.Log("woop Steps = " + assetData.stepCompletedBool.Length);
    }

    public void CheckSteps() 
    {
        //Gets rid of the finish button once the button is clicked
        finishButton.gameObject.SetActive(false);

        //Loops through all of the steps in the list
        for (int i = 0; i < assetData.stepCompletedBool.Length - 1; i++)
        {
            Debug.Log("Step woop");
            //Prints the step text
            stepCheckText.text += "\n" + assetData.stepList[i] + " - ";
            
            //Determines whether the tep was completed or no and prints the result accordingly
            if (assetData.stepCompletedBool[i] == true)
            {
                Debug.Log("Step completed woop");
                stepCheckText.text += "Completed\n";
            }
            else if (assetData.stepCompletedBool[i] == false)
            {
                Debug.Log("Step failed woop");
                stepCheckText.text += "Incomplete\n";
            }
        }
    }
}
