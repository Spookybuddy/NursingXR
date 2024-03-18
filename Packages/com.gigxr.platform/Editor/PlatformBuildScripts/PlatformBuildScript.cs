using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using GIGXR.Platform.Core.DependencyValidator;

class BuildAddressablesProcessor
{
    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        BuildPlayerWindow.RegisterBuildPlayerHandler(BuildAddressablesPrompt);
    }



    static public void AddressablesBuild()
    {
        Debug.Log($"[PlatformBuildScript] Building Addressables | start");

        AddressableAssetSettings.CleanPlayerContent(
            AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);

        AddressableAssetSettings.BuildPlayerContent();


        Debug.Log($"[PlatformBuildScript] Building Addressables | done");
    }



    private static void BuildAddressablesPrompt(BuildPlayerOptions options)
    {
        var dialogTitle = "Build Pipeline Prompt";
        var dialogText = "Do you want to rebuild addressables before building the player?";
        var confirmText = "Update addressables with build";
        var denyText = "Skip";

        if (EditorUtility.DisplayDialog(dialogTitle, dialogText, confirmText, denyText))
        {
            try
            {
                RequireDependencyEditorValidator.ValidateAddressablesForBuild();
            }
            catch (UnityEditor.Build.BuildFailedException e)
            {
                Debug.LogError(e.Message);
                throw e;
            }
            
            AddressablesBuild();
        }
        BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
    }

}