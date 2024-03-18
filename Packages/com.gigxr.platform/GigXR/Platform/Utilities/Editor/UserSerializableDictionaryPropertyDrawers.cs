using GIGXR.Platform.Core;
using GIGXR.Platform.Utilities.SerializableDictionary.Example.Example;
using UnityEditor;
using GIGXR.Platform.Core.Audio;

namespace GIGXR.Platform.Utilities.SerializableDictionary
{
    [CustomPropertyDrawer(typeof(StringStringDictionary))]
    [CustomPropertyDrawer(typeof(ObjectColorDictionary))]
    // [CustomPropertyDrawer(typeof(StringColorArrayDictionary))]
    [CustomPropertyDrawer(typeof(StringObjectDictionary))]
    [CustomPropertyDrawer(typeof(StringGameObjectDictionary))]
    [CustomPropertyDrawer(typeof(EnumObjectDictionary<>))]
    [CustomPropertyDrawer(typeof(GenericEnumIntDictionary<>))]
    [CustomPropertyDrawer(typeof(GenericStringEnumDictionary<>))]
    [CustomPropertyDrawer(typeof(StringScriptableObjectDictionary<>))]
    [CustomPropertyDrawer(typeof(StringListScriptableObjectDictionary<>))]
    [CustomPropertyDrawer(typeof(AnimationEventsCollectionDictionary))]
    [CustomPropertyDrawer(typeof(AudioDataDictionary))]
    [CustomPropertyDrawer(typeof(AudioScenarioCollectionDictionary))]
    [CustomPropertyDrawer(typeof(MappedGIGXRElementDictionary))]
    [CustomPropertyDrawer(typeof(GigVersion))]
    public class UserSerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer
    {
    }

    // [CustomPropertyDrawer(typeof(ColorArrayStorage))]
    // public class AnySerializableDictionaryStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer
    // {
    // }
}