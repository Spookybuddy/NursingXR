using TMPro;
using GIGXR.Platform.UI;
using GIGXR.Platform.Managers;

namespace GIGXR.Platform.Mobile.UI
{
    /// <summary>
    /// Attach to text objects to configure at runtime from the Profile Manager
    /// </summary>
    public class TextObject : UiObject
    {
        public TextType ThisTextType;

        private TextMeshProUGUI textMeshProUgui;

        private void Awake()
        {
            textMeshProUgui = GetComponent<TextMeshProUGUI>();

            // TODO Improve MobileCompositeRoot lookup
            TextProperties textProperties =
                FindObjectOfType<MobileCompositionRoot>().MobileProfile.MobileText.ReturnTextConfig(ThisTextType);

            textMeshProUgui.font = textProperties.Font;
            textMeshProUgui.fontSize = textProperties.FontSize;
        }
    }
}