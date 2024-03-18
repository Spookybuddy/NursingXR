using GIGXR.Platform.HMD.UI;
using UnityEngine.UI;

public class ImageElement : GIGXRElement, IElementRetrieve<Image>
{
    public Image image;

    public Image GetElement()
    {
        return image;
    }

    public override void SetupData(UiInfo uiInfo)
    {
        if(uiInfo is ImageInfo imageInfo)
        {
            image.color = imageInfo.color;
        }
    }
}
