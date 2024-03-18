using GIGXR.Platform.HMD.UI;
using UnityEngine;

public abstract class GIGXRElement : MonoBehaviour
{
    public abstract void SetupData(UiInfo uiInfo);
}

public interface IElementRetrieve<T>
{
    T GetElement();
}