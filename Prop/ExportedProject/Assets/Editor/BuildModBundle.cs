using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class BuildModBundle
{
    const string BundleName = "rgs";
    const string PrefabPath = "Assets/outsidemods/prefab/rgs.prefab";
    const string BindingsPath = "Assets/Editor/mesh_bindings.json";

    [InitializeOnLoadMethod]
    static void OnEditorLoad()
    {
        EditorApplication.delayCall += () =>
        {
            if (!Application.isBatchMode) ResolveMeshBindings();
        };
    }

    [MenuItem("Modding/Build \"rgs\" Bundle (Android)")]
    public static void BuildFromMenu()
    {
        Build(BuildTarget.Android);
    }

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

    static void ResolveMeshBindings()
    {
        if (!File.Exists(PrefabPath)) return;
        Dictionary<string, string> bindings = LoadBindings();

        GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
        if (root == null) return;

        bool dirty = false;
        foreach (MeshFilter mf in root.GetComponentsInChildren<MeshFilter>(true))
        {
            string relPath = GetHierarchyPath(root.transform, mf.transform);
            string assetPath = null;
            if (bindings.TryGetValue(relPath, out string mapped))
            {
                assetPath = mapped;
            }
            else if (mf.sharedMesh == null)
            {
                string guess = "Assets/Mesh/" + mf.gameObject.name + ".obj";
                if (File.Exists(guess)) assetPath = guess;
            }
            if (!string.IsNullOrEmpty(assetPath) && File.Exists(assetPath))
            {
                Mesh loaded = AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Mesh>().FirstOrDefault();
                if (loaded != null && mf.sharedMesh != loaded)
                {
                    mf.sharedMesh = loaded;
                    dirty = true;
                }
            }
        }
        foreach (MeshCollider mc in root.GetComponentsInChildren<MeshCollider>(true))
        {
            if (mc.sharedMesh == null)
            {
                MeshFilter mf = mc.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    mc.sharedMesh = mf.sharedMesh;
                    dirty = true;
                }
            }
        }

        if (dirty) PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
    }

    static Dictionary<string, string> LoadBindings()
    {
        var result = new Dictionary<string, string>();
        if (!File.Exists(BindingsPath)) return result;
        string json = File.ReadAllText(BindingsPath);
        foreach (Match m in Regex.Matches(json, "\"([^\"]*)\"\\s*:\\s*\"([^\"]*)\""))
        {
            result[m.Groups[1].Value] = m.Groups[2].Value;
        }
        return result;
    }

    static string GetHierarchyPath(Transform root, Transform t)
    {
        if (t == root) return "";
        var parts = new List<string>();
        Transform cur = t;
        while (cur != null && cur != root)
        {
            parts.Add(cur.name);
            cur = cur.parent;
        }
        parts.Reverse();
        return string.Join("/", parts);
    }

    static void Build(BuildTarget target)
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string outDir = Path.Combine(projectRoot, "Build", target.ToString());
        Directory.CreateDirectory(outDir);

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        ResolveMeshBindings();
        TagAllAssets();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
            outDir,
            BuildAssetBundleOptions.ChunkBasedCompression,
            target);
        if (manifest == null)
            throw new Exception("[BuildModBundle] BuildAssetBundles failed.");
    }

    static void TagAllAssets()
    {
        string[] files = Directory.GetFiles("Assets", "*.*", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            string assetPath = file.Replace('\\', '/');
            if (assetPath.EndsWith(".meta") || assetPath.EndsWith(".cs") || assetPath.StartsWith("Assets/Editor/"))
                continue;
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer != null && importer.assetBundleName != BundleName)
                importer.assetBundleName = BundleName;
        }
    }
}
