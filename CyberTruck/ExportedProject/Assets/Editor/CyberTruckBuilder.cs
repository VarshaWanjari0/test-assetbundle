// -----------------------------------------------------------------------------
//  CyberTruckBuilder.cs  -  Cybertruck mod build pipeline (Android AssetBundle)
//
//  Put this file in  Assets/Editor/  of the project (it already is).
//
//  What it does
//    1. imports CyberTruck.fbx with the right settings (1:1 metres, readable)
//    2. copies the body + four wheel meshes out of the FBX into real Mesh assets
//       (Assets/CyberTruck/Meshes/*.asset) so the prefab can reference them
//    3. wires those meshes + the six .mat materials into CyberTruck.prefab
//    4. builds the Android AssetBundle that the game loads
//
//  Menu:   CyberTruck / 1 - Prepare project
//          CyberTruck / 2 - Build Android AssetBundle
//          CyberTruck / 3 - Check vehicle rig
//
//  Step 1 also runs automatically the first time the project is opened, so in
//  the normal case you just press "2" after the import finished.
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
    public const string FbxPath = Root + "/Models/CyberTruck.fbx";
    public const string MeshDir = Root + "/Meshes";
    public const string MatDir = Root + "/Materials";
    public const string PrefabPath = Root + "/Prefabs/CyberTruck.prefab";
    public const string PrefabAliasPath = Root + "/Prefabs/rgs.prefab";
    public const string BundleName = "cybertruck";
    public const string BuildDir = "Build/Android";

    // name of the mesh object inside the prefab  ->  mesh name inside the FBX
    static readonly string[,] MeshMap =
    {
        { "Body",    "CyberTruck_Body" },
        { "Mesh_FL", "Wheel_FL"        },
        { "Mesh_FR", "Wheel_FR"        },
        { "Mesh_RL", "Wheel_RL"        },
        { "Mesh_RR", "Wheel_RR"        },
    };

    // material list per mesh, in submesh order
    static readonly string[] BodyMats =
    { "CyberTruck_Body", "CyberTruck_Trim", "CyberTruck_Light", "CyberTruck_Tail" };
    static readonly string[] WheelMats = { "CyberTruck_Tire", "CyberTruck_Rim" };

    // ---------------------------------------------------------------- auto run
    [InitializeOnLoadMethod]
    static void AutoPrepare()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            if (!File.Exists(FbxPath)) return;
            if (File.Exists(MeshDir + "/CyberTruck_Body.asset") &&
                File.Exists(PrefabPath) && PrefabIsWired()) return;
            Debug.Log("[CyberTruck] first time setup - preparing meshes + prefab ...");
            PrepareProject();
        };
    }

    // ---------------------------------------------------------------- menu
    [MenuItem("CyberTruck/1 \u00b7 Prepare project (meshes + prefab)", false, 1)]
    public static void PrepareProject()
    {
        ConfigureImporter();
        AssetDatabase.Refresh();
        BuildMeshAssets();
        WirePrefab();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CyberTruck] ready. now run:  CyberTruck / 2 - Build Android AssetBundle");
    }

    [MenuItem("CyberTruck/2 \u00b7 Build Android AssetBundle", false, 2)]
    public static void BuildBundle()
    {
        PrepareProject();

        // the bundle MUST be built for the platform that will load it
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            Debug.Log("[CyberTruck] switching build target to Android ...");
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            {
                Debug.LogError("[CyberTruck] could not switch to Android - is the Android module installed " +
                               "(Unity Hub > Installs > Add modules > Android Build Support)?");
                return;
            }
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
            BuildAssetBundleOptions.ChunkBasedCompression,   // LZ4 - loads fast on Android
            EditorUserBuildSettings.activeBuildTarget);

        if (manifest == null)
        {
            Debug.LogError("[CyberTruck] bundle build FAILED - check the console above.");
            return;
        }
        string p = Path.GetFullPath(Path.Combine(BuildDir, BundleName));
        if (File.Exists(p))
        {
            var mb = new FileInfo(p).Length / 1024f / 1024f;
            Debug.Log("[CyberTruck] AssetBundle built -> " + p + "  (" + mb.ToString("0.00") + " MB)");
            EditorUtility.RevealInFinder(p);
        }
        else Debug.LogWarning("[CyberTruck] build finished but " + p + " was not found.");
    }

    [MenuItem("CyberTruck/3 \u00b7 Check vehicle rig", false, 3)]
    public static void CheckRig()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null) { Debug.LogError("[CyberTruck] prefab missing: " + PrefabPath); return; }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("[CyberTruck] rig report");

        var rb = prefab.GetComponent<Rigidbody>();
        if (rb != null)
            sb.AppendLine("  rigidbody  mass " + rb.mass + "  drag " + rb.drag + "  angDrag " + rb.angularDrag
                          + "  collision " + rb.collisionDetectionMode + "  interp " + rb.interpolation);

        var wheels = prefab.GetComponentsInChildren<WheelCollider>(true);
        sb.AppendLine("  wheel colliders: " + wheels.Length);
        foreach (var w in wheels)
            sb.AppendLine("     " + w.transform.name + " pos " + w.transform.localPosition
                          + " r " + w.radius + " susp " + w.suspensionDistance);

        var cc = prefab.GetComponent("CarControl") as MonoBehaviour;
        if (cc != null)
        {
            var t = cc.GetType();
            sb.AppendLine("  CarControl found");
            TryLog(sb, t, cc, "topSpeed");
            TryLog(sb, t, cc, "reverseSpeed");
            TryLog(sb, t, cc, "maxTorque");
            var wc = t.GetField("wheelColliders");
            var tm = t.GetField("tireMeshes");
            if (wc != null) sb.AppendLine("     wheelColliders wired: " + ((Array)wc.GetValue(cc))?.Length);
            if (tm != null) sb.AppendLine("     tireMeshes wired: " + ((Array)tm.GetValue(cc))?.Length);
        }
        else sb.AppendLine("  !! CarControl component missing (script GUID mismatch!)");

        foreach (var mf in prefab.GetComponentsInChildren<MeshFilter>(true))
        {
            var mesh = mf.sharedMesh;
            sb.AppendLine("  mesh '" + mf.gameObject.name + "' -> " +
                          (mesh != null ? mesh.name + " (" + mesh.subMeshCount + " submeshes)"
                                        : "NULL (run step 1)"));
            if (mesh != null)
            {
                var mr = mf.GetComponent<MeshRenderer>();
                if (mr != null && mr.sharedMaterials.Length != mesh.subMeshCount)
                    sb.AppendLine("     !! material count " + mr.sharedMaterials.Length +
                                  " != submeshes " + mesh.subMeshCount);
            }
        }

        var cam = prefab.GetComponentInChildren<Camera>(true);
        if (cam != null)
            sb.AppendLine("  camera fov " + cam.fieldOfView + " near " + cam.nearClipPlane +
                          " far " + cam.farClipPlane + " on '" + cam.gameObject.name + "'");

        sb.AppendLine("  unity version: " + Application.unityVersion);
        Debug.Log(sb.ToString());
    }

    static void TryLog(System.Text.StringBuilder sb, Type t, object o, string f)
    {
        var fi = t.GetField(f);
        if (fi != null) sb.AppendLine("     " + f + " = " + fi.GetValue(o));
    }

    // ---------------------------------------------------------------- helpers
    static void ConfigureImporter()
    {
        var mi = AssetImporter.GetAtPath(FbxPath) as ModelImporter;
        if (mi == null) { Debug.LogError("[CyberTruck] no ModelImporter for " + FbxPath); return; }

        bool dirty = false;
        if (!mi.isReadable) { mi.isReadable = true; dirty = true; }
        if (mi.useFileScale) { mi.useFileScale = false; dirty = true; }      // 1 unit == 1 m
        if (Mathf.Abs(mi.globalScale - 1f) > 0.0001f) { mi.globalScale = 1f; dirty = true; }
        if (mi.materialImportMode == ModelImporterMaterialImportMode.None) { mi.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription; dirty = true; }
        if (mi.importAnimation) { mi.importAnimation = false; dirty = true; }
        if (mi.animationType != ModelImporterAnimationType.None) { mi.animationType = ModelImporterAnimationType.None; dirty = true; }
        if (mi.meshCompression != ModelImporterMeshCompression.Off) { mi.meshCompression = ModelImporterMeshCompression.Off; dirty = true; }
        if (mi.addCollider) { mi.addCollider = false; dirty = true; }
        // keep the axis conversion Unity does by default (RH -> LH); the FBX is
        // authored so the car ends up facing +Z with its right side at +X
        if (mi.bakeAxisConversion) { mi.bakeAxisConversion = false; dirty = true; }

        if (dirty) { mi.SaveAndReimport(); Debug.Log("[CyberTruck] model importer updated"); }
    }

    static void BuildMeshAssets()
    {
        Directory.CreateDirectory(MeshDir);
        var meshes = AssetDatabase.LoadAllAssetsAtPath(FbxPath).OfType<Mesh>().ToArray();
        if (meshes.Length == 0) { Debug.LogError("[CyberTruck] no meshes in " + FbxPath); return; }

        int made = 0;
        foreach (var m in meshes)
        {
            string assetPath = MeshDir + "/" + Sanitize(m.name) + ".asset";
            var copy = UnityEngine.Object.Instantiate(m);
            copy.name = m.name;

            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if (existing != null)
            {
                EditorUtility.CopySerialized(copy, existing);
                EditorUtility.SetDirty(existing);
            }
            else AssetDatabase.CreateAsset(copy, assetPath);
            made++;
        }
        foreach (var m in meshes)
            if (m.name == "CyberTruck_Body")
                Debug.Log("[CyberTruck] body mesh bounds: size " + m.bounds.size + " centre " + m.bounds.center);

        Debug.Log("[CyberTruck] " + made + " mesh asset(s) written to " + MeshDir);
    }

    static string Sanitize(string s)
    {
        foreach (char c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
        return s.Replace(' ', '_');
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
        if (contents == null) { Debug.LogError("[CyberTruck] cannot open " + PrefabPath); return; }

        try
        {
            for (int i = 0; i < MeshMap.GetLength(0); i++)
            {
                string goName = MeshMap[i, 0];
                string meshName = MeshMap[i, 1];

                var t = FindDeep(contents.transform, goName);
                if (t == null) { Debug.LogWarning("[CyberTruck] object not found in prefab: " + goName); continue; }

                var mf = t.GetComponent<MeshFilter>();
                var mr = t.GetComponent<MeshRenderer>();
                if (mf == null) { Debug.LogWarning("[CyberTruck] no MeshFilter on " + goName); continue; }

                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(MeshDir + "/" + meshName + ".asset");
                if (mesh == null) { Debug.LogWarning("[CyberTruck] mesh asset missing: " + meshName); continue; }
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
                Debug.Log("[CyberTruck] wired " + goName + " <- " + mesh.name + " submeshes " + mesh.subMeshCount);
            }

            // Vehicle Tuning: 5000 speed, 500 in back, good acceleration, 4000 mass, zero jump drag
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
                rb.drag = 0.005f; // Zero air drag so jump never loses speed
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

        // keep the "rgs" alias in sync (some mod loaders look that name up)
        AssetDatabase.DeleteAsset(PrefabAliasPath);
        AssetDatabase.CopyAsset(PrefabPath, PrefabAliasPath);
        AssetDatabase.ImportAsset(PrefabAliasPath);
    }

    static bool PrefabIsWired()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null) return false;
        var t = FindDeep(prefab.transform, "Body");
        if (t == null) return false;
        var mf = t.GetComponent<MeshFilter>();
        return mf != null && mf.sharedMesh != null;
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
