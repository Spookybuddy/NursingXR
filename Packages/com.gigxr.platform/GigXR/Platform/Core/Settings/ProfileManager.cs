namespace GIGXR.Platform
{
    using GIGXR.Platform.Core.ScriptableObjects;
    using GIGXR.Platform.Core.Settings;
    using UnityEngine;
#if UNITY_EDITOR
    using Core;
    using UnityEditor;
#endif

    [HelpURL("https://app.clickup.com/8621331/docs/8738k-8540/8738k-6220")]
    public class ProfileManager : ScriptableObject
    {
        public const string ProfileFolderInAssets = "Resources/Config";

        public const string ProfileManagerLocation
            = "Assets/" + ProfileFolderInAssets + "/ProfileManager.asset";

        public NetworkingProfile networkProfile;

        [FeatureFlag(FeatureFlags.Dictation)]
        public DictationProfile dictationProfile;

        public AppDetailsProfile appDetailsProfile;

        public AuthenticationProfile authenticationProfile;

        public FeatureFlagsProfile FeatureFlagsProfile;

        public PerformanceProfile PerformanceProfile;

        public CalibrationProfile CalibrationProfile;

        public StyleProfile StyleProfile;

        public InjectableScriptableObjectCollection injectableScriptableObjects;

#if UNITY_EDITOR
        public static ProfileManager GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<ProfileManager>(ProfileManagerLocation);

            if (settings == null)
            {
                settings = CreateInstance<ProfileManager>();

                System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo
                    ($"{Application.dataPath}/{ProfileFolderInAssets}");

                if (!dirInfo.Exists)
                {
                    dirInfo.Create();
                }

                AssetDatabase.CreateAsset(settings, ProfileManagerLocation);
                AssetDatabase.SaveAssets();
            }

            return settings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
#endif
    }
}