#if UNITY_EDITOR
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.AddressableAssets.ResourceProviders;
using UnityEngine.AddressableAssets;

// using AddressablesPlayAssetDelivery; // unused in this project until we get Android working

namespace GIGXR.Platform.Pigeon.Editor
{
    using PigeonDeliveryTool.Settings;
    using System;
    using System.Threading;
    using UnityEditor;
    using UnityEditor.AddressableAssets.Build;
    using UnityEditor.AddressableAssets.Settings;
    using UnityEngine;

    /// <summary>
    /// Tools to automate delivery configuration in the Unity editor for various platforms.
    /// </summary>
    public static class ConfigureBuildSettings
    {
    #region Dependencies

        private static ProfileManager _profileManager;

        private static ProfileManager ProfileManager
        {
            get
            {
                if (_profileManager == null)
                {
                    _profileManager = AssetDatabase.LoadAssetAtPath<ProfileManager>(ProfileManager.ProfileManagerLocation);
                }

                return _profileManager;
            }
        }

    #endregion

        private static PigeonBuildSettings settings;

        private static PigeonBuildSettings Settings
        {
            get
            {
                if (settings == null)
                {
                    settings = PigeonBuildSettings.GetSettings();
                }

                return settings;
            }
        }

        static string LogTag
        {
            get { return PigeonDeliveryTool.Settings.PigeonBuildSettings.logTag; }
        }

        /// <summary>
        /// Added to support automated addressable builds.
        /// Cleans build cache as well. 
        /// </summary>
        [MenuItem("GIGXR/Pigeon Delivery Tool/Build Addressables")]
        public static void BuildAddressables()
        {
            bool result = BuildAddressableContent();

            if (result == false)
            {
                throw new Exception("Failed");
            }
        }

        /// <summary>
        /// Stole this from addressable code somewhere. 
        /// </summary>
        /// <returns></returns>
        static bool BuildAddressableContent()
        {
            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
            bool success = string.IsNullOrEmpty(result.Error);

            if (!success)
            {
                Debug.LogError("Addressables build error encountered: " + result.Error);
            }

            return success;
        }

        /// <summary>
        /// TODO - requires Google Play Asset Delivery 
        /// </summary>
        // [MenuItem("GIGXR/Pigeon Delivery Tool/Configure Build/Android/AAB (Production GMS)")]
        public static void Configure_AAB()
        {
            ShowProgressBar("Switching Android configuration to AAB...");

            // set scripting backend, architecture, build config
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);

            PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Android, Il2CppCompilerConfiguration.Master);

            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.All;

            // set SDK versions
            PlayerSettings.Android.minSdkVersion    = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel30;

            // apply signing settings
            if (!string.IsNullOrEmpty(Settings.PathToKeystore))
            {
                PlayerSettings.Android.useCustomKeystore = true;
                PlayerSettings.Android.keystoreName      = Settings.PathToKeystore;
                PlayerSettings.Android.keystorePass      = Settings.KeystorePass;
                PlayerSettings.Android.keyaliasName      = Settings.KeystoreAlias;
                PlayerSettings.Android.keyaliasPass      = Settings.KeystoreAliasPass;
            }
            else
            {
                Debug.LogWarning(LogTag + "Keystore information is missing; build will not be correctly signed.");
            }

            // target production (assumes we're making an AAB to ship); change to QA afterward if desired
            TargetGmsProd();

            // configure settings for splitting app into packs for play store
            PlayerSettings.Android.useAPKExpansionFiles          = true;
            EditorUserBuildSettings.buildAppBundle               = true;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
            EditorUserBuildSettings.development                  = false;
            EditorUserBuildSettings.allowDebugging               = false;

            // apply changes to directory names and asset groups
            RemoveAssetsFromGroups();
            RenameAssets(PigeonDeliveryTool.Settings.PigeonBuildConfiguration.ANDROID_AAB);
            AssignAssetGroups(PigeonDeliveryTool.Settings.PigeonBuildConfiguration.ANDROID_AAB);

