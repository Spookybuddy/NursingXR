using GIGXR.Platform.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Slate : BaseUiObject
{
    #region Editor Set Values

    public float distanceInFrontOfUserMeters = 0.5f;

    public TextMeshPro titleTextMesh;

    public ScrollRect scrollView;

    public RectMask2D contentMask;

    public float slateAccommodation = 0.015f;

    #endregion

    public void Setup(string title)
    {
        ClearContent();

        SetTitle(title);
    }

    /// <summary>
    /// Activates the Slate and positions it where needed.
    /// </summary>
    /// <param name="showTransform">World position for the Slate</param>
    public void Show(Transform showTransform = null, bool inFrontOfUser = false)
    {
        // Make the GameObject itself active
        gameObject.SetActive(true);

        // Place the Slate in front of the user
        if(inFrontOfUser)
        {
            transform.position = Camera.main.transform.position + (Camera.main.transform.forward * distanceInFrontOfUserMeters);
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }
        // Place the Slate at a given location
        else if(showTransform != null)
        {
            gameObject.transform.position = showTransform.position;
            gameObject.transform.rotation = showTransform.rotation;
            
            // Don't position the slate right over the other, but slightly adjusted in front
            gameObject.transform.position -= gameObject.transform.forward * slateAccommodation;
        }
    }

    public void SetTitle(string title)
    {
        titleTextMesh.text = title;
    }

    public void AddContent(GameObject content)
    {
        content.transform.SetParent(scrollView.content, false);
    }

    /// <summary>
    /// Use this method when you want to add content that sits at the same
    /// level as the layout content in the hierarchy. Useful for additional
    /// backgrounds.
    /// </summary>
    public void AddContentAsSibling(GameObject content)
    {
        content.transform.SetParent(scrollView.content.parent, false);

        // For now, we'll assume that objects need to appear under the content
        // list so move this content to the top of the hierarchy
        content.transform.SetAsFirstSibling();
    }

    public void AdjustMaskPadding(Vector4 padding)
    {
        contentMask.padding = padding;
    }

    public void ResetScroll()
    {
        scrollView.content.transform.localPosition = Vector3.zero;
    }

    public void ClearContent()
    {
        for (int n = scrollView.content.childCount - 1; n >= 0; n--)
        {
            Destroy(scrollView.content.GetChild(n).gameObject);
        }
    }

    public void AddSlateButton(string buttonName, UnityAction buttonClick, Transform parent)
    {
        var newButton = UiBuilder.BuildSlateButton(buttonClick: buttonClick, 
            buttonText: buttonName);

        newButton.transform.SetParent(parent, false);
    }

    protected override void SubscribeToEventBuses()
    {
    }
}
