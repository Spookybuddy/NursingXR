using GIGXR.Platform.CommonAssetTypes;
using GIGXR.Platform.Core.DependencyInjection;
using GIGXR.Platform.Scenarios;
using GIGXR.Platform.Scenarios.GigAssets;
using GIGXR.Platform.Scenarios.GigAssets.EventArgs;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.WSA;

public enum BodySection { Head, Torso, LeftArm, RightArm, LeftLeg, RightLeg, All };

public enum BodySystem { Outer, Inner1, Inner2 };

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

    private AutopsyBodyPartAssetTypeComponent[] allBodyParts;
    private AutopsyScaleAssetTypeComponent scaleBehavior;
    private BodySystem curSystem = BodySystem.Outer;
    private BodySection curSection = BodySection.All;
    private List<BodySystem> activeSystems;

    // Indices need to correspond to the BodySystems
    private int[] activeLayers = new int[3] { 0, 0, 0 }, totalLayers = new int[3];

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

        sectionsUI.SetActive(false);
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
        foreach (AutopsyBodyPartAssetTypeComponent bodyPart in allBodyParts)
        {
                bodyPart.ResetObject();
        }
    }

    // Resets the position and rotation of only the active body parts
    public void ResetActiveObjects()
    {
        foreach (AutopsyBodyPartAssetTypeComponent bodyPart in allBodyParts)
        {
            if ((curSection == BodySection.All || curSection == bodyPart.GetBodySection())
                && activeSystems.Contains(bodyPart.GetBodySystem()))
            {
                bodyPart.ResetObject();
            }
        }
    }

    #endregion

    #region Menu Navigation

    public void ViewSystems()
    {
        sectionsUI.SetActive(false);
        systemsUI.SetActive(true);
    }

    public void ViewSections()
    {
        systemsUI.SetActive(false);
        sectionsUI.SetActive(true);
    }

    public void ChangeSystemToOuter()
    {
        curSystem = BodySystem.Outer;
        curSystemLabel.text = "Outer System";
        UpdateLayerLabel();
    }

    public void ChangeSystemToInner1()
    {
        curSystem = BodySystem.Inner1;
        curSystemLabel.text = "Inner 1 System";
        UpdateLayerLabel();
    }

    public void ChangeSystemToInner2()
    {
        curSystem = BodySystem.Inner2;
        curSystemLabel.text = "Inner 2 System";
        UpdateLayerLabel();
    }

    #endregion

    #region Body Part Visibility Manipulators

    public void ChangeSectionToHead()
    {
        curSection = (BodySection)0;
        curSectionLabel.text = "Head";
        UpdateInteractibleObjects();
    }

    public void ChangeSectionToTorso()
    {
        curSection = (BodySection)1;
        curSectionLabel.text = "Torso";
        UpdateInteractibleObjects();
    }

    public void ChangeSectionToLeftArm()
    {
        curSection = (BodySection)2;
        curSectionLabel.text = "Left Arm";
        UpdateInteractibleObjects();
    }

    public void ChangeSectionToRightArm()
    {
        curSection = (BodySection)3;
        curSectionLabel.text = "Right Arm";
        UpdateInteractibleObjects();
    }

    public void ChangeSectionToLeftLeg()
    {
        curSection = (BodySection)4;
        curSectionLabel.text = "Left Leg";
        UpdateInteractibleObjects();
    }

    public void ChangeSectionToRightLeg()
    {
        curSection = (BodySection)5;
        curSectionLabel.text = "Right Leg";
        UpdateInteractibleObjects();
    }

    public void ChangeSectionToAll()
    {
        curSection = (BodySection)6;
        curSectionLabel.text = "All Sections";
        UpdateInteractibleObjects();
    }

    public void IncrementCurrentLayer()
    {
        if (activeLayers[(int)curSystem] < totalLayers[(int)curSystem])
        {
            if (activeLayers[(int)curSystem]++ == 0)
            {
                activeSystems.Add(curSystem);
            }
            UpdateLayerLabel();
        }
        UpdateInteractibleObjects();
    }

    public void DecrementCurrentLayer()
    {
        if (activeLayers[(int)curSystem] > 0)
        {
            if (--activeLayers[(int)curSystem] == 0)
            {
                activeSystems.Remove(curSystem);
            }
            UpdateLayerLabel();
        }
        UpdateInteractibleObjects();
    }

    #endregion

    // Updates the visibility and interactibility of body parts based on the active body
    // systems and body section.
    public void UpdateInteractibleObjects()
    {
        foreach (AutopsyBodyPartAssetTypeComponent bodyPart in allBodyParts)
        {
            if (!bodyPart.gameObject.GetComponent<IsEnabledAssetTypeComponent>().AssetData.isEnabled.runtimeData.Value &&
                (curSection == BodySection.All || curSection == bodyPart.GetBodySection()) &&
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
            else if (bodyPart.gameObject.activeSelf && !((curSection == BodySection.All ||
                     curSection == bodyPart.GetBodySection()) &&
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
        curLayerLabel.text = "Layer: " + activeLayers[(int)curSystem] + "/" + totalLayers[(int)curSystem];
    }
}
