namespace GIGXR.Platform.Utilities.SerializableDictionary.Example.Example
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class StringStringDictionary : SerializableDictionary<string, string>
    {
    }

    [Serializable]
    public class ObjectColorDictionary : SerializableDictionary<UnityEngine.Object, Color>
    {
    }

    [Serializable]
    public class StringObjectDictionary : SerializableDictionary<string, UnityEngine.Object>
    {
    }

    [Serializable]
    public class StringGameObjectDictionary : SerializableDictionary<string, GameObject>
    {
    }

    // [Serializable]
    // public class ColorArrayStorage : SerializableDictionary.Storage<Color[]>
    // {
    // }
    //
    // [Serializable]
    // public class StringColorArrayDictionary : SerializableDictionary<string, Color[], ColorArrayStorage>
    // {
    // }

    [Serializable]
    public class EnumObjectDictionary<TEnum> : SerializableDictionary<TEnum, UnityEngine.Object>
    {
    }
    
    [Serializable]
    public class GenericEnumIntDictionary<TEnum> : SerializableDictionary<TEnum, int>
    {
    }
    
    [Serializable]
    public class StringString
    {
        public string stringA;
        public string stringB;
    }
    
    // [Serializable]
    // public interface iStringStringInt
    // {
    //     string stringA;
    //     string stringB;
    // }

    [Serializable]
    public abstract class iStringString
    {
        public string stringA;
        public string stringB;
    }
    
    [Serializable]
    public class StringExample : iStringString
    {
        public string stringAb;
        public string stringBb;
    }

    [Serializable]
    public class GenericStringEnumDictionary<TClass> : SerializableDictionary<string, TClass>
    {
    }

    [Serializable]
    public class StringScriptableObjectDictionary<T> : SerializableDictionary<string, T> where T : ScriptableObject
    {
    }

    [Serializable]
    public class StringListScriptableObjectDictionary<T> : SerializableDictionary<string, ScriptableObjectList<T>> where T : ScriptableObject
    {
    }

    [Serializable]
    public class ScriptableObjectList<T> where T : ScriptableObject
    {
        public List<T> scriptableObjectList = new List<T>();
    }

    [Serializable]
    public class MyClass
    {
        public int    i;
        public string str;
    }

    [Serializable]
    public class QuaternionMyClassDictionary : SerializableDictionary<Quaternion, MyClass>
    {
    }
}