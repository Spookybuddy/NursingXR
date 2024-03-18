namespace GIGXR.Platform.UI
{
    using GIGXR.Platform.Core.DependencyValidator;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Container MonoBehavior to hold reference between an Image and TextMesh TextField so that they can easily
    /// be swapped out.
    /// </summary>
    [SelectionBase]
    public class LabeledIcon : MonoBehaviour
    {
        // --- Serialized Variables:

        [SerializeField, RequireDependency] 
        private TMPro.TextMeshProUGUI iconTextField;

        [SerializeField, RequireDependency] 
        private Image iconImage;

        // --- Public API:

        public Image IconImage { get { return iconImage; } }

        public void Configure(Sprite newIcon, string iconText)
        {
            iconImage.sprite = newIcon;
            iconTextField.text = iconText;
        }

        public void Configure(LabeledIconScriptableObject labeledIcon)
        {
            if(labeledIcon != null)
            {
                Configure(labeledIcon.iconSprite, labeledIcon.iconName);
            }
            else
            {
                Configure(null, "Icon");
            }
        }

        public void SetIconColor(Color newColor)
        {
            iconImage.color = newColor;
        }
    }
}