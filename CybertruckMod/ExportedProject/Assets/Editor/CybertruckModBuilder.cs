// CybertruckModBuilder.cs
// One-click builder for the Cybertruck outside-mod AssetBundle (Android).
//
// What it does:
//   1. Loads Assets/Models/cybertruck.glb (imported by glTFast)
//   2. Attaches its visual meshes/materials under the "truck" body node of the
//      rgs.prefab template (which already contains the game's vehicle rig:
//      CarControl + RidingCar + wheel colliders + doors + camera + audio)
//   3. Re-applies the Cybertruck performance spec (topSpeed/reverseSpeed/torque/
//      suspension/grip) so the prefab is always consistent
//   4. Points the two EngineSound AudioSources at the EV clips
//   5. Saves back to Assets/outsidemods/prefab/rgs.prefab  (kept as "rgs"
//      because the game's mod loader (Assets/Scripts/Assembly-CSharp/rgs class)
//      loads the bundle asset by that name)
//   6. Assigns AssetBundle name "rgs" and builds bundles for Android into
//      ../AssetBundles/Android/
//
// Menu:  Tools > Cybertruck > Build Mod (Android)
//        Tools > Cybertruck > Attach Cybertruck Body Only
//        Tools > Cybertruck > Build AssetBundle Only
//
// Requirements: Unity 2021.3.x (project targets 2021.3.43f1) + Android Build Support.

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor;
using static UnityEditor.BuildPipeline;
using System.IO;
using System.Collections.Generic;

public static class CybertruckModBuilder
{
    const string PrefabPath   = "Assets/outsidemods/prefab/rgs.prefab";
    const string GlbPath      = "Assets/Models/cybertruck.glb";
    const string EvLoopClip   = "Assets/AudioClip/EV_Engine_Loop.wav";
    const string EvLoadClip   = "Assets/AudioClip/EV_Engine_Load.wav";
    const string BundleName   = "rgs";   // asset name the game's mod loader expects

    // ---------------------------------------------------------------
    [MenuItem("Tools/Cybertruck/Build Mod (Android)")]
    public static void BuildAll()
    {
        if (!AttachCybertruckBody()) return;
        ApplySpecs();
        if (!BuildBundle()) return;
        EditorUtility.DisplayDialog("Cybertruck Mod",
            "AssetBundle built!\n\nOutput: AssetBundles/Android/" + BundleName +
            "\n\nCopy it into the game's mods folder (same place the Truck mod lives), " +
            "then load it in-game with the mod menu / cheat phone.", "Nice 🤙");
    }

    [MenuItem("Tools/Cybertruck/Attach Cybertruck Body Only")]
    public static void AttachOnly() { if (AttachCybertruckBody()) { ApplySpecs(); } }

    [MenuItem("Tools/Cybertruck/Apply Cybertruck Specs")]
    public static void SpecsOnly() { ApplySpecs(); }

    [MenuItem("Tools/Cybertruck/Build AssetBundle Only")]
    public static void BundleOnly() { BuildBundle(); }

    // ---------------------------------------------------------------
    static bool AttachCybertruckBody()
    {
        var glb = AssetDatabase.LoadAssetAtPath<GameObject>(GlbPath);
        if (glb == null)
        {
            EditorUtility.DisplayDialog("Cybertruck Mod",
                "Could not load " + GlbPath + "\n\n" +
                "Make sure the glTFast package (com.unity.cloud.gltfast) restored " +
                "successfully and the project has finished importing.", "OK");
            return false;
        }

        var root = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            // Find the "truck" body child (it has no renderer of its own in the export —
            // we parent the GLB visuals here).
            Transform truck = FindDeepChild(root.transform, "truck");
            if (truck == null)
            {
                EditorUtility.DisplayDialog("Cybertruck Mod", "Couldn't find the 'truck' child in rgs.prefab!", "OK");
                return false;
            }

            // Remove any previously attached body visual
            var old = truck.Find("Cybertruck (Body)");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            // Instantiate GLB visual and nest it
            var body = (GameObject)PrefabUtility.InstantiatePrefab(glb, truck);
            body.name = "Cybertruck (Body)";
            body.transform.localPosition = Vector3.zero;
            body.transform.localRotation = Quaternion.identity;
            // The Cybertruck GLB is ~4.7 m long; the truck rig expects ~8 m wheelbase.
            // Scale to fit between the wheel colliders (adjust to taste).
            body.transform.localScale = Vector3.one * 1.6f;

            // Report bounds so the user can fine-tune scale/offset in the Inspector
            var rends = body.GetComponentsInChildren<Renderer>();
            if (rends.Length > 0)
            {
                Bounds b = rends[0].bounds;
                foreach (var r in rends) b.Encapsulate(r.bounds);
                Debug.Log($"[Cybertruck] Body bounds size = {b.size} (before parenting offsets). " +
                          "If the body looks misaligned vs wheel colliders, tweak " +
                          "'Cybertruck (Body)' localScale/localPosition in the prefab.");
            }

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Debug.Log("[Cybertruck] ✅ Cybertruck body attached to rgs.prefab");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
        return true;
    }

