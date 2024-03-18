#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;

namespace PigeonDeliveryTool.Settings
{
    /// <summary>
    ///     Configuration for assets which need to have different
    ///     paths in different build configurations.
    ///     
    ///     E.g., when building for HMD, the path should be
    ///         ParentDirectory/HMDName
    /// </summary>
    [Serializable]
    public class AndroidBuildTypeSpecificPathInfo
    {
        public string ParentDirectory;
        public string HMDName;
        public string APKName;
        public string AABName;
        public string IOSName;

        public string GetName(PigeonBuildConfiguration config)
        {
            switch (config)
            {
                case PigeonBuildConfiguration.HMD:
                    return HMDName;
                case PigeonBuildConfiguration.ANDROID_APK:
                    return APKName;
                case PigeonBuildConfiguration.ANDROID_AAB:
                    return AABName;
                case PigeonBuildConfiguration.IOS:
                    return IOSName;
                default:
                    return "";
            }
        }
    }

    /// <summary>
    ///     Contains AddressableAssetGroup info for a specified asset.
    ///     The asset at path AssetPath belongs in the AddressableAssetGroups
    ///     defined by HMDGroup, APKGroup, etc, depending on the build configuration.
    /// </summary>
    [Serializable]
    public class BuildSpecificAssetGroupInfo
    {
        public string                AssetPath;
        public AddressableAssetGroup HMDGroup;
        public AddressableAssetGroup APKGroup;
        public AddressableAssetGroup AABGroup;
        public AddressableAssetGroup IOSGroup;
        [HideInInspector] public string AssetGuid { get { return AssetDatabase.AssetPathToGUID(AssetPath); } }

        public AddressableAssetGroup GetGroup(PigeonBuildConfiguration config)
        {
            switch (config)
            {
                case PigeonBuildConfiguration.HMD:
                    return HMDGroup;
                case PigeonBuildConfiguration.ANDROID_AAB:
                    return AABGroup;
                case PigeonBuildConfiguration.ANDROID_APK:
                    return APKGroup;
                case PigeonBuildConfiguration.IOS:
                    return IOSGroup;
                default:
                    return null;
            }
        }
    }

    public class PigeonBuildSettings : ScriptableObject
    {
        public static string logTag                    = "[Pigeon] ";
        public static string k_DefaultConfigFolder     = "Assets/Editor/Pigeon";
        public static string k_DefaultConfigObjectName = "PigeonAndroidSettings";

        public static string k_DefaultSettingsPath
        {
            get
            {
                return $"{k_DefaultConfigFolder}/{k_DefaultConfigObjectName}.asset";
            }
        }

        public static PigeonBuildSettings GetSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<PigeonBuildSettings>(k_DefaultSettingsPath);

            if (settings == null)
            {
                settings = CreateInstance<PigeonBuildSettings>();

                if (!AssetDatabase.IsValidFolder(k_DefaultConfigFolder))
                    Directory.CreateDirectory(k_DefaultConfigFolder);

                AssetDatabase.CreateAsset(settings, k_DefaultSettingsPath);
                settings = AssetDatabase.LoadAssetAtPath<PigeonBuildSettings>(k_DefaultSettingsPath);

                AssetDatabase.SaveAssets();
            }

            return settings;
        }

        [HideInInspector]
        public PigeonBuildConfiguration currentConfiguration;

        [SerializeField]
        [Tooltip("The path to the keystore to use, from the project folder.")]
        public string PathToKeystore;

        [SerializeField]
        [Tooltip("The password for the keystore specified above.")]
        public string KeystorePass;

        [SerializeField]
        [Tooltip("The alias to use with the keystore specified above.")]
        public string KeystoreAlias;

        [SerializeField]
        [Tooltip("The password for the alias specified above.")]
        public string KeystoreAliasPass;

        [SerializeField]
        [Tooltip("Assets to be included in different asset groups for different build types.")]
        public List<BuildSpecificAssetGroupInfo> AssetGroupChanges = new List<BuildSpecificAssetGroupInfo>();

        [SerializeField]
        [Tooltip("Directory name changes to make when switching between APK and AAB.")]
        public List<AndroidBuildTypeSpecificPathInfo> PathChanges = new List<AndroidBuildTypeSpecificPathInfo>();
    }

    public enum PigeonBuildConfiguration
    {
        HMD,
        IOS,
        ANDROID_APK,
        ANDROID_AAB
    }
}
#endif