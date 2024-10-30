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

public enum BodySection { Head, Torso, LeftArm, RightArm, LeftLeg, RightLeg, All };

public enum BodySystem { Outer, Inner1, Inner2 };

//----------------------------------------------------------------------------------------------------------
// NOTE: THE OBJECT ORGANIZER MUST ALWAYS BE THE LAST OBJECT IN THE PRESET ASSET LIST IN THE PRESET SCENARIO
//----------------------------------------------------------------------------------------------------------

public class AutopsyOrganizerAssetTypeComponent : BaseAssetTypeComponent<AutopsyOrganizerAssetData>
{
    public static readonly BodySystem[] ALL_BODY_SYSTEMS = { BodySystem.Outer,
                                                             BodySystem.Inner1,
                                                             BodySystem.Inner2 };

    public static readonly BodySection[] ALL_BODY_SECTIONS = { BodySection.Head, BodySection.Torso,
                                                               BodySection.LeftArm, BodySection.RightArm,
                                                               BodySection.LeftLeg, BodySection.RightLeg, 
                                                               BodySection.All };

    [SerializeField] private Interactable resetAllButtonInteractible;
    [SerializeField] private Interactable resetActiveButtonInteractible;
    [SerializeField] private Interactable viewSystemsButtonInteractible;
    [SerializeField] private Interactable viewSectionsButtonInteractible;
    [SerializeField] private Interactable incrementLayerButtonInteractible;
    [SerializeField] private Interactable decrementLayerButtonInteractible;
    [SerializeField] private Interactable[] bodySystemButtonInteractibles;
    [SerializeField] private Interactable[] bodySectionButtonInteractibles;
    [SerializeField] private TMP_Text curSystemLabel, curLayerLabel, curSectionLabel;
    [SerializeField] private GameObject systemsUI, sectionsUI;
    // Indices need to correspond to the BodySystems
    [SerializeField] private GameObject[] systemButtonBackplates, systemButtonSelectedBackplates;

    private AutopsyBodyPartAssetTypeComponent[] allBodyParts;
    private AutopsyScaleAssetTypeComponent scaleBehavior;

    private List<BodySystem> activeSystems;

    // Indices need to correspond to the BodySystems
    // DO NOT UPDATE DIRECTLY! This variable will be updated when the corresponding asset data is updated!
    private int[] activeLayers = new int[3];

    // Indices need to correspond to the BodySystems
    private int[] totalLayers = new int[3];

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
        //Get a reference to the scale
        AutopsyScaleAssetTypeComponent[] tempScales = new AutopsyScaleAssetTypeComponent[0];
        do
        {
            tempScales = GameObject.FindObjectsByType<AutopsyScaleAssetTypeComponent>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        } while (tempScales.Length == 0);
        scaleBehavior = tempScales[0];

        //Get references to every body part game object
        do
        {
            allBodyParts = GameObject.FindObjectsByType<AutopsyBodyPartAssetTypeComponent>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        } while (allBodyParts.Length == 0);

        Debug.Log("Number of Body Parts: " + allBodyParts.Length);

        foreach (AutopsyBodyPartAssetTypeComponent bodyPart in allBodyParts)
        {
            Debug.Log("Body Part: " + bodyPart.GetBodySection() + " " + bodyPart.GetBodySystem());
        }

        //Set up layers for the body systems
        foreach (AutopsyBodyPartAssetTypeComponent bodyPart in allBodyParts)
        {
            if (totalLayers[(int)(bodyPart.GetBodySystem())] < bodyPart.GetSystemLayer())
            {
                totalLayers[(int)(bodyPart.GetBodySystem())] = bodyPart.GetSystemLayer();
            }
        }

        for (int i = 0; i < totalLayers.Length; i++)
        {
            activeLayers[i] = totalLayers[i];
        }