    // ---------------------------------------------------------------
    static void ApplySpecs()
    {
        var root = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            int fixedCar = 0, fixedWheel = 0, fixedRb = 0;

            foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb == null) continue;
                var cType = mb.GetType().Name;
                if (cType == "CarControl")
                {
                    var so = new SerializedObject(mb);
                    SetFloat(so, "topSpeed", 5000f);
                    SetFloat(so, "reverseSpeed", 500f);
                    SetFloat(so, "maxTorque", 60000f);
                    // gears: long spread for EV punch
                    var gears = so.FindProperty("gears");
                    if (gears != null && gears.isArray)
                    {
                        int[] g = { 800, 1600, 2400, 3200, 4000, 5000 };
                        gears.arraySize = g.Length;
                        for (int i = 0; i < g.Length; i++) gears.GetArrayElementAtIndex(i).intValue = g[i];
                    }
                    so.ApplyModifiedPropertiesWithoutUndo();
                    fixedCar++;
                }
            }

            foreach (var wc in root.GetComponentsInChildren<WheelCollider>(true))
            {
                // Heavy EV: firm springs, long dampers, HUGE grip, planted feel
                var spring = wc.suspensionSpring;
                spring.spring = 65000f; spring.damper = 7000f; spring.targetPosition = 0.5f;
                wc.suspensionSpring = spring;
                wc.suspensionDistance = 0.22f;
                wc.forceAppPointDistance = 0.05f;
                wc.mass = 60f;
                wc.wheelDampingRate = 0.4f;
                var ff = wc.forwardFriction;
                ff.extremumSlip = 2f; ff.extremumValue = 10f; ff.asymptoteSlip = 1.5f; ff.asymptoteValue = 2f; ff.stiffness = 1.4f;
                wc.forwardFriction = ff;
                var sf = wc.sidewaysFriction;
                sf.extremumSlip = 1.8f; sf.extremumValue = 10f; sf.asymptoteSlip = 1.5f; sf.asymptoteValue = 2f; sf.stiffness = 1.5f;
                wc.sidewaysFriction = sf;
                fixedWheel++;
            }

            var rb = root.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass = 4000f;               // battery weight keeps it planted
                rb.angularDrag = 2.5f;         // resists high-speed flipping
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // no tunneling at 5000!
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                fixedRb++;
            }

            // Center of mass: slam it rock-bottom
            var com = FindDeepChild(root.transform, "Center of Mass");
            if (com != null)
            {
                com.localPosition = new Vector3(com.localPosition.x, -0.35f, com.localPosition.z);
            }

            // EV audio: replace engine sounds with the synthesized EV whine
            var loopClip = AssetDatabase.LoadAssetAtPath<AudioClip>(EvLoopClip);
            var loadClip = AssetDatabase.LoadAssetAtPath<AudioClip>(EvLoadClip);
            foreach (var src in root.GetComponentsInChildren<AudioSource>(true))
            {
                if (src.gameObject.name == "EngineSound"   && loopClip != null) src.clip = loopClip;
                if (src.gameObject.name == "EngineSound 2" && loadClip != null) src.clip = loadClip;
            }

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Debug.Log($"[Cybertruck] ✅ Specs applied — CarControl:{fixedCar} WheelCollider:{fixedWheel} Rigidbody:{fixedRb}");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    // ---------------------------------------------------------------
    static bool BuildBundle()
    {
        // Make sure the prefab carries the bundle name
        var importer = AssetImporter.GetAtPath(PrefabPath);
        if (importer == null)
        {
            Debug.LogError("[Cybertruck] Importer not found for " + PrefabPath);
            return false;
        }
        importer.assetBundleName = BundleName;
        importer.SaveAndReimport();

        string outDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../AssetBundles/Android"));
        Directory.CreateDirectory(outDir);

        var manifest = BuildPipeline.BuildAssetBundles(outDir, BuildAssetBundleOptions.None, BuildTarget.Android);
        if (manifest == null)
        {
            Debug.LogError("[Cybertruck] AssetBundle build FAILED. Is the Android Build Support module installed?");
            return false;
        }
        Debug.Log("[Cybertruck] ✅ Bundle built at: " + outDir);
        return true;
    }

    // ---------------------------------------------------------------
    static Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform t in parent)
        {
            if (t.name == name) return t;
            var found = FindDeepChild(t, name);
            if (found != null) return found;
        }
        return null;
    }

    static void SetFloat(SerializedObject so, string prop, float value)
    {
        var p = so.FindProperty(prop);
        if (p != null) p.floatValue = value;
        else Debug.LogWarning("[Cybertruck] Property not found: " + prop);
    }
}
#endif
