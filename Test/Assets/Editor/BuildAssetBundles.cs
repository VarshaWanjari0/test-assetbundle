using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildAssetBundles
{
    [MenuItem("Build/Build AssetBundles")]
    public static void BuildAllBundles()
    {
        string assetBundleDirectory = Path.Combine("Assets", "AssetBundles");
        if (!Directory.Exists(assetBundleDirectory)) Directory.CreateDirectory(assetBundleDirectory);

        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Equals("-buildTarget", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                if (Enum.TryParse(args[i + 1], true, out BuildTarget parsedTarget))
                {
                    target = parsedTarget;
                }
                break;
            }
        }

        try
        {
            var setupMethod = typeof(SetupCarMod).GetMethod("Build", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (setupMethod != null)
            {
                Debug.Log("[BuildAssetBundles] Executing SetupCarMod.Build()...");
                setupMethod.Invoke(null, null);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[BuildAssetBundles] Note on SetupCarMod: " + ex.Message);
        }

        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
            assetBundleDirectory,
            BuildAssetBundleOptions.None,
            target
        );

        if (manifest == null) throw new Exception("[BuildAssetBundles] Build failed or null manifest.");
        Debug.Log("[BuildAssetBundles] Build completed successfully for " + target);
    }
}
