using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using TMPro;

public class TextMeshProTheme : InteractableThemeBase
{
    public override bool IsEasingSupported => false;

    public TextMeshProTheme()
    {
        Types = new Type[] { typeof(TextMeshPro) };
        Name = "TextMeshPro Theme";
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
                        Name = "Text",
                        Type = ThemePropertyTypes.String,
                        Values = new List<ThemePropertyValue>(),
                        Default = new ThemePropertyValue() { String = "" }
                    }
                },
            CustomProperties = new List<ThemeProperty>(),
        };
    }

    /// <inheritdoc />
    public override ThemePropertyValue GetProperty(ThemeStateProperty property)
    {
        ThemePropertyValue start = new ThemePropertyValue();

        if(property.Name == "Text")
        {
            start.String = Host.GetComponent<TextMeshPro>().text;
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
        var textMesh = Host.GetComponent<TextMeshPro>();

        if (property.Name == "Text")
        {
            textMesh.text = value.String;
        }
    }
}