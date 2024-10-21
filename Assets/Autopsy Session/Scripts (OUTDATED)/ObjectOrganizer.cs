//--------------------------------------------------------------------------------------
// ObjectOrganizer
// 
// Keeps track of which InteractibleObjects belong to which body system and section,
// displaying only the ones that should be displayed based on the settings in the menu.
//--------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectOrganizer : MonoBehaviour
{
    [SerializeField]
    private GameObject parentOfInteractibles;
    [SerializeField]
    private MenuManager menuManager;
    [SerializeField]
    private ScaleBehavior scaleBehavior;

    private InteractibleObject[] allInteractibles;
    private BodySection curSection = BodySection.All;
    private List<BodySystem> activeSystems;

    // Indices need to correspond to the BodySystems
    private int[] activeLayers = new int[3] {0, 0, 0}, totalLayers = new int[3];

    // Start is called before the first frame update
    void Start()
    {
        activeSystems = new List<BodySystem>();
        activeSystems.Add(BodySystem.Outer);
        activeSystems.Add(BodySystem.Inner1);
        activeSystems.Add(BodySystem.Inner2);
        allInteractibles = parentOfInteractibles.GetComponentsInChildren<InteractibleObject>();
        foreach (InteractibleObject intObj in allInteractibles)
        {
            if (totalLayers[(int)(intObj.GetBodySystem())] < intObj.GetSystemLayer())
            {
                totalLayers[(int)(intObj.GetBodySystem())] = intObj.GetSystemLayer();
            }
        }
        for (int i = 0; i < totalLayers.Length; i++)
        {
            activeLayers[i] = totalLayers[i];
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //--------------------------------------------------------------------------------------
    // Getters and Setters
    //--------------------------------------------------------------------------------------

    public int GetTotalLayers(int index)
    {
        return totalLayers[index];
    }

    public int GetActiveLayer(int index)
    {
        return activeLayers[index];
    }

    public void ChangeActiveSection(int section)
    {
        curSection = (BodySection)section;
        UpdateInteractibleObjects();
        menuManager.UpdateSectionLabel(section);
    }

    public void IncrementLayer(int systemIndex)
    {
        if (activeLayers[systemIndex] < totalLayers[systemIndex])
        {
            if (activeLayers[systemIndex]++ == 0)
            {
                activeSystems.Add((BodySystem)systemIndex);
            }
        }
        UpdateInteractibleObjects();
    }

    public void DecrementLayer(int systemIndex)
    {
        if (activeLayers[systemIndex] > 0)
        {
            if (--activeLayers[systemIndex] == 0)
            {
                activeSystems.Remove((BodySystem)systemIndex);
            }
        }
        UpdateInteractibleObjects();
    }

    // Updates the visibility and interactibility of InteractibleObjects based on the active body
    // systems and body section.
    public void UpdateInteractibleObjects()
    {
        foreach (InteractibleObject intObj in allInteractibles)
        {
            if (!intObj.gameObject.activeSelf && (curSection == BodySection.All ||
                curSection == intObj.GetBodySection()) &&
                activeSystems.Contains(intObj.GetBodySystem()) &&
                activeLayers[(int)(intObj.GetBodySystem())] >= intObj.GetSystemLayer())
            {
                if (intObj.IsOnScaleButInactive())
                {
                    scaleBehavior.AddObjectToScale(intObj);
                    intObj.SetOnScaleButInactive(false);
                }
                intObj.gameObject.SetActive(true);
            }
            else if (intObj.gameObject.activeSelf && !((curSection == BodySection.All ||
                     curSection == intObj.GetBodySection()) &&
                     activeSystems.Contains(intObj.GetBodySystem()) &&
                     activeLayers[(int)(intObj.GetBodySystem())] >= intObj.GetSystemLayer()))
            {
                if (scaleBehavior.IsObjectOnScale(intObj))
                {
                    scaleBehavior.RemoveObjectFromScale(intObj);
                    intObj.SetOnScaleButInactive(true);
                }
                intObj.gameObject.SetActive(false);
            }
        }
    }

    // Resets the position on location of all objects if resetAll is true,
    // otherwise resets the position and locations of only the active objects
    public void ResetObjects(bool resetAll)
    {
        if (!menuManager.InMenuAnimation())
        {
            foreach (InteractibleObject intObj in allInteractibles)
            {
                if (resetAll || ((curSection == BodySection.All || curSection == intObj.GetBodySection())
                                 && activeSystems.Contains(intObj.GetBodySystem())))
                {
                    intObj.ResetObject();
                }
            }
        }
    }
}
