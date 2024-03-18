using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectTransformTheme : InteractableThemeBase
{
    public override bool IsEasingSupported => false;

    public RectTransformTheme()
    {
        Types = new Type[] { typeof(RectTransform) };
        Name = "RectTransform Theme";
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
                        Name = "RectTransform",
                        Type = ThemePropertyTypes.Vector2,
                        Values = new List<ThemePropertyValue>(),
                        Default = new ThemePropertyValue() { Vector2 = new Vector2(0, 0) }
                    },
                },
            CustomProperties = new List<ThemeProperty>(),
        };
    }

    /// <inheritdoc />
    public override ThemePropertyValue GetProperty(ThemeStateProperty property)
    {
        ThemePropertyValue start = new ThemePropertyValue();
        start.Vector2 = Host.GetComponent<RectTransform>().sizeDelta;
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
        Host.GetComponent<RectTransform>().sizeDelta = value.Vector2;
    }
}
