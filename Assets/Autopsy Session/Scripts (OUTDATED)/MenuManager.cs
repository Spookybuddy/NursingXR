using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private GameObject freeCamButtons, systemsButtons, sectionsButtons;
    [SerializeField]
    private RectTransform menuBackground;
    [SerializeField]
    private TMPro.TextMeshProUGUI toggleMenuText, systemLabelText, layerLabelText,
                                  sectionLabelText;
    [SerializeField]
    private ObjectOrganizer objectOrganizer;

    private bool inMenuAnim = false, isMenuOpen = false;
    private BodySystem curSystem = BodySystem.Outer;

    // Start is called before the first frame update
    void Start()
    {
        sectionsButtons.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //--------------------------------------------------------------------------------------
    // Getters and Setters
    //--------------------------------------------------------------------------------------

    public bool InMenuAnimation()
    {
        return inMenuAnim;
    }

    public bool IsMenuOpen()
    {
        return isMenuOpen;
    }

    // Opens and closes the menu
    public void ToggleMenu()
    {
        if (!inMenuAnim)
        {
            inMenuAnim = true;
            if (isMenuOpen)
            {
                isMenuOpen = false;
                StartCoroutine(CloseMenu());
            }
            else
            {
                freeCamButtons.SetActive(false);
                StartCoroutine(OpenMenu());
            }
        }
    }

    // Coroutine for animating the menu opening
    IEnumerator OpenMenu()
    {
        for (int i = 1; i <= 30; i++)
        {
            yield return new WaitForSeconds(1 / 60f);
            menuBackground.anchoredPosition = new Vector2(700 * Mathf.Sin(Mathf.PI * i / 60) - 350,
                                                          menuBackground.anchoredPosition.y);
        }
        menuBackground.anchoredPosition = new Vector2(350, menuBackground.anchoredPosition.y);
        isMenuOpen = true;
        toggleMenuText.text = "‹";
        inMenuAnim = false;
    }

    // Coroutine for animating the menu closing
    IEnumerator CloseMenu()
    {
        for (int i = 1; i <= 30; i++)
        {
            yield return new WaitForSeconds(1 / 60f);
            menuBackground.anchoredPosition = new Vector2(700 * Mathf.Sin(-Mathf.PI * i / 60) + 350,
                                                          menuBackground.anchoredPosition.y);
        }
        menuBackground.anchoredPosition = new Vector2(-350, menuBackground.anchoredPosition.y);
        toggleMenuText.text = "›";
        inMenuAnim = false;
        freeCamButtons.SetActive(true);
    }

    public void ViewSystems()
    {
        sectionsButtons.SetActive(false);
        systemsButtons.SetActive(true);
    }

    public void ViewSections()
    {
        systemsButtons.SetActive(false);
        sectionsButtons.SetActive(true);
    }

    public void ChangeSelectedSystem(int systemIn)
    {
        switch (systemIn)
        {
            case 0:
                curSystem = BodySystem.Outer;
                systemLabelText.text = "Outer System";
                layerLabelText.text = "Layer: " + objectOrganizer.GetActiveLayer(0) + "/" +
                                      objectOrganizer.GetTotalLayers(0);
                break;
            case 1:
                curSystem = BodySystem.Inner1;
                systemLabelText.text = "Inner 1 System";
                layerLabelText.text = "Layer: " + objectOrganizer.GetActiveLayer(1) + "/" +
                                      objectOrganizer.GetTotalLayers(1);
                break;
            case 2:
                curSystem = BodySystem.Inner2;
                systemLabelText.text = "Inner 2 System";
                layerLabelText.text = "Layer: " + objectOrganizer.GetActiveLayer(2) + "/" +
                                      objectOrganizer.GetTotalLayers(2);
                break;
        }
    }

    public void IncrementCurrentLayer()
    {
        objectOrganizer.IncrementLayer((int)curSystem);
        UpdateLayerLabel();
    }

    public void DecrementCurrentLayer()
    {
        objectOrganizer.DecrementLayer((int)curSystem);
        UpdateLayerLabel();
    }

    public void UpdateLayerLabel()
    {
        layerLabelText.text = "Layer: " + objectOrganizer.GetActiveLayer((int)curSystem) + "/" +
                              objectOrganizer.GetTotalLayers((int)curSystem);
    }

    public void UpdateSectionLabel(int sectionIndex)
    {
        switch (sectionIndex)
        {
            case 0:
                sectionLabelText.text = "Head";
                break;
            case 1:
                sectionLabelText.text = "Torso";
                break;
            case 2:
                sectionLabelText.text = "LeftArm";
                break;
            case 3:
                sectionLabelText.text = "RightArm";
                break;
            case 4:
                sectionLabelText.text = "LeftLeg";
                break;
            case 5:
                sectionLabelText.text = "RightLeg";
                break;
            case 6:
                sectionLabelText.text = "All Sections";
                break;
        }
    }
}
