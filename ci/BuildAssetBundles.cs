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

        // Project-specific builders
        InvokeMethod("SetupBuildingProp", "Build");
        InvokeMethod("SetupBatmobileMod", "Build");
        InvokeMethod("CyberTruckBuilder", "BuildBundle");
        InvokeMethod("CybertruckModBuilder", "BuildModAndroid");
        InvokeMethod("CybertruckModBuilder", "Build");
        InvokeMethod("HypercarModBuilder", "BuildAll");

        // Standard AssetBundle build — runs AFTER project-specific builders
        // so the hypercar body is already attached to rgs.prefab
        try
        {
            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
                assetBundleDirectory,
                BuildAssetBundleOptions.ChunkBasedCompression,
                target
            );
            if (manifest != null)
            {
                Debug.Log("[BuildAssetBundles] Standard build generated:");
                foreach (string b in manifest.GetAllAssetBundles())
                {
                    Debug.Log("  - " + b);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[BuildAssetBundles] Standard build info: " + ex.Message);
        }

        // Copy any bundles from Build/Android to Assets/AssetBundles so upload-artifact captures them
        if (Directory.Exists("AssetBundles/Android"))
        {
            foreach (string file in Directory.GetFiles("AssetBundles/Android"))
            {
                string dest = Path.Combine(assetBundleDirectory, Path.GetFileName(file));
                File.Copy(file, dest, true);
                Debug.Log("[BuildAssetBundles] Copied " + file + " -> " + dest);
            }
        }
        if (Directory.Exists("Build/Android"))
        {
            foreach (string file in Directory.GetFiles("Build/Android"))
            {
                string dest = Path.Combine(assetBundleDirectory, Path.GetFileName(file));
                File.Copy(file, dest, true);
                Debug.Log("[BuildAssetBundles] Copied " + file + " -> " + dest);
            }
        }
    }

    private static void InvokeMethod(string typeName, string methodName)
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
                MethodInfo m = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
                if (m != null)
                {
                    Debug.Log("[BuildAssetBundles] Executing " + typeName + "." + methodName + "()...");
                    m.Invoke(null, null);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[BuildAssetBundles] Error executing " + typeName + "." + methodName + ": " + ex);
        }
    }
}
