using System;
using System.IO;
using System.Collections.Generic;
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

        Debug.Log("[BuildAssetBundles] Collecting all city assets for target: " + target);
        List<string> assetPaths = new List<string>();
        foreach (string f in Directory.GetFiles("Assets", "*.*", SearchOption.AllDirectories))
        {
            string norm = f.Replace('\\', '/');
            if (norm.EndsWith(".meta") || norm.EndsWith(".cs") || norm.Contains("AssetBundles/")) continue;
            assetPaths.Add(norm);
        }

        AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
        buildMap[0].assetBundleName = "oldcity";
        buildMap[0].assetNames = assetPaths.ToArray();

        Debug.Log("[BuildAssetBundles] Building 'oldcity' bundle with " + assetPaths.Count + " assets (LZMA compressed)...");
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
            assetBundleDirectory,
            buildMap,
            BuildAssetBundleOptions.None,
            target
        );

        if (manifest == null) throw new Exception("[BuildAssetBundles] Build failed or returned null manifest.");

        Debug.Log("[BuildAssetBundles] Successfully generated bundles:");
        foreach (string b in manifest.GetAllAssetBundles())
        {
            string bPath = Path.Combine(assetBundleDirectory, b);
            long bSize = File.Exists(bPath) ? new FileInfo(bPath).Length : 0;
            Debug.Log("  - " + b + " (" + (bSize / 1024.0) + " KB)");
        }
    }
}
