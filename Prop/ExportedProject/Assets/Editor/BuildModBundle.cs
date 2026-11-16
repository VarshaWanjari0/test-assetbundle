using System;
using System.IO;
using UnityEditor;
using UnityEngine;

// Builds the "rgs" mod bundle the Indian Bikes Driving 3D mod loader expects.
// Bundle name: rgs | entry asset: Assets/outsidemods/prefab/rgs.prefab
// Everything under Assets/ (except .cs/.meta) is tagged into the "rgs" bundle
// so the prefab, meshes, materials, textures, audio and shaders ship together.
public static class BuildModBundle
{
    const string BundleName = "rgs";

    [MenuItem("Modding/Build \"rgs\" Bundle (Android)")]
    public static void BuildFromMenu()
    {
        Build(BuildTarget.Android);
    }

    // CLI: unity -batchmode -quit -nographics -projectPath <this project> \
    //        -buildTarget Android -executeMethod BuildModBundle.BuildFromCommandLine -logFile build.log
    public static void BuildFromCommandLine()
    {
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], "-buildTarget", StringComparison.OrdinalIgnoreCase) &&
                Enum.TryParse(args[i + 1], true, out BuildTarget parsed))
            {
                target = parsed;
                break;
            }
        }
        Build(target);
    }

    static void Build(BuildTarget target)
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string outDir = Path.Combine(projectRoot, "Build", target.ToString());
        Directory.CreateDirectory(outDir);

        TagAllAssets();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
            outDir,
            BuildAssetBundleOptions.ChunkBasedCompression,
            target);
        if (manifest == null)
        {
            throw new Exception("[BuildModBundle] BuildPipeline.BuildAssetBundles returned null; see the build log above for the reason.");
        }

        string[] bundles = manifest.GetAllAssetBundles();
        bool found = false;
        foreach (string b in bundles)
        {
            Debug.Log("[BuildModBundle] Built bundle: " + b + " -> " + Path.Combine(outDir, b));
            if (b == BundleName) found = true;
        }
        if (!found)
        {
            Debug.LogWarning("[BuildModBundle] Expected bundle '" + BundleName + "' was not produced. Found: " +
                             (bundles.Length == 0 ? "(none)" : string.Join(", ", bundles)));
        }
    }

    static void TagAllAssets()
    {
        string[] files = Directory.GetFiles("Assets", "*.*", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            if (file.EndsWith(".meta") || file.EndsWith(".cs")) continue;
            string assetPath = file.Replace('\\', '/');
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer != null && importer.assetBundleName != BundleName)
            {
                importer.assetBundleName = BundleName;
            }
        }
    }
}
