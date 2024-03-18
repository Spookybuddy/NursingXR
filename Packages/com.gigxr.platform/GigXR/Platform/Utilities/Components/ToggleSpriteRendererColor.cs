using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ToggleSpriteRendererColor : MonoBehaviour
{
    public Color primaryColor;
    
    public Color secondaryColor;

    private bool toggledOn = false;

    private SpriteRenderer AttachedRenderer
    {
        get
        {
            if(_attachedRenderer == null)
            {
                _attachedRenderer = GetComponent<SpriteRenderer>();
            }

            return _attachedRenderer;
        }
    }

    private SpriteRenderer _attachedRenderer;

    /// <summary>
    /// Called via Unity Editor.
    /// </summary>
    public void ToggleColor()
    {
        toggledOn = !toggledOn;

        if (!toggledOn)
        {
            AttachedRenderer.color = primaryColor;
        }
        else
        {
            AttachedRenderer.color = secondaryColor;
        }
    }

    /// <summary>
    /// Called via Unity Editor.
    /// </summary>
    public void SetState(bool value)
    {
        toggledOn = value;

        if (!toggledOn)
        {
            AttachedRenderer.color = primaryColor;
        }
        else
        {
            AttachedRenderer.color = secondaryColor;
        }
    }
}