        //Set up listeners for button functionality
        resetAllButtonInteractible.OnClick.AddListener(ResetAllObjects);
        resetActiveButtonInteractible.OnClick.AddListener(ResetActiveObjects);
        viewSystemsButtonInteractible.OnClick.AddListener(ViewSystems);
        viewSectionsButtonInteractible.OnClick.AddListener(ViewSections);
        incrementLayerButtonInteractible.OnClick.AddListener(IncrementCurrentLayer);
        decrementLayerButtonInteractible.OnClick.AddListener(DecrementCurrentLayer);
        bodySystemButtonInteractibles[0].OnClick.AddListener(ChangeSystemToOuter);
        bodySystemButtonInteractibles[1].OnClick.AddListener(ChangeSystemToInner1);
        bodySystemButtonInteractibles[2].OnClick.AddListener(ChangeSystemToInner2);
        bodySectionButtonInteractibles[0].OnClick.AddListener(ChangeSectionToHead);
        bodySectionButtonInteractibles[1].OnClick.AddListener(ChangeSectionToTorso);
        bodySectionButtonInteractibles[2].OnClick.AddListener(ChangeSectionToLeftArm);
        bodySectionButtonInteractibles[3].OnClick.AddListener(ChangeSectionToRightArm);
        bodySectionButtonInteractibles[4].OnClick.AddListener(ChangeSectionToLeftLeg);
        bodySectionButtonInteractibles[5].OnClick.AddListener(ChangeSectionToRightLeg);
        bodySectionButtonInteractibles[6].OnClick.AddListener(ChangeSectionToAll);


        activeSystems = new List<BodySystem>();
        activeSystems.Add(BodySystem.Outer);
        activeSystems.Add(BodySystem.Inner1);
        activeSystems.Add(BodySystem.Inner2);

