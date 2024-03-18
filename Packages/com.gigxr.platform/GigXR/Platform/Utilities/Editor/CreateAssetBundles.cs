#if UNITY_EDITOR

using UnityEditor;

namespace GIGXR.Platform.Utilities
{
    public class CreateAssetBundles
    {
        [MenuItem("Assets/Build AssetBundles")]
        static void BuildAllAssetBundles()
        {
            BuildPipeline.BuildAssetBundles("Assets/AssetBundles/WSA", BuildAssetBundleOptions.None,
                BuildTarget.WSAPlayer);
            //BuildPipeline.BuildAssetBundles("Assets/AssetBundles/iOS", BuildAssetBundleOptions.None, BuildTarget.iOS);
            // BuildPipeline.BuildAssetBundles("Assets/AssetBundles/Android", BuildAssetBundleOptions.None, BuildTarget.Android);
        }
    }
}

#endif // UNITY_EDITOR