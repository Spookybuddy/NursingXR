using GIGXR.Platform.CommonAssetTypes;
using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.WSA;

public class StepManagerAssetTypeComponent : BaseAssetTypeComponent<StepManagerAssetData>
{
    private int mistakePageNum = 0, totalPages = 0;
    [SerializeField] private TMP_Text mistakeDisplayText;
    [SerializeField] private Interactable nextPageButton, prevPageButton;

    private IScenarioManager scenarioManager;

    #region Dependencies

    [InjectDependencies]
    public void InjectDependencies(IScenarioManager injectedScenarioManager)
    {
        scenarioManager = injectedScenarioManager;
    }

    #endregion

    #region BaseAssetTypeComponent overrides

    public override void SetEditorValues()
    {

    }

    protected override void Setup()
    {
        nextPageButton.OnClick.AddListener(IncrementMistakePageNum);
        prevPageButton.OnClick.AddListener(DecrementMistakePageNum);
    }

    protected override void Teardown()
    {
        nextPageButton.OnClick.RemoveListener(IncrementMistakePageNum);
        prevPageButton.OnClick.RemoveListener(DecrementMistakePageNum);
    }

    #endregion

    #region Check Step Functions

    public void CheckStep0()
    {
        assetData.stepChecks.runtimeData.Value[0] = true;
    }

    public void CheckStep1()
    {
        assetData.stepChecks.runtimeData.Value[1] = true;
    }
    
    public void CheckStep2()
    {
        assetData.stepChecks.runtimeData.Value[2] = true;
    }

    public void CheckStep3()
    {
        assetData.stepChecks.runtimeData.Value[3] = true;
    }

    public void CheckStep4()
    {
        assetData.stepChecks.runtimeData.Value[4] = true;
    }

    public void CheckStep5()
    {
        assetData.stepChecks.runtimeData.Value[5] = true;
    }

    public void CheckStep6()
    {
        assetData.stepChecks.runtimeData.Value[6] = true;
    }

    public void CheckStep7()
    {
        assetData.stepChecks.runtimeData.Value[7] = true;
    }

    #endregion

    #region Order Mistake Functions

    public void AddNextOrder0()
    {
        int i;
        for (i = 0; i < assetData.attemptedOrder.runtimeData.Value.Length; i++)
        { 
            if (assetData.attemptedOrder.runtimeData.Value[i] == 0)
            {
                return;
            }
            if (assetData.attemptedOrder.runtimeData.Value[i] == -1)
            {
                break;
            }
        }
        if (i < assetData.attemptedOrder.runtimeData.Value.Length)
        {
            assetData.attemptedOrder.runtimeData.Value[i] = 0;
        }
    }

    public void AddNextOrder1()
    {
        int i;
        for (i = 0; i < assetData.attemptedOrder.runtimeData.Value.Length; i++)
        {
            if (assetData.attemptedOrder.runtimeData.Value[i] == 1)
            {
                return;
            }
            if (assetData.attemptedOrder.runtimeData.Value[i] == -1)
            {
                break;
            }
        }
        if (i < assetData.attemptedOrder.runtimeData.Value.Length)
        {
            assetData.attemptedOrder.runtimeData.Value[i] = 1;
        }
    }

    public void AddNextOrder2()
    {
        int i;
        for (i = 0; i < assetData.attemptedOrder.runtimeData.Value.Length; i++)
        {
            if (assetData.attemptedOrder.runtimeData.Value[i] == 2)
            {
                return;
            }
            if (assetData.attemptedOrder.runtimeData.Value[i] == -1)
            {
                break;
            }
        }
        if (i < assetData.attemptedOrder.runtimeData.Value.Length)
        {
            assetData.attemptedOrder.runtimeData.Value[i] = 2;
        }
    }

    public void AddNextOrder3()
    {
        int i;
        for (i = 0; i < assetData.attemptedOrder.runtimeData.Value.Length; i++)
        {
            if (assetData.attemptedOrder.runtimeData.Value[i] == 3)
            {
                return;
            }
            if (assetData.attemptedOrder.runtimeData.Value[i] == -1)
            {
                break;
            }
        }
        if (i < assetData.attemptedOrder.runtimeData.Value.Length)
        {
            assetData.attemptedOrder.runtimeData.Value[i] = 3;
        }
    }

    public void AddNextOrder4()
    {
        int i;
        for (i = 0; i < assetData.attemptedOrder.runtimeData.Value.Length; i++)
        {
            if (assetData.attemptedOrder.runtimeData.Value[i] == 4)
            {
                return;
            }
            if (assetData.attemptedOrder.runtimeData.Value[i] == -1)
            {
                break;
            }
        }
        if (i < assetData.attemptedOrder.runtimeData.Value.Length)
        {
            assetData.attemptedOrder.runtimeData.Value[i] = 4;
        }
    }

    public void AddNextOrder5()
    {
        int i;
        for (i = 0; i < assetData.attemptedOrder.runtimeData.Value.Length; i++)
        {
            if (assetData.attemptedOrder.runtimeData.Value[i] == 5)
            {
                return;
            }
            if (assetData.attemptedOrder.runtimeData.Value[i] == -1)
            {
                break;
            }
        }
        if (i < assetData.attemptedOrder.runtimeData.Value.Length)
        {
            assetData.attemptedOrder.runtimeData.Value[i] = 5;
        }
    }

    public void AddNextOrder6()
    {
        int i;
        for (i = 0; i < assetData.attemptedOrder.runtimeData.Value.Length; i++)
        {
            if (assetData.attemptedOrder.runtimeData.Value[i] == 6)
            {
                return;
            }
            if (assetData.attemptedOrder.runtimeData.Value[i] == -1)
            {
                break;
            }
        }
        if (i < assetData.attemptedOrder.runtimeData.Value.Length)
        {
            assetData.attemptedOrder.runtimeData.Value[i] = 6;
        }
    }

    #endregion

    #region Other Mistake Functions

    public void MarkOtherMistake0()
    {
        assetData.otherMistakes.runtimeData.Value[0] = true;
    }

    public void MarkOtherMistake1()
    {
        assetData.otherMistakes.runtimeData.Value[1] = true;
    }

    #endregion

    #region Mistake Text Functions

    public void SetUpMistakeText()
    {
        int mistakesMade = 0;
        string mistakeTextTemp = "";

        //Analyze which steps were performed
        for (int i = 0; i < assetData.stepChecks.runtimeData.Value.Length; i++)
        {
            if (!assetData.stepChecks.runtimeData.Value[i] && i != 4)
            {
                mistakeTextTemp += StepManagerAssetData.FAILED_CHECK_TEXT[i] + "\n\n";
                mistakesMade++;
            }

            if (assetData.stepChecks.runtimeData.Value[i] && i == 4)
            {
                if (assetData.otherMistakes.runtimeData.Value[0])
                {
                    mistakeTextTemp += StepManagerAssetData.FAILED_OTHER_TEXT[0] + "\n\n";
                    mistakesMade++;
                }
                else
                {
                    mistakeTextTemp += StepManagerAssetData.FAILED_CHECK_TEXT[i] + "\n\n";
                    mistakesMade++;
                }
            }
        }

        //Analyze the order of performed steps
        if (IsStepBeforeOther(3, 0))
        {
            mistakeTextTemp += StepManagerAssetData.FAILED_ORDER_TEXT[0] + "\n\n";
            mistakesMade++;
        }
        if (IsStepBeforeOther(6, 0))
        {
            mistakeTextTemp += StepManagerAssetData.FAILED_ORDER_TEXT[1] + "\n\n";
            mistakesMade++;
        }
        if (IsStepBeforeOther(3, 2))
        {
            mistakeTextTemp += StepManagerAssetData.FAILED_ORDER_TEXT[2] + "\n\n";
            mistakesMade++;
        }
        if (IsStepBeforeOther(6, 3))
        {
            mistakeTextTemp += StepManagerAssetData.FAILED_ORDER_TEXT[3] + "\n\n";
            mistakesMade++;
        }
        if (IsStepBeforeOther(6, 5))
        {
            mistakeTextTemp += StepManagerAssetData.FAILED_ORDER_TEXT[4] + "\n\n";
            mistakesMade++;
        }
        if (IsStepBeforeOther(7, 6))
        {
            mistakeTextTemp += StepManagerAssetData.FAILED_ORDER_TEXT[5] + "\n\n";
            mistakesMade++;
        }

        //Analyze if other mistakes were made
        if (assetData.otherMistakes.runtimeData.Value[1])
        {
            mistakeTextTemp += StepManagerAssetData.FAILED_OTHER_TEXT[1] + "\n\n";
            mistakesMade++;
        }

        //Updating Variables
        if (mistakesMade != 0)
        {
            totalPages = (mistakesMade + 2) / 3;
            assetData.mistakeText.runtimeData.Value = mistakeTextTemp;
        }

        DisplayMistakePage();
    }

    public bool IsStepBeforeOther(int step, int other)
    {
        int indexOfStep = -1, indexOfOther = -1;
        for (int i = 0; i < assetData.attemptedOrder.runtimeData.Value.Length; i++)
        {
            if (assetData.attemptedOrder.runtimeData.Value[i] == step)
            {
                indexOfStep = i;
            }
            if (assetData.attemptedOrder.runtimeData.Value[i] == other)
            {
                indexOfOther = i;
            }
        }
        return indexOfStep != -1 && indexOfStep < indexOfOther;
    }

    public void IncrementMistakePageNum()
    {
        if (mistakePageNum < totalPages)
        {
            mistakePageNum++;
        }
        DisplayMistakePage();
    }

    public void DecrementMistakePageNum()
    {
        if (mistakePageNum > 0)
        {
            mistakePageNum--;
        }
        DisplayMistakePage();
    }

    public void DisplayMistakePage()
    {
        int curNewlineNum, pageStartIndex = 0, pageEndIndex;

        for (curNewlineNum = 0; curNewlineNum < mistakePageNum * 3; curNewlineNum++)
        {
            pageStartIndex = assetData.mistakeText.runtimeData.Value.Substring(pageStartIndex).IndexOf("\n\n") + 2;
        }

        pageEndIndex = pageStartIndex;

        for (int i = 0; i < 3; i++)
        {
            if (assetData.mistakeText.runtimeData.Value.Substring(pageEndIndex).IndexOf("\n\n") == -1)
            {
                pageEndIndex = -1;
                break;
            }

            pageEndIndex = assetData.mistakeText.runtimeData.Value.Substring(pageStartIndex).IndexOf("\n\n") + 2;
        }

        if (pageEndIndex == -1)
        {
            mistakeDisplayText.text = assetData.mistakeText.runtimeData.Value.Substring(pageStartIndex);
        }
        else
        {
            mistakeDisplayText.text = assetData.mistakeText.runtimeData.Value.Substring(pageStartIndex, pageEndIndex - pageStartIndex);
        }
    }

    #endregion
}