            Settings.currentConfiguration = PigeonDeliveryTool.Settings.PigeonBuildConfiguration.ANDROID_AAB;

            // I don't think we can do this TODO below; the app version and bundle version are read-only -RM
            // TODO: Manual steps - these may be automated later but they require special app info
            // - Increment app version
            // - Increment the bundle version code

            // AddressablesInitSingleton.Instance.IsUsingAssetBundles = true; // needs google play
        }

        /// <summary>
        /// TODO Requires Google Play Asset Delivery. 
        /// </summary>
        // [MenuItem("GIGXR/Pigeon Delivery Tool/Configure Build/Android/APK (QA GMS)")]
        public static void Configure_APK()
        {
            ShowProgressBar("Switching Android configuration to APK...");

            // set scripting backend, architecture, build config
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);

            PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Android, Il2CppCompilerConfiguration.Release);

            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            // set SDK versions
            PlayerSettings.Android.minSdkVersion    = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel30;

            PlayerSettings.Android.useCustomKeystore = false;
            PlayerSettings.Android.keystoreName      = "";
            PlayerSettings.Android.keystorePass      = "";
            PlayerSettings.Android.keyaliasName      = "";
            PlayerSettings.Android.keyaliasPass      = "";

            // target QA; we only use apks for testing
            TargetGmsQa();

            // configure to include all assets in 1 apk
            PlayerSettings.Android.useAPKExpansionFiles          = false;
            EditorUserBuildSettings.buildAppBundle               = false;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            EditorUserBuildSettings.development                  = false;
            EditorUserBuildSettings.allowDebugging               = false;

            // apply changes to directory names and asset groups
            RemoveAssetsFromGroups();
            RenameAssets(PigeonDeliveryTool.Settings.PigeonBuildConfiguration.ANDROID_APK);
            AssignAssetGroups(PigeonDeliveryTool.Settings.PigeonBuildConfiguration.ANDROID_APK);

            Settings.currentConfiguration = PigeonDeliveryTool.Settings.PigeonBuildConfiguration.ANDROID_APK;

            // AddressablesInitSingleton.Instance.IsUsingAssetBundles = false; // needs google play
        }

        /// <summary>
        /// Configures HMD Addressables settings.
        /// </summary>
        // TODO expand below to cover all settings for test/prod HMD and iOS builds (and rename menu items to match)
        //
        //[MenuItem("GIGXR/Pigeon Delivery Tool/Configure Build/Reset/Reset Addressables for HMD")]
        public static void Configure_HMD()
        {
            ShowProgressBar("Resetting Addressables for HMD");
            RemoveAssetsFromGroups();
            RenameAssets(PigeonDeliveryTool.Settings.PigeonBuildConfiguration.HMD);
            AssignAssetGroups(PigeonDeliveryTool.Settings.PigeonBuildConfiguration.HMD);

            Settings.currentConfiguration = PigeonDeliveryTool.Settings.PigeonBuildConfiguration.HMD;
        }

        /// <summary>
        /// Configures IOS addressables settings.
        /// </summary>
        //[MenuItem("GIGXR/Pigeon Delivery Tool/Configure Build/Reset/Reset Addressables for IOS")]
        public static void Configure_IOS()
        {
            ShowProgressBar("Resetting Addressables for IOS");
            RemoveAssetsFromGroups();
            RenameAssets(PigeonDeliveryTool.Settings.PigeonBuildConfiguration.IOS);
            AssignAssetGroups(PigeonDeliveryTool.Settings.PigeonBuildConfiguration.IOS);

            Settings.currentConfiguration = PigeonBuildConfiguration.IOS;
        }
        
        /// <summary>
        /// For scripts that modify ProfileManager.
        /// </summary>
        private static void SaveChangesToProfileManager()
        {
            EditorUtility.SetDirty(ProfileManager);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Sets the target GMS to QA in Profile Manager.
        /// </summary>
        [MenuItem("GIGXR/Pigeon Delivery Tool/Set Target GMS/QA")]
        public static void TargetGmsQa()
        {
            ShowProgressBar("Targeting QA GMS...");
            ProfileManager.authenticationProfile.SetTargetGMS(ProfileManager.authenticationProfile.GetEnvironmentByName("QA"));
            SaveChangesToProfileManager();
        }

        /// <summary>
        /// Sets the target GMS to Production (PR) in Profile Manager.
        /// </summary>
        [MenuItem("GIGXR/Pigeon Delivery Tool/Set Target GMS/PR")]
        public static void TargetGmsProd()
        {
            ShowProgressBar("Targeting Production GMS...");

            ProfileManager.authenticationProfile.SetTargetGMS
                (ProfileManager.authenticationProfile.GetEnvironmentByName("Production"));

            SaveChangesToProfileManager();
        }

        /// <summary>
        /// Displays Android build settings.
        /// </summary>
        [MenuItem("GIGXR/Pigeon Delivery Tool/View Build Settings/Android")]
        public static void ShowAndroidBuildSettings()
        {
            AssetDatabase.OpenAsset(Settings);
        }

        /// <summary>
        ///     Rename assets to switch from the current build config to the
        ///     one specified by newConfig. See overload.
        /// </summary>
        private static void RenameAssets(PigeonDeliveryTool.Settings.PigeonBuildConfiguration newConfig)
        {
            RenameAssets(Settings.currentConfiguration, newConfig);
        }

        /// <summary>
        ///     Rename assets as specified to move from previousConfig to newConfig.
        ///     Used primarily to rename specified Resources <=> _Resources 
        ///     directories in HH2, for ommission from the main APK in AAB builds and
        ///     inclusion in Resources in other builds.
        /// </summary>
        private static void RenameAssets
        (
            PigeonDeliveryTool.Settings.PigeonBuildConfiguration previousConfig,
            PigeonDeliveryTool.Settings.PigeonBuildConfiguration newConfig
        )
        {
            foreach (PigeonDeliveryTool.Settings.AndroidBuildTypeSpecificPathInfo pathInfo in Settings.PathChanges)
            {
                string parentDirectory = pathInfo.ParentDirectory;
                string oldName         = pathInfo.GetName(previousConfig);
                string newName         = pathInfo.GetName(newConfig);

                RenameAsset
                    (
                        parentDirectory,
                        oldName,
                        newName
                    );
            }
        }

        /// <summary>
        ///     Rename a specified asset or directory. Used primarily to rename specified
        ///     Resources <=> _Resources folders in HH2, for ommission from the main APK in
        ///     AAB builds.
        /// </summary>
        /// <param name="parentDirectory">
        ///     The directory containing the asset or directory to be renamed.
        /// </param>
        /// <param name="oldName">
        ///     The original name of the asset to be renamed (its subpath under parentDirectory before renaming)
        /// </param>
        /// <param name="newName">
        ///     The new name of the asset (its subpath under parentDirectory after renaming)
        /// </param>
        private static void RenameAsset
        (
            string parentDirectory,
            string oldName,
            string newName
        )
        {
            if (oldName == newName) return;
            parentDirectory = parentDirectory + "/";
            string oldPath = parentDirectory + oldName;
            string newPath = parentDirectory + newName;

            if (AssetDatabase.IsValidFolder(oldPath))
            {
                Debug.Log(LogTag + "Moving asset... original folder exists.");

                if (AssetDatabase.IsValidFolder(newPath))
                {
                    Debug.LogError(LogTag + "Cannot move asset; target path already exists.");
                }
                else
                {
                    AssetDatabase.MoveAsset(oldPath, newPath);
                    // AssetDatabase.DeleteAsset(oldPath);
                    Debug.Log(LogTag + "Moving asset... success!");
                }
            }
            else
            {
                if (AssetDatabase.IsValidFolder(newPath))
                {
                    Debug.LogWarning
                        (LogTag + "Skipping moving asset; it was already moved or another asset exists at the target location.");
                }
                else
                {
                    Debug.LogError(LogTag + "Cannot move asset; neither original nor target path exist :");
                }
            }
        }

        /// <summary>
        ///     Go through all assets which have been configured to appear in different
        ///     AddressableAssetGroups depending on the build configuration.
        ///     Remove them each from all addressable asset groups.
        ///     Used when leaving one build configuration, before adding the assets in
        ///     question to nother AddressableAssetGroups when entering a new build configuration.
        /// </summary>
        private static void RemoveAssetsFromGroups()
        {
            foreach (PigeonDeliveryTool.Settings.BuildSpecificAssetGroupInfo assetGroupInfo in Settings.AssetGroupChanges)
            {
                RemoveAssetFromGroups(assetGroupInfo);
            }
        }

        /// <summary>
        ///     Remove the asset specified in groupInfo from all AddressableAssetGroups.
        ///     Used when leaving one build configuration, before adding the asset in
        ///     question to another AddressableAssetGroup when entering a new build configuration.
        /// </summary>
        /// <param name="groupInfo"></param>
        private static void RemoveAssetFromGroups(PigeonDeliveryTool.Settings.BuildSpecificAssetGroupInfo groupInfo)
        {
            groupInfo?.HMDGroup?.Settings.RemoveAssetEntry(groupInfo.AssetGuid);
            groupInfo?.AABGroup?.Settings.RemoveAssetEntry(groupInfo.AssetGuid);
            groupInfo?.APKGroup?.Settings.RemoveAssetEntry(groupInfo.AssetGuid);
            groupInfo?.IOSGroup?.Settings.RemoveAssetEntry(groupInfo.AssetGuid);
        }

        /// <summary>
        ///     Go through all of the assets which are contained in different AddressableAssetGroups
        ///     across different platforms / build configs. Assign these assets to the correct
        ///     AddressableAssetGroup.
        /// </summary>
        /// <param name="config">
        ///     The PigeonBuildConfiguration to switch to. Assets will be moved to
        ///     AddressableAssetGroups as assigned in this build configuration.
        /// </param>
        private static void AssignAssetGroups(PigeonDeliveryTool.Settings.PigeonBuildConfiguration config)
        {
            foreach (PigeonDeliveryTool.Settings.BuildSpecificAssetGroupInfo assetGroupInfo in Settings.AssetGroupChanges)
            {
                string                assetGuid   = assetGroupInfo.AssetGuid;
                AddressableAssetGroup targetGroup = assetGroupInfo.GetGroup(config);
                AssignAssetGroup(assetGuid, targetGroup);
            }
        }

        /// <summary>
        ///     Assign the asset with the specified guid to the referenced AddressableAssetGroup
        /// </summary>
        /// <param name="assetGuid">
        ///     The asset to be assigned to an AddressableAssetGroup.
        /// </param>
        /// <param name="targetGroup">
        ///     The AddressableAssetGroup in which to place the asset specified by assetGuid.
        /// </param>
        private static void AssignAssetGroup(string assetGuid, AddressableAssetGroup targetGroup)
        {
            targetGroup?.Settings.CreateOrMoveEntry(assetGuid, targetGroup);
        }

        /// <summary>
        /// Shows a progress bar for visual feedback.
        /// </summary>
        /// <param name="message"></param>
        private static void ShowProgressBar(string message)
        {
            float seconds = 1.5f;
            var   step    = 0.1f;

            for (float t = 0; t < seconds; t += step)
            {
                EditorUtility.DisplayProgressBar
                    (
                        "Pigeon Delivery Tool",
                        message,
                        t / seconds
                    );

                Thread.Sleep((int)(step * 1000.0f));
            }

            EditorUtility.ClearProgressBar();
        }
    }
}
#endif