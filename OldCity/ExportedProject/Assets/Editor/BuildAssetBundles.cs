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

        // Run dynamic setup methods
        InvokeSetupMethod("SetupOldCity");

        Debug.Log("[BuildAssetBundles] Collecting all assets for 'cm7' bundle...");
        List<string> assetPaths = new List<string>();
        foreach (string f in Directory.GetFiles("Assets", "*.*", SearchOption.AllDirectories))
        {
            string norm = f.Replace('\\', '/');
            if (norm.EndsWith(".meta") || norm.EndsWith(".cs") || norm.Contains("AssetBundles/") || norm.EndsWith(".zip")) continue;
            assetPaths.Add(norm);
        }

        AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
        buildMap[0].assetBundleName = "cm7";
        buildMap[0].assetNames = assetPaths.ToArray();

        Debug.Log("[BuildAssetBundles] Building 'cm7' AssetBundle with " + assetPaths.Count + " assets (target: " + target + ")...");
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

    private static void InvokeSetupMethod(string typeName)
    {
        try
        {
            Type t = Type.GetType(typeName);
            if (t == null)
            {
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    t = asm.GetType(typeName);
                    if (t != null) break;
                }
            }

            if (t != null)
            {
                MethodInfo m = t.GetMethod("Build", BindingFlags.Public | BindingFlags.Static);
                if (m != null)
                {
                    Debug.Log("[BuildAssetBundles] Executing " + typeName + ".Build()...");
                    m.Invoke(null, null);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[BuildAssetBundles] Error executing " + typeName + ": " + ex);
        }
    }
}
