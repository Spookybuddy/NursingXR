#if UNITY_EDITOR
using System.IO;

using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

using GIGXR.Platform.Managers;

namespace GIGXR.Platform.Mobile.Utilities
{
    /// <summary>
    /// These are post-build scripts that add required "Frameworks and Libraries" and "Capabilities" to the generated
    /// Xcode project.
    ///
    /// Without this these will need to be added manually after every build for iOS.
    /// </summary>
    public class XcodePostBuildProcessor
    {
        [PostProcessBuild(0)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            if (target != BuildTarget.iOS)
            {
                return;
            }

            var projectPath = PBXProject.GetPBXProjectPath(path);
            var project = new PBXProject();
            project.ReadFromFile(projectPath);
            var unityMainTargetGuid = project.GetUnityMainTargetGuid();
            var unityFrameworkTargetGuid = project.GetUnityFrameworkTargetGuid();

            var profileManager = AssetDatabase.LoadAssetAtPath<MobileProfileScriptableObject>(
                AssetDatabase.GUIDToAssetPath(
                    AssetDatabase.FindAssets(string.Format("t:{0}", typeof(MobileProfileScriptableObject)))[0]
                )
            );

            // Add "Frameworks and Libraries".

            if (profileManager.EnableCloudMessaging)
            {
                project.AddFrameworkToProject(unityFrameworkTargetGuid, "UserNotifications.framework", false);
            }

            project.AddFrameworkToProject(unityFrameworkTargetGuid, "VideoToolbox.framework", false);

            // Disable bitcode (deprecated in XCode 14)

            string targetGuid = project.GetUnityMainTargetGuid();
            project.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");

            targetGuid = project.TargetGuidByName(PBXProject.GetUnityTestTargetName());
            project.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");

            targetGuid = project.GetUnityFrameworkTargetGuid();
            project.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");

            // Save project.pbxproj.
            File.WriteAllText(projectPath, project.WriteToString());

            // Add "Capabilities" to gigxr.entitlements.
            var entitlements = new ProjectCapabilityManager(projectPath, "gigxr.entitlements",
                targetGuid: unityMainTargetGuid);

            if (profileManager.EnableDynamicLinks)
            {
                entitlements.AddAssociatedDomains(new[] { $"applinks:{profileManager.DynamicLinkUrl}" });
            }

            if (profileManager.EnableCloudMessaging)
            {
                entitlements.AddBackgroundModes(BackgroundModesOptions.RemoteNotifications);
                entitlements.AddPushNotifications(true);
            }

            entitlements.WriteToFile();

            // Add compliance information to info.plist
            string plistPath = path + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));
            plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", false); // do we need support here for 3rd party platform devs to feed a "true" in here?
            plist.WriteToFile(plistPath);
        }
    }
}
#endif