using System;
using System.IO;
using System.Reflection;
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

        // Run SetupBuildingProp directly - do NOT catch and ignore exceptions!
        SetupBuildingProp.Build();

        Debug.Log("[BuildAssetBundles] Building AssetBundles for target: " + target);
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
            assetBundleDirectory,
            BuildAssetBundleOptions.None,
            target
        );

        if (manifest == null) throw new Exception("[BuildAssetBundles] Build failed or returned null manifest.");

        Debug.Log("[BuildAssetBundles] Successfully generated bundles:");
        foreach (string b in manifest.GetAllAssetBundles())
        {
            string bPath = Path.Combine(assetBundleDirectory, b);
            long bSize = File.Exists(bPath) ? new FileInfo(bPath).Length : 0;
            Debug.Log("  - " + b + " (" + (bSize / (1024.0 * 1024.0)).ToString("F2") + " MB)");
        }
    }
}
