using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;

#endif

namespace GIGXR.Platform.Utilities
{
    /// <summary>
    /// Utility class for handling singleton ScriptableObjects for data management.
    /// </summary>
    public abstract class ScriptableSingleton<T> : ScriptableObject where T : ScriptableSingleton<T>
    {
        // --- Public Properties:

        private static string FileName { get { return typeof(T).Name; } }

#if UNITY_EDITOR
        private static string AssetPath { get { return "Assets/Resources/Config/" + FileName + ".asset"; } }
#endif

        public static T Instance
        {
            get
            {
                if (cachedInstance == null)
                {
                    // Debug.Log($"Loading {FileName}");
                    cachedInstance = Resources.Load(string.Concat("Config/", FileName)) as T;
                }

                if (cachedInstance == null)
                {
                    Debug.LogWarning($"No instance of {FileName} found, using default values");
                    cachedInstance = CreateInstance<T>();
                }

                return cachedInstance;
            }
        }

        private static T cachedInstance;

#if UNITY_EDITOR
        protected static T CreateAndSave()
        {
            T instance = CreateInstance<T>();
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.delayCall += () => SaveAsset(instance);
            }
            else
            {
                SaveAsset(instance);
            }

            return instance;
        }

        private static void SaveAsset(T obj)
        {
            string dirName = Path.GetDirectoryName(AssetPath);
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            AssetDatabase.CreateAsset(obj, AssetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Saved {FileName} instance");
        }
#endif
    }
}