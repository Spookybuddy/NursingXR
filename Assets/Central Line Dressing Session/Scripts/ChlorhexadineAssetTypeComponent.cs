using GIGXR.Platform.CommonAssetTypes;
using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.WSA;

public class ChlorhexadineAssetTypeComponent : BaseAssetTypeComponent<ChlorhexadineAssetData>
{
    private bool leftWing1Triggered = false, leftWing2Triggered = false,
                 rightWing1Triggered = false, rightWing2Triggered = false;

    private string fingerHittingLeft1 = "", fingerHittingLeft2 = "",
                   fingerHittingRight1 = "", fingerHittingRight2 = "";

    private bool notCompletedStep2 = true, notCompletedStep3 = true;

    private bool[] step3CleanZones = { false, false, false, false };

    private StepManagerAssetTypeComponent stepManager;

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
        StepManagerAssetTypeComponent[] tempManagers = new StepManagerAssetTypeComponent[0];
        do
        {
            tempManagers = GameObject.FindObjectsByType<StepManagerAssetTypeComponent>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        } while (tempManagers.Length == 0);
        stepManager = tempManagers[0];
    }

    protected override void Teardown()
    {

    }

    #endregion

    #region Getters & Setters

    private void ResetStep3CleanZones()
    {
        step3CleanZones = new bool[] { false, false, false, false };
    }

    #endregion

    #region Collision & Trigger Functions

    public void WingHitBoxTriggered(string fingerName, string wingTriggerName)
    {
        if (!IsInitialized)
        {
            return;
        }
        if (notCompletedStep2)
        {
            if (wingTriggerName == "Left Wing Hitbox 1")
            {
                if (!leftWing1Triggered && !leftWing2Triggered && !rightWing1Triggered)
                {
                    fingerHittingLeft1 = fingerName;
                    leftWing1Triggered = true;
                }
                else if (rightWing1Triggered)
                {
                    if (((fingerHittingRight1 + fingerName).Contains("Left Thumb") &&
                        (fingerHittingRight1 + fingerName).Contains("Left Index")) ||
                        ((fingerHittingRight1 + fingerName).Contains("Right Thumb") &&
                        (fingerHittingRight1 + fingerName).Contains("Right Index")))
                    {
                        fingerHittingLeft1 = fingerName;
                        leftWing1Triggered = true;
                    }
                }
            }
            else if (wingTriggerName == "Left Wing Hitbox 2")
            {
                if (leftWing1Triggered && fingerName == fingerHittingLeft1)
                {
                    fingerHittingLeft2 = fingerName;
                    leftWing2Triggered = true;
                }
            }
            else if (wingTriggerName == "Right Wing Hitbox 1")
            {
                if (!rightWing1Triggered && !rightWing2Triggered && !leftWing1Triggered)
                {
                    fingerHittingRight1 = fingerName;
                    rightWing1Triggered = true;
                }
                else if (rightWing1Triggered)
                {
                    if (((fingerHittingLeft1 + fingerName).Contains("Left Thumb") &&
                        (fingerHittingLeft1 + fingerName).Contains("Left Index")) ||
                        ((fingerHittingLeft1 + fingerName).Contains("Right Thumb") &&
                        (fingerHittingLeft1 + fingerName).Contains("Right Index")))
                    {
                        fingerHittingRight1 = fingerName;
                        rightWing1Triggered = true;
                    }
                }
            }
            else if (wingTriggerName == "Right Wing Hitbox 2")
            {
                if (rightWing1Triggered && fingerName == fingerHittingRight1)
                {
                    fingerHittingRight2 = fingerName;
                    rightWing2Triggered = true;
                }
            }

            if (leftWing2Triggered && rightWing2Triggered)
            {
                Debug.Log("Checking Step 2");
                stepManager.CheckStep2();
                stepManager.AddNextOrder2();
                if (notCompletedStep3)
                {
                    Debug.Log("Resetting Step 3");
                    ResetStep3CleanZones();
                }
                notCompletedStep2 = false;
            }
        }
    }

    public void WingHitBoxExited(string fingerName, string wingTriggerName)
    {
        if (!IsInitialized)
        {
            return;
        }
        if (notCompletedStep2)
        {
            if (wingTriggerName == "Left Wing Hitbox 1")
            {
                if (leftWing1Triggered && fingerName == fingerHittingLeft1)
                {
                    fingerHittingLeft1 = "";
                    leftWing1Triggered = false;
                }
            }
            else if (wingTriggerName == "Left Wing Hitbox 2")
            {
                if (leftWing2Triggered && fingerName == fingerHittingLeft2)
                {
                    fingerHittingLeft2 = "";
                    leftWing2Triggered = false;
                }
            }
            else if (wingTriggerName == "Right Wing Hitbox 1")
            {
                if (rightWing1Triggered && fingerName == fingerHittingRight1)
                {
                    fingerHittingRight1 = "";
                    rightWing1Triggered = false;
                }
            }
            else if (wingTriggerName == "Right Wing Hitbox 2")
            {
                if (rightWing2Triggered && fingerName == fingerHittingRight2)
                {
                    fingerHittingRight2 = "";
                    rightWing2Triggered = false;
                }
            }
        }
    }

    public void TestForStep3Progress(Collider other)
    {
        if (!IsInitialized)
        {
            return;
        }

        if (notCompletedStep3)
        {
            if (other.name == "Chlorhexadine Clean Hitbox 0" && !step3CleanZones[0])
            {
                Debug.Log("Checking 0");
                step3CleanZones[0] = true;
            }
            else if (other.name == "Chlorhexadine Clean Hitbox 1" && !step3CleanZones[1])
            {
                Debug.Log("Checking 1");
                step3CleanZones[1] = true;
            }
            else if (other.name == "Chlorhexadine Clean Hitbox 2" && !step3CleanZones[2])
            {
                Debug.Log("Checking 2");
                step3CleanZones[2] = true;
            }
            else if (other.name == "Chlorhexadine Clean Hitbox 3" && !step3CleanZones[3])
            {
                Debug.Log("Checking 3");
                step3CleanZones[3] = true;
            }
            foreach (bool zone in step3CleanZones)
            {
                if (!zone)
                {
                    return;
                }
            }

            Debug.Log("Checking step 3");
            stepManager.CheckStep3();
            stepManager.AddNextOrder3();
            notCompletedStep3 = false;
        }
    }

    #endregion
}
