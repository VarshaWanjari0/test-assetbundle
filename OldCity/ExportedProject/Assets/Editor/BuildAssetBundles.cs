using System;
using System.IO;
using System.Collections.Generic;
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

        // Run SetupOldCity
        SetupOldCity.Build();

        List<string> assetPaths = new List<string>
        {
            "Assets/test/megacity/cm7.prefab",
            "Assets/outsidemods/prefab/rgs.prefab"
        };

        AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
        buildMap[0].assetBundleName = "cm7";
        buildMap[0].assetNames = assetPaths.ToArray();

        Debug.Log("[BuildAssetBundles] Building 'cm7' AssetBundle (target: " + target + ")...");
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
            assetBundleDirectory,
            buildMap,
            BuildAssetBundleOptions.ChunkBasedCompression,
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
