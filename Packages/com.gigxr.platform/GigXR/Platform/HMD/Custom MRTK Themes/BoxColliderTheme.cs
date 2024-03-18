using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;

public class BoxColliderTheme : InteractableThemeBase
{
    public override bool IsEasingSupported => false;

    public BoxColliderTheme()
    {
        Types = new Type[] { typeof(BoxCollider) };
        Name = "Box Collider Theme";
    }

    /// <inheritdoc />
    public override ThemeDefinition GetDefaultThemeDefinition()
    {
        return new ThemeDefinition()
        {
            ThemeType = GetType(),
            StateProperties = new List<ThemeStateProperty>()
                {
                    new ThemeStateProperty()
                    {
                        Name = "Center",
                        Type = ThemePropertyTypes.Vector3,
                        Values = new List<ThemePropertyValue>(),
                        Default = new ThemePropertyValue() { Vector3 = Vector3.zero }
                    },
                    new ThemeStateProperty()
                    {
                        Name = "Size",
                        Type = ThemePropertyTypes.Vector3,
                        Values = new List<ThemePropertyValue>(),
                        Default = new ThemePropertyValue() { Vector3 = Vector3.one }
                    }
                },
            CustomProperties = new List<ThemeProperty>(),
        };
    }

    /// <inheritdoc />
    public override ThemePropertyValue GetProperty(ThemeStateProperty property)
    {
        ThemePropertyValue start = new ThemePropertyValue();

        // TODO How to handle multiple properties better?
        if(property.Name == "Center")
        {
            start.Vector2 = Host.GetComponent<BoxCollider>().center;
        }
        else if(property.Name == "Size")
        {
            start.Vector2 = Host.GetComponent<BoxCollider>().size;
        }
        
        return start;
    }

    /// <inheritdoc />
    public override void SetValue(ThemeStateProperty property, int index, float percentage)
    {
        SetValue(property, property.Values[index]);
    }

    /// <inheritdoc />
    protected override void SetValue(ThemeStateProperty property, ThemePropertyValue value)
    {
        var boxCollider = Host.GetComponent<BoxCollider>();

        if (property.Name == "Center")
        {
            boxCollider.center = value.Vector3;
        }
        else if (property.Name == "Size")
        {
            boxCollider.size = value.Vector3;
        }

        // TODO Improve this process that resizes the Near Interactable Touchable so it can still be clicked on
        var nearInteractionTouchable = Host.GetComponent<NearInteractionTouchable>();

        if (nearInteractionTouchable != null)
        {
            // project size to local coordinate system
            Vector2 adjustedSize = new Vector2(
                        Math.Abs(Vector3.Dot(boxCollider.size, nearInteractionTouchable.LocalRight)),
                        Math.Abs(Vector3.Dot(boxCollider.size, nearInteractionTouchable.LocalUp)));

            // Resize helper
            if (adjustedSize != nearInteractionTouchable.Bounds)
            {
                nearInteractionTouchable.SetBounds(adjustedSize);
            }

            // Recenter helper
            if (nearInteractionTouchable.LocalCenter != boxCollider.center + Vector3.Scale(boxCollider.size / 2.0f, nearInteractionTouchable.LocalForward))
            {
                nearInteractionTouchable.SetLocalCenter(boxCollider.center + Vector3.Scale(boxCollider.size / 2.0f, nearInteractionTouchable.LocalForward));
            }
        }
    }
}