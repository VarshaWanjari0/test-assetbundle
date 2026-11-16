// Rebuilds the recovered root prefab as an Android Unity AssetBundle, NOT a glTF file.
// Asset and address names intentionally match the supplied examples.
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ModBundleBuilder
{
    private const string PrefabPath = "Assets/outsidemods/prefab/rgs.prefab";
    private const string Address = "assets/outsidemods/prefab/rgs.prefab";
    private const string BundleName = "rgs";
    private const string DefaultFilename = "pumpkinBoy.glb";

    [MenuItem("Mods/Build Android bundle...")]
    public static void BuildFromMenu()
    {
        string destination = EditorUtility.SaveFilePanel("Save Android AssetBundle", "", DefaultFilename, "glb");
        if (string.IsNullOrEmpty(destination)) return;
        try {
            Build(destination);
            EditorUtility.DisplayDialog("Bundle created", "Saved to:\n" + destination + "\n\nTest on the target game; compatibility is not guaranteed.", "OK");
        } catch (Exception e) {
            Debug.LogException(e);
            EditorUtility.DisplayDialog("Bundle build failed", e.Message, "OK");
        }
    }

    // Call with -executeMethod ModBundleBuilder.BuildFromCommandLine -modOutput /absolute/path/name.glb
    public static void BuildFromCommandLine()
    {
        string output = null;
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++) {
            if (args[i] == "-modOutput") {
                if (++i >= args.Length || args[i].StartsWith("-")) throw new ArgumentException("-modOutput requires a file path.");
                output = args[i];
            }
        }
        if (string.IsNullOrEmpty(output)) throw new ArgumentException("Supply -modOutput /absolute/path/name.glb");
        Build(output);
    }

    private static void Build(string output)
    {
        if (!Path.IsPathRooted(output)) throw new ArgumentException("Output path must be absolute.");
        string fullOutput = Path.GetFullPath(output);
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string assetDir = Path.GetFullPath(Application.dataPath) + Path.DirectorySeparatorChar;
        if (fullOutput.StartsWith(assetDir, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Output must be outside the project's Assets folder.");
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            throw new InvalidOperationException("Unity Android Build Support is not installed for this editor.");
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null) throw new InvalidOperationException("Missing prefab " + PrefabPath);
        int missing = 0;
        foreach (Transform t in prefab.GetComponentsInChildren<Transform>(true))
            missing += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
        if (missing != 0) throw new InvalidOperationException("Prefab contains " + missing + " missing scripts. Resolve them before building.");

        string temporary = Path.Combine(projectRoot, "Library", "RecoveredModBundleBuild");
        if (Directory.Exists(temporary)) Directory.Delete(temporary, true);
        Directory.CreateDirectory(temporary);
        var map = new AssetBundleBuild[] { new AssetBundleBuild {
            assetBundleName = BundleName,
            assetNames = new[] { PrefabPath },
            addressableNames = new[] { Address }
        }};
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(temporary, map,
            BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.ForceRebuildAssetBundle,
            BuildTarget.Android);
        string bundle = Path.Combine(temporary, BundleName);
        if (manifest == null || !File.Exists(bundle)) throw new InvalidOperationException("Unity did not produce the bundle. See the Editor log.");
        string parent = Path.GetDirectoryName(fullOutput);
        if (string.IsNullOrEmpty(parent)) throw new ArgumentException("Invalid output path.");
        Directory.CreateDirectory(parent);
        File.Copy(bundle, fullOutput, true);
        Debug.Log("Mod bundle written to " + fullOutput + " (address: " + Address + ", target: Android)");
    }
}
