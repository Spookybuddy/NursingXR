using GIGXR.Platform.Utilities.SerializableDictionary.Example;
using GIGXR.Platform.Utilities.SerializableDictionary.Example.Example;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerializableDictionaryExample : MonoBehaviour
{
    // The dictionaries can be accessed throught a property
    [SerializeField]
    StringStringDictionary m_stringStringDictionary;

    public IDictionary<string, string> StringStringDictionary
    {
        get { return m_stringStringDictionary; }
        set { m_stringStringDictionary.CopyFrom(value); }
    }

    void Reset()
    {
        // access by property
        StringStringDictionary = new Dictionary<string, string>()
        {
            { "first key", "value A" }, { "second key", "value B" }, { "third key", "value C" }
        };

        // m_Something5 = new QuaternionMyClassDictionary() { { Quaternion.identity, new MyClass() } };
        // m_objectColorDictionary = new ObjectColorDictionary() { { gameObject, Color.blue }, { this, Color.red } };
    }
}