using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using GIGXR.Platform.Utilities.SerializableDictionary;
using GIGXR.Platform.HMD.UI;

[Serializable]
public class MappedGIGXRElementDictionary : SerializableDictionary<string, GIGXRElement>
{
}

public class OmniUIConnection : MonoBehaviour
{
    #region Editor Set Values

    public MappedGIGXRElementDictionary mappedElements;

    #endregion

    private LayoutElement layoutElement;

    private void Awake()
    {
        layoutElement = GetComponent<LayoutElement>();
    }

    public void Setup(string key, UiInfo info)
    {
        if(mappedElements.ContainsKey(key))
        {
            mappedElements[key].SetupData(info);
        }
        else
        {
            Debug.LogWarning($"[OmniUiConnection] {name} does not have key {key}.");
        }
    }

    public void SetPreferredSize(Vector2 size)
    {
        if(layoutElement != null)
        {
            layoutElement.preferredWidth = size.x;
            layoutElement.preferredHeight = size.y;
        }
    }

    public TextMeshProUGUI GetTextObject(string key)
    {
        if (mappedElements.ContainsKey(key))
        {
            return ((IElementRetrieve<TextMeshProUGUI>)mappedElements[key]).GetElement();
        }

        return null;
    }
}