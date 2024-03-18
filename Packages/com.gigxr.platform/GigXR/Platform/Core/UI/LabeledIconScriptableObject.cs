using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Label Icon", menuName = "GIGXR/ScriptableObjects/LabeledIconScriptableObject")]
public class LabeledIconScriptableObject : ScriptableObject
{
    public string iconName;
    public Sprite iconSprite;
    // Note if scale is set to 0, will use default scale on prefab
    public Vector3 iconScale;
}
