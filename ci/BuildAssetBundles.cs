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

        // Invoke project-specific setup via Reflection so missing classes never cause compile errors
        InvokeSetupMethod("SetupBuildingProp");
        InvokeSetupMethod("SetupBatmobileMod");

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
            Debug.Log("  - " + b);
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
