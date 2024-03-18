using GIGXR.Platform.HMD.UI;
using TMPro;
using UnityEngine.UI;

public class ButtonElement : GIGXRElement
{
    public TextMeshProUGUI buttonText;

    public Button button;

    public override void SetupData(UiInfo uiInfo)
    {
        if(uiInfo is ButtonInfo info)
        {
            buttonText.text = info.buttonText;

            button.onClick.AddListener(info.buttonAction);
        }
    }
}
