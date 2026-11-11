// -----------------------------------------------------------------------------
//  CyberTruckBuilder.cs  -  Cybertruck mod build pipeline (Android AssetBundle)
// -----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class CyberTruckBuilder
{
    public const string Root = "Assets/CyberTruck";
    public const string ModelDir = Root + "/Models";
    public const string MeshDir = Root + "/Meshes";
    public const string MatDir = Root + "/Materials";
    public const string PrefabPath = Root + "/Prefabs/CyberTruck.prefab";
    public const string PrefabAliasPath = Root + "/Prefabs/rgs.prefab";
    public const string BundleName = "rgs";
    public const string BuildDir = "Build/Android";

    static readonly string[,] MeshMap =
    {
        { "Body",    "CyberTruck_Body" },
        { "Mesh_FL", "Wheel_FL"        },
        { "Mesh_FR", "Wheel_FR"        },
        { "Mesh_RL", "Wheel_RL"        },
        { "Mesh_RR", "Wheel_RR"        },
    };

    static readonly string[] BodyMats =
    { "CyberTruck_Body", "CyberTruck_Trim", "CyberTruck_Light", "CyberTruck_Tail" };
    static readonly string[] WheelMats = { "CyberTruck_Tire", "CyberTruck_Rim" };

    [MenuItem("CyberTruck/1 · Prepare project", false, 1)]
    public static void PrepareProject()
    {
        ConfigureImporters();
        AssetDatabase.Refresh();
        BuildMeshAssets();
        WirePrefab();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CyberTruck] Project successfully prepared!");
    }

    [MenuItem("CyberTruck/2 · Build Android AssetBundle", false, 2)]
    public static void BuildBundle()
    {
        PrepareProject();

        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            Debug.Log("[CyberTruck] Switching build target to Android...");
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        }

        Directory.CreateDirectory(BuildDir);
        var build = new AssetBundleBuild[2];
        build[0].assetBundleName = "rgs";
        build[0].assetNames = new[]
        {
            PrefabPath.Replace("\\", "/"),
            PrefabAliasPath.Replace("\\", "/"),
        };
        build[1].assetBundleName = "cybertruck";
        build[1].assetNames = new[]
        {
            PrefabPath.Replace("\\", "/"),
            PrefabAliasPath.Replace("\\", "/"),
        };

        var manifest = BuildPipeline.BuildAssetBundles(
            BuildDir,
            build,
            BuildAssetBundleOptions.ChunkBasedCompression,
            EditorUserBuildSettings.activeBuildTarget);

        if (manifest == null)
        {
            Debug.LogError("[CyberTruck] Bundle build FAILED.");
            return;
        }

        string p = Path.GetFullPath(Path.Combine(BuildDir, "rgs"));
        if (File.Exists(p))
        {
            var mb = new FileInfo(p).Length / 1024f / 1024f;
            Debug.Log("[CyberTruck] AssetBundle built successfully -> " + p + " (" + mb.ToString("0.00") + " MB)");
        }
    }

    static void ConfigureImporters()
    {
        string[] names = new[] { "CyberTruck_Body", "Wheel_FL", "Wheel_FR", "Wheel_RL", "Wheel_RR" };
        foreach (var name in names)
        {
            string path = ModelDir + "/" + name + ".obj";
            var mi = AssetImporter.GetAtPath(path) as ModelImporter;
            if (mi != null)
            {
                mi.isReadable = true;
                mi.globalScale = 1f;
                mi.importAnimation = false;
                mi.SaveAndReimport();
            }
        }
    }

    static void BuildMeshAssets()
    {
        Directory.CreateDirectory(MeshDir);
        string[] names = new[] { "CyberTruck_Body", "Wheel_FL", "Wheel_FR", "Wheel_RL", "Wheel_RR" };
        foreach (var name in names)
        {
            string objPath = ModelDir + "/" + name + ".obj";
            AssetDatabase.ImportAsset(objPath, ImportAssetOptions.ForceUpdate);
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(objPath);
            if (mesh == null)
            {
                mesh = AssetDatabase.LoadAllAssetsAtPath(objPath).OfType<Mesh>().FirstOrDefault();
            }

            if (mesh != null)
            {
                string assetPath = MeshDir + "/" + name + ".asset";
                var copy = UnityEngine.Object.Instantiate(mesh);
                copy.name = name;
                var existing = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
                if (existing != null)
                {
                    EditorUtility.CopySerialized(copy, existing);
                    EditorUtility.SetDirty(existing);
                }
                else
                {
                    AssetDatabase.CreateAsset(copy, assetPath);
                }
                Debug.Log("[CyberTruck] Created mesh asset: " + assetPath);
            }
            else
            {
                Debug.LogError("[CyberTruck] Could not find mesh at " + objPath);
            }
        }
    }

    static Dictionary<string, Material> LoadMaterials()
    {
        var dict = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);
        foreach (var g in AssetDatabase.FindAssets("t:Material", new[] { MatDir }))
        {
            var p = AssetDatabase.GUIDToAssetPath(g);
            var m = AssetDatabase.LoadAssetAtPath<Material>(p);
            if (m != null) dict[m.name] = m;
        }
        return dict;
    }

    static void WirePrefab()
    {
        var mats = LoadMaterials();
        var contents = PrefabUtility.LoadPrefabContents(PrefabPath);
        if (contents == null) { Debug.LogError("[CyberTruck] Cannot open " + PrefabPath); return; }

        try
        {
            for (int i = 0; i < MeshMap.GetLength(0); i++)
            {
                string goName = MeshMap[i, 0];
                string meshName = MeshMap[i, 1];

                var t = FindDeep(contents.transform, goName);
                if (t == null) continue;

                var mf = t.GetComponent<MeshFilter>();
                var mr = t.GetComponent<MeshRenderer>();
                if (mf == null) continue;

                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(MeshDir + "/" + meshName + ".asset");
                if (mesh == null) continue;
                mf.sharedMesh = mesh;

                if (mr != null)
                {
                    var names = meshName == "CyberTruck_Body" ? BodyMats : WheelMats;
                    var arr = new Material[mesh.subMeshCount];
                    for (int s = 0; s < arr.Length; s++)
                    {
                        Material mm;
                        arr[s] = mats.TryGetValue(names[Mathf.Min(s, names.Length - 1)], out mm) ? mm : null;
                    }
                    mr.sharedMaterials = arr;
                }
                Debug.Log("[CyberTruck] Wired " + goName + " <- " + mesh.name);
            }

            // Speed & Acceleration Tuning: 5000 speed, 500 in back, good acceleration
            var cc = contents.GetComponent<CarControl>();
            if (cc != null)
            {
                cc.topSpeed = 5000f;
                cc.reverseSpeed = 500f;
                cc.maxTorque = 68000f; // Good acceleration
            }

            var rb = contents.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass = 4000f;
                rb.drag = 0.005f; // Zero air drag so jumps never drop speed
                rb.angularDrag = 1.0f;
            }

            // Suspension: good suspension absorbing bumps smoothly
            var wheelColliders = contents.GetComponentsInChildren<WheelCollider>(true);
            foreach (var wc in wheelColliders)
            {
                wc.radius = 0.47f;
                wc.suspensionDistance = 0.30f;
                var spring = wc.suspensionSpring;
                spring.spring = 45000f;
                spring.damper = 5500f;
                spring.targetPosition = 0.40f;
                wc.suspensionSpring = spring;
            }

            // Player Protection: player should not fall at high speed
            var cic = contents.GetComponent<CarImpactCheck>();
            if (cic != null)
            {
                cic.enabled = false;
                cic.damage = false;
                cic._colDist = 999999f;
            }

            var rc = contents.GetComponent<RidingCar>();
            if (rc != null)
            {
                rc.frontGlass = null;
                rc.CamDis = 10;
            }

            var ev = contents.GetComponent<ExplosionVehicle>();
            if (ev != null)
            {
                ev.health = 999999999;
                ev.enabled = false;
            }

            var triggerKill = contents.transform.Find("TriggerKill");
            if (triggerKill != null) triggerKill.gameObject.SetActive(false);

            var doorJoints = contents.GetComponentsInChildren<CarJointControl>(true);
            foreach (var dj in doorJoints)
            {
                dj.DropThisDoor = false;
                dj.health = 999999999;
            }

            // Cameras
            var cam = contents.transform.Find("Cam");
            if (cam != null) cam.localPosition = new Vector3(0.0f, 1.60f, 0.20f);

            var interiorCam = contents.transform.Find("Interior/InteriorCam");
            if (interiorCam != null) interiorCam.localPosition = new Vector3(0.0f, 0.25f, 0.15f);

            PrefabUtility.SaveAsPrefabAsset(contents, PrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }

        AssetDatabase.DeleteAsset(PrefabAliasPath);
        AssetDatabase.CopyAsset(PrefabPath, PrefabAliasPath);
        AssetDatabase.ImportAsset(PrefabAliasPath);
    }

    static Transform FindDeep(Transform root, string name)
    {
        if (root.name == name) return root;
        for (int i = 0; i < root.childCount; i++)
        {
            var r = FindDeep(root.GetChild(i), name);
            if (r != null) return r;
        }
        return null;
    }
}