        for(int i = 0; i < systemButtonSelectedBackplates.Length; i++)
        {
            if (i == 0)
            {
                systemButtonBackplates[i].SetActive(false);
            }
            else
            {
                systemButtonSelectedBackplates[i].SetActive(false);
            }
        }
        sectionsUI.SetActive(false);
    }

    IEnumerator DelayedLayerNumberAssignment()
    {
        yield return new WaitForEndOfFrame();
        assetData.outerLayer.runtimeData.Value = totalLayers[0];
        assetData.inner1Layer.runtimeData.Value = totalLayers[1];
        assetData.inner2Layer.runtimeData.Value = totalLayers[2];
    }

    protected override void Teardown()
    {
        resetAllButtonInteractible.OnClick.RemoveListener(ResetAllObjects);
        resetActiveButtonInteractible.OnClick.RemoveListener(ResetActiveObjects);
        viewSystemsButtonInteractible.OnClick.RemoveListener(ViewSystems);
        viewSectionsButtonInteractible.OnClick.RemoveListener(ViewSections);
        incrementLayerButtonInteractible.OnClick.RemoveListener(IncrementCurrentLayer);
        decrementLayerButtonInteractible.OnClick.RemoveListener(DecrementCurrentLayer);
        bodySystemButtonInteractibles[0].OnClick.RemoveListener(ChangeSystemToOuter);
        bodySystemButtonInteractibles[1].OnClick.RemoveListener(ChangeSystemToInner1);
        bodySystemButtonInteractibles[2].OnClick.RemoveListener(ChangeSystemToInner2);
        bodySectionButtonInteractibles[0].OnClick.RemoveListener(ChangeSectionToHead);
        bodySectionButtonInteractibles[1].OnClick.RemoveListener(ChangeSectionToTorso);
        bodySectionButtonInteractibles[2].OnClick.RemoveListener(ChangeSectionToRightArm);
        bodySectionButtonInteractibles[3].OnClick.RemoveListener(ChangeSectionToLeftArm);
        bodySectionButtonInteractibles[4].OnClick.RemoveListener(ChangeSectionToLeftLeg);
        bodySectionButtonInteractibles[5].OnClick.RemoveListener(ChangeSectionToRightLeg);
        bodySectionButtonInteractibles[6].OnClick.RemoveListener(ChangeSectionToAll);
    }

    #endregion

    #region Body Part Resetters

    // Resets the position and rotation of all body parts
    public void ResetAllObjects()
    {
        assetData.toggleResetAll.runtimeData.Value = !assetData.toggleResetAll.runtimeData.Value;
    }

    // Resets the position and rotation of only the active body parts
    public void ResetActiveObjects()
    {
        assetData.toggleResetActive.runtimeData.Value = !assetData.toggleResetActive.runtimeData.Value;
    }

    #endregion

    #region Menu Navigation

    //Intentionally made to not network sync
    public void ViewSystems()
    {
        sectionsUI.SetActive(false);
        systemsUI.SetActive(true);
    }

    //Intentionally made to not network sync
    public void ViewSections()
    {
        systemsUI.SetActive(false);
        sectionsUI.SetActive(true);
    }

    public void ChangeSystemToOuter()
    {
        assetData.curSystem.runtimeData.Value = BodySystem.Outer;
    }

    public void ChangeSystemToInner1()
    {
        assetData.curSystem.runtimeData.Value = BodySystem.Inner1;
    }

    public void ChangeSystemToInner2()
    {
        assetData.curSystem.runtimeData.Value = BodySystem.Inner2;
    }

    #endregion

    #region Body Part Visibility Manipulators

    public void ChangeSectionToHead()
    {
        assetData.curSection.runtimeData.Value = (BodySection)0;
    }

    public void ChangeSectionToTorso()
    {
        assetData.curSection.runtimeData.Value = (BodySection)1;
    }

    public void ChangeSectionToLeftArm()
    {
        assetData.curSection.runtimeData.Value = (BodySection)2;
    }

    public void ChangeSectionToRightArm()
    {
        assetData.curSection.runtimeData.Value = (BodySection)3;
    }

    public void ChangeSectionToLeftLeg()
    {
        assetData.curSection.runtimeData.Value = (BodySection)4;
    }

    public void ChangeSectionToRightLeg()
    {
        assetData.curSection.runtimeData.Value = (BodySection)5;
    }

    public void ChangeSectionToAll()
    {
        assetData.curSection.runtimeData.Value = (BodySection)6;
    }

    public void IncrementCurrentLayer()
    {
        if (assetData.notSetLayers.runtimeData.Value)
        {
            assetData.outerLayer.runtimeData.Value = totalLayers[0];
            assetData.inner1Layer.runtimeData.Value = totalLayers[1];
            assetData.inner2Layer.runtimeData.Value = totalLayers[2];
            assetData.notSetLayers.runtimeData.Value = false;
        }
        switch (assetData.curSystem.runtimeData.Value)
        {
            case BodySystem.Outer:
                if (assetData.outerLayer.runtimeData.Value < totalLayers[0])
                {
                    if (assetData.outerLayer.runtimeData.Value == 0)
                    {
                        activeSystems.Add(assetData.curSystem.runtimeData.Value);
                    }
                    assetData.outerLayer.runtimeData.Value += 1;
                    Debug.Log("Detected Change outer is now " + assetData.outerLayer.runtimeData.Value);
                }
                break;
            case BodySystem.Inner1:
                if (assetData.inner1Layer.runtimeData.Value < totalLayers[1])
                {
                    if (assetData.inner1Layer.runtimeData.Value == 0)
                    {
                        activeSystems.Add(assetData.curSystem.runtimeData.Value);
                    }
                    assetData.inner1Layer.runtimeData.Value++;
                }
                break;
            case BodySystem.Inner2:
                if (assetData.inner2Layer.runtimeData.Value < totalLayers[2])
                {
                    if (assetData.inner2Layer.runtimeData.Value == 0)
                    {
                        activeSystems.Add(assetData.curSystem.runtimeData.Value);
                    }
                    assetData.inner2Layer.runtimeData.Value++;
                }
                break;
        }
    }

    public void DecrementCurrentLayer()
    {
        if (assetData.notSetLayers.runtimeData.Value)
        {
            assetData.outerLayer.runtimeData.Value = totalLayers[0];
            assetData.inner1Layer.runtimeData.Value = totalLayers[1];
            assetData.inner2Layer.runtimeData.Value = totalLayers[2];
            assetData.notSetLayers.runtimeData.Value = false;
        }
        switch (assetData.curSystem.runtimeData.Value)
        {
            case BodySystem.Outer:
                if (assetData.outerLayer.runtimeData.Value > 0)
                {
                    if (assetData.outerLayer.runtimeData.Value == 1)
                    {
                        activeSystems.Remove(assetData.curSystem.runtimeData.Value);
                    }
                    assetData.outerLayer.runtimeData.Value -= 1;
                    Debug.Log("Detected Change outer is now " + assetData.outerLayer.runtimeData.Value);
                }
                break;
            case BodySystem.Inner1:
                if (assetData.inner1Layer.runtimeData.Value > 0)
                {
                    if (assetData.inner1Layer.runtimeData.Value == 1)
                    {
                        activeSystems.Remove(assetData.curSystem.runtimeData.Value);
                    }
                    assetData.inner1Layer.runtimeData.Value--;
                }
                break;
            case BodySystem.Inner2:
                if (assetData.inner2Layer.runtimeData.Value > 0)
                {
                    if (assetData.inner2Layer.runtimeData.Value == 1)
                    {
                        activeSystems.Remove(assetData.curSystem.runtimeData.Value);
                    }
                    assetData.inner2Layer.runtimeData.Value--;
                }
                break;
        }
    }

    #endregion

    #region Property Change Handlers


    //[RegisterPropertyChange] makes the function a listener for when a certain specified property changes
    [RegisterPropertyChange(nameof(AutopsyOrganizerAssetData.curSystem))]
    private void OnCurrentSystemChanged(AssetPropertyChangeEventArgs args)
    {
        if (!IsInitialized)
        {
            return;
        }
        BodySystem newSystem = (BodySystem)args.AssetPropertyValue;
        if (newSystem == BodySystem.Outer)
        {
            curSystemLabel.text = "Outer System";
            systemButtonBackplates[0].SetActive(false);
            systemButtonBackplates[1].SetActive(true);
            systemButtonBackplates[2].SetActive(true);
            systemButtonSelectedBackplates[0].SetActive(true);
            systemButtonSelectedBackplates[1].SetActive(false);
            systemButtonSelectedBackplates[2].SetActive(false);
        }
        else if (newSystem == BodySystem.Inner1)
        {
            curSystemLabel.text = "Inner 1 System";
            systemButtonBackplates[0].SetActive(true);
            systemButtonBackplates[1].SetActive(false);
            systemButtonBackplates[2].SetActive(true);
            systemButtonSelectedBackplates[0].SetActive(false);
            systemButtonSelectedBackplates[1].SetActive(true);
            systemButtonSelectedBackplates[2].SetActive(false);
        }
        else if (newSystem == BodySystem.Inner2)
        {
            curSystemLabel.text = "Inner 2 System";
            systemButtonBackplates[0].SetActive(true);
            systemButtonBackplates[1].SetActive(true);
            systemButtonBackplates[2].SetActive(false);
            systemButtonSelectedBackplates[0].SetActive(false);
            systemButtonSelectedBackplates[1].SetActive(false);
            systemButtonSelectedBackplates[2].SetActive(true);
        }
        else
        {
            Debug.Log("An error occurred.");
            curSystemLabel.text = "An error occurred.";
        }
        UpdateLayerLabel();
    }

    [RegisterPropertyChange(nameof(AutopsyOrganizerAssetData.curSection))]
    private void OnCurrentSectionChanged(AssetPropertyChangeEventArgs args)
    {
        if (!IsInitialized)
        {
            return;
        }
        BodySection newSection = (BodySection)args.AssetPropertyValue;
        if (newSection == BodySection.Head)
        {
            curSectionLabel.text = "Head";
        }
        else if (newSection == BodySection.Torso)
        {
            curSectionLabel.text = "Torso";
        }
        else if (newSection == BodySection.LeftArm)
        {
            curSectionLabel.text = "Left Arm";
        }
        else if (newSection == BodySection.RightArm)
        {
            curSectionLabel.text = "Right Arm";
        }
        else if (newSection == BodySection.LeftLeg)
        {
            curSectionLabel.text = "Left Leg";
        }
        else if (newSection == BodySection.RightLeg)
        {
            curSectionLabel.text = "Right Leg";
        }
        else if (newSection == BodySection.All)
        {
            curSectionLabel.text = "All Sections";
        }
        else
        {
            Debug.Log("An error occurred.");
            curSystemLabel.text = "An error occurred.";
        }
        UpdateInteractibleObjects();
    }

    [RegisterPropertyChange(nameof(AutopsyOrganizerAssetData.outerLayer))]
    public void OnOuterLayerChanged(AssetPropertyChangeEventArgs args)
    {
        if (!IsInitialized)
        {
            return;
        }
        if (!assetData.notSetLayers.runtimeData.Value)
        {
            int newValue = (int)args.AssetPropertyValue;
            Debug.Log("Detected change in outer layer");
            activeLayers[0] = newValue;
            UpdateLayerLabel();
            UpdateInteractibleObjects();
        }
    }

    [RegisterPropertyChange(nameof(AutopsyOrganizerAssetData.inner1Layer))]
    public void OnInner1LayerChanged(AssetPropertyChangeEventArgs args)
    {
        if (!IsInitialized)
        {
            return;
        }
        if (!assetData.notSetLayers.runtimeData.Value)
        {
            int newValue = (int)args.AssetPropertyValue;
            Debug.Log("Detected change in inner1 layer");
            activeLayers[1] = newValue;
            UpdateLayerLabel();
            UpdateInteractibleObjects();
        }
    }

    [RegisterPropertyChange(nameof(AutopsyOrganizerAssetData.inner2Layer))]
    public void OnInner2LayerChanged(AssetPropertyChangeEventArgs args)
    {
        if (!IsInitialized)
        {
            return;
        }
        if (!assetData.notSetLayers.runtimeData.Value)
        {
            int newValue = (int)args.AssetPropertyValue;
            Debug.Log("Detected change in inner2 layer");
            activeLayers[2] = newValue;
            UpdateLayerLabel();
            UpdateInteractibleObjects();
        }
    }
    
    [RegisterPropertyChange(nameof(AutopsyOrganizerAssetData.toggleResetAll))]
    public void OnToggleResetAllChanged(AssetPropertyChangeEventArgs args)
    {
        if (!IsInitialized)
        {
            return;
        }
        foreach (AutopsyBodyPartAssetTypeComponent bodyPart in allBodyParts)
        {
            bodyPart.ResetObject();
        }
    }

    [RegisterPropertyChange(nameof(AutopsyOrganizerAssetData.toggleResetActive))]
    public void OnToggleResetActiveChanged(AssetPropertyChangeEventArgs args)
    {
        if (!IsInitialized)
        {
            return;
        }
        foreach (AutopsyBodyPartAssetTypeComponent bodyPart in allBodyParts)
        {
            if ((assetData.curSection.runtimeData.Value == BodySection.All ||
                 assetData.curSection.runtimeData.Value == bodyPart.GetBodySection())
                && activeSystems.Contains(bodyPart.GetBodySystem()))
            {
                bodyPart.ResetObject();
            }
        }
    }

    // Updates the visibility and interactibility of body parts based on the active body
    // systems and body section.
    public void UpdateInteractibleObjects()
    {
        foreach (AutopsyBodyPartAssetTypeComponent bodyPart in allBodyParts)
        {
            if (!bodyPart.gameObject.GetComponent<IsEnabledAssetTypeComponent>().AssetData.isEnabled.runtimeData.Value &&
                (assetData.curSection.runtimeData.Value == BodySection.All ||
                 assetData.curSection.runtimeData.Value == bodyPart.GetBodySection()) &&
                activeSystems.Contains(bodyPart.GetBodySystem()) &&
                activeLayers[(int)(bodyPart.GetBodySystem())] >= bodyPart.GetSystemLayer())
            {
                if (bodyPart.IsOnScaleButInactive())
                {
                    scaleBehavior.AddObjectToScale(bodyPart);
                    bodyPart.SetOnScaleButInactive(false);
                }
                bodyPart.gameObject.GetComponent<IsEnabledAssetTypeComponent>().AssetData.isEnabled.runtimeData.Value = true;
            }
            else if (bodyPart.gameObject.activeSelf && !((assetData.curSection.runtimeData.Value == BodySection.All ||
                     assetData.curSection.runtimeData.Value == bodyPart.GetBodySection()) &&
                     activeSystems.Contains(bodyPart.GetBodySystem()) &&
                     activeLayers[(int)(bodyPart.GetBodySystem())] >= bodyPart.GetSystemLayer()))
            {
                if (scaleBehavior.IsObjectOnScale(bodyPart))
                {
                    scaleBehavior.RemoveObjectFromScale(bodyPart);
                    bodyPart.SetOnScaleButInactive(true);
                }
                bodyPart.gameObject.GetComponent<IsEnabledAssetTypeComponent>().AssetData.isEnabled.runtimeData.Value = false;
            }
        }
    }

    public void UpdateLayerLabel()
    {
        curLayerLabel.text = "Layer: " + activeLayers[(int)(assetData.curSystem.runtimeData.Value)] +
                             "/" + totalLayers[(int)(assetData.curSystem.runtimeData.Value)];
    }

    #endregion
}
