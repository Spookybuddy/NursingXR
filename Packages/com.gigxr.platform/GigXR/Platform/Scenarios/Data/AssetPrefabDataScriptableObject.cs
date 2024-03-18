using System;
using UnityEngine;
using GIGXR.Platform.Utilities.SerializableDictionary.Example.Example;
using UnityEditor;

namespace GIGXR.Platform.ScenarioBuilder.Data
{
    [Serializable]
    public class AssetPrefabDataScriptableObject : ScriptableObject
    {
        public StringGameObjectDictionary assetData = new StringGameObjectDictionary();

        public GameObject GetAsset(string id)
        {
            if (assetData.ContainsKey(id))
            {
                return assetData[id];
            }

            return null;
        }

        public void AddAsset(string id, GameObject gameObject)
        {
            if (!assetData.ContainsKey(id))
            {
                assetData.Add(id, gameObject);
            }
        }

        public void RemoveAsset(string id)
        {
            if (assetData.ContainsKey(id))
            {
                assetData.Remove(id);
            }
        }

        public void ReplaceAsset(string oldId, string newId)
        {
            GameObject gameObject = null;

            // Make sure to remove the key only if it exists
            if (assetData.ContainsKey(oldId))
            {
                assetData.Remove(oldId, out gameObject);
            }

            // Make sure the new ID is not in the dictionary
            if (!assetData.ContainsKey(newId) && gameObject != null)
            {
                assetData.Add(newId, gameObject);
            }
            else
            {
                // TODO Could be more descriptive
                GIGXR.Platform.Utilities.Logger.Warning($"Not able to add {newId}");
            }
        }

        public void DestroyAsset(string id)
        {
#if UNITY_EDITOR
            if (assetData.ContainsKey(id))
            {
                assetData.Remove(id, out GameObject prefabAsset);

                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefabAsset, out string guid, out long file))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    AssetDatabase.DeleteAsset(path);
                }
            }
#endif
        }
    }
}