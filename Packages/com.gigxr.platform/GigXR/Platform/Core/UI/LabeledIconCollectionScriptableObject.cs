using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Label Icon Collection", menuName = "GIGXR/ScriptableObjects/LabeledIconCollectionScriptableObject")]
public class LabeledIconCollectionScriptableObject : ScriptableObject
{
    public List<LabeledIconMapping> labeledIconCollection;

    public LabeledIconScriptableObject GetLabelIcon(string iconMapping)
    {
        foreach(var t in labeledIconCollection)
        {
            if(t.iconName == iconMapping)
            {
                return t.iconInfo;
            }
        }

        return null;
    }

    [Serializable]
    public class LabeledIconMapping
    {
        public string iconName;
        public LabeledIconScriptableObject iconInfo;
    }
}
