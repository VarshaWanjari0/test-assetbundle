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

        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;

        // Allow overriding build target via command line argument: -buildTarget <TargetName>
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Equals("-buildTarget", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                string targetArg = args[i + 1];
                if (Enum.TryParse(targetArg, true, out BuildTarget parsedTarget))
                {
                    target = parsedTarget;
                    Debug.Log($"[BuildAssetBundles] Build target specified via CLI: {target}");
                }
                else
                {
                    Debug.LogWarning($"[BuildAssetBundles] Failed to parse build target '{targetArg}'. Using default: {target}");
                }
                break;
            }
        }

        Debug.Log($"[BuildAssetBundles] Starting AssetBundle build for target '{target}' into '{assetBundleDirectory}'...");

        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
            assetBundleDirectory,
            BuildAssetBundleOptions.None,
            target
        );

        if (manifest == null)
        {
            string errorMsg = "[BuildAssetBundles] AssetBundle build failed or returned null manifest.";
            Debug.LogError(errorMsg);
            throw new Exception(errorMsg);
        }

        string[] bundles = manifest.GetAllAssetBundles();
        Debug.Log($"[BuildAssetBundles] Successfully built {bundles.Length} bundle(s):");
        foreach (string bundle in bundles)
        {
            Debug.Log($"  - {bundle}");
        }
    }
}
