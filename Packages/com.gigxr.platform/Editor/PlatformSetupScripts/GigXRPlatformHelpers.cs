using UnityEditor;
using UnityEngine;

public class GigXRPlatformHelpers : MonoBehaviour
{
    [MenuItem("GIGXR/Apply Recommended PlayerSettings (All Platforms)")]
    public static void SetRecommendedPlayerSettings()
    {
        Debug.Log("[GigXRPlatformHelpers] Applying Recommended Player Settings");

        // -----------------------------------------------------------------------------------------------------------------

        // XR Settings
        // TODO: Better automatic XR setting handling here
        var generalXRSettingsPaths = AssetDatabase.FindAssets
            ("t:XRGeneralSettingsPerBuildTarget", new string[] { "Packages/com.gigxr.platform/XR" });

        if (generalXRSettingsPaths.Length > 0)
        {
            var xrGeneralSettingsObj = AssetDatabase.LoadAssetAtPath<UnityEditor.XR.Management.XRGeneralSettingsPerBuildTarget>
                (generalXRSettingsPaths[0]);

            if (xrGeneralSettingsObj != null)
            {
                var wsaXRGeneralSettings = xrGeneralSettingsObj.SettingsForBuildTarget(BuildTargetGroup.WSA);

                if (wsaXRGeneralSettings) wsaXRGeneralSettings.InitManagerOnStart = true;

                var iosXRGeneralSettings = xrGeneralSettingsObj.SettingsForBuildTarget(BuildTargetGroup.iOS);

                if (iosXRGeneralSettings) iosXRGeneralSettings.InitManagerOnStart = true;

                var androidXRGeneralSettings = xrGeneralSettingsObj.SettingsForBuildTarget(BuildTargetGroup.Android);

                if (androidXRGeneralSettings) androidXRGeneralSettings.InitManagerOnStart = true;
            }
        }

        UnityEditor.XR.ARCore.ARCoreSettings.currentSettings.requirement
            = UnityEditor.XR.ARCore.ARCoreSettings.Requirement.Required;

        UnityEditor.XR.ARCore.ARCoreSettings.currentSettings.depth = UnityEditor.XR.ARCore.ARCoreSettings.Requirement.Required;

        UnityEditor.XR.ARKit.ARKitSettings.currentSettings.requirement = UnityEditor.XR.ARKit.ARKitSettings.Requirement.Required;

        UnityEditor.XR.ARKit.ARKitSettings.currentSettings.faceTracking = false;

        // -----------------------------------------------------------------------------------------------------------------

        // WSA specific build settings
        // Platform Capabilities
        PlayerSettings.WSA.SetCapability(PlayerSettings.WSACapability.WebCam, true);
        PlayerSettings.WSA.SetCapability(PlayerSettings.WSACapability.InternetClient, true);
        PlayerSettings.WSA.SetCapability(PlayerSettings.WSACapability.Microphone, true);
        PlayerSettings.WSA.SetCapability(PlayerSettings.WSACapability.InternetClientServer, true);
        PlayerSettings.WSA.SetCapability(PlayerSettings.WSACapability.PicturesLibrary, true);

        PlayerSettings.WSA.SetCapability(PlayerSettings.WSACapability.PrivateNetworkClientServer, true);

        PlayerSettings.WSA.SetCapability(PlayerSettings.WSACapability.Objects3D, true);
        //PlayerSettings.WSA.SetCapability(PlayerSettings.WSACapability.VoipCall, true);
        PlayerSettings.WSA.SetCapability(PlayerSettings.WSACapability.SpatialPerception, true);
        PlayerSettings.WSA.SetCapability(PlayerSettings.WSACapability.GazeInput, true);

        // WSA Platform Target Family
        PlayerSettings.WSA.SetTargetDeviceFamily(PlayerSettings.WSATargetFamily.Holographic, true);

        // TODO: Fails with "AddAssetToSameFile failed because the other asset is not persistent"
        // var mrSettings = new UnityEditor.XR.WindowsMR.WindowsMRPackageSettings();
        // var wsaSettings = mrSettings.GetBuildSettingsForBuildTargetGroup(BuildTargetGroup.WSA);
        // var runtimeSettings = mrSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.WSA);
        // wsaSettings.UsePrimaryWindowForDisplay = true;
        // wsaSettings.HolographicRemoting = false;
        // runtimeSettings.UseSharedDepthBuffer = true;
        // runtimeSettings.DepthBufferFormat = UnityEngine.XR.WindowsMR.WindowsMRSettings.DepthBufferOption.DepthBuffer24Bit;
        PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.WSA, ApiCompatibilityLevel.NET_4_6);

        // -----------------------------------------------------------------------------------------------------------------

        // Android specific build settings
        PlayerSettings.Android.minSdkVersion    = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel30;
        PlayerSettings.Android.minifyDebug      = false;
        PlayerSettings.Android.minifyRelease    = false;

        PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_4_6);

        // -----------------------------------------------------------------------------------------------------------------

        // iOS specific build settings
        PlayerSettings.iOS.sdkVersion                 = iOSSdkVersion.DeviceSDK;
        PlayerSettings.iOS.cameraUsageDescription     = "Required for augmented reality support";
        PlayerSettings.iOS.microphoneUsageDescription = "Required for speech recognition support";

        PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.iOS, ApiCompatibilityLevel.NET_4_6);

        // -----------------------------------------------------------------------------------------------------------------

        // TODO: Setup Quality Settings levels automatically
    }
}