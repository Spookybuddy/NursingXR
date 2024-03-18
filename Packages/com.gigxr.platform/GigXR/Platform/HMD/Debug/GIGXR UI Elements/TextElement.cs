using GIGXR.Platform.HMD.UI;
using TMPro;

public class TextElement : GIGXRElement, IElementRetrieve<TextMeshProUGUI>
{
    public TextMeshProUGUI text;

    public TextMeshProUGUI GetElement()
    {
        return text;
    }

    public override void SetupData(UiInfo uiInfo)
    {
        if(uiInfo is TextInfo info)
        {
            text.text = info.text;

            // If no value is provided, then the value in the Prefab that is already associated with
            // the TextMeshProUGUI text will be used.
            if (info.fontSize.HasValue)
            {
                text.fontSize = info.fontSize.Value;
            }

            if(info.textAlignment.HasValue)
            {
                text.alignment = info.textAlignment.Value;
            }

            if(info.textColor.HasValue)
            {
                text.color = info.textColor.Value;
            }
        }
    }
}
