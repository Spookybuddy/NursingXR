using UnityEngine.UI;
using GIGXR.Platform.UI;

namespace GIGXR.Platform.Mobile.UI
{
    /// <summary>
    /// Attach to Image Objects to configure image properties at runtime from the Profile Manager
    /// </summary>
    public class ImageObject : UiObject
    {
        /// <summary>
        /// The type of this Image
        /// </summary>
        public ImageType ThisImageType;

        /// <summary>
        /// Reference to the image for manipulation
        /// </summary>
        private Image thisImage;

        private void Awake()
        {
            thisImage = GetComponent<Image>();

            // TODO Improve the lookup to the composition root
            ImageProperties imageProperties = FindObjectOfType<MobileCompositionRoot>().MobileProfile.MobileImage.ReturnImageConfig(ThisImageType);

            thisImage.color = imageProperties.Color;
        }
    }
}