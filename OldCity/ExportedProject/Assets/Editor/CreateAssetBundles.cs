using UnityEditor;
using System.IO;

public class CreateAssetBundles
{
    [MenuItem("Assets/Build AssetBundles (Android)")]
    public static void BuildAndroidAssetBundles()
    {
        string assetBundleDirectory = "AssetBundles/Android";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.Android);
    }

    [MenuItem("Assets/Build AssetBundles (Standalone)")]
    public static void BuildStandaloneAssetBundles()
    {
        string assetBundleDirectory = "AssetBundles/Standalone";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
    }
}
