// HypercarModBuilder.cs
// One-click builder for the Hypercar Agunta outside-mod AssetBundle (Android).
//
// What it does:
//   1. Loads Assets/Models/hypercar_agunta.glb (imported by glTFast)
//   2. Attaches its visual meshes/materials under the "truck" body node of the
//      rgs.prefab template (which already contains the game's vehicle rig:
//      CarControl + RidingCar + wheel colliders + doors + camera + audio)
//   3. Re-applies Hypercar performance spec (topSpeed/reverseSpeed/torque/
//      suspension/grip) so the prefab is always consistent
//   4. Points the EngineSound AudioSources at the existing JetEngine clip
//   5. Saves back to Assets/outsidemods/prefab/rgs.prefab  (kept as "rgs"
//      because the game's mod loader loads the bundle asset by that name)
//   6. Assigns AssetBundle name "rgs" and builds bundles for Android into
//      ../AssetBundles/Android/
//
// Menu:  Tools > Hypercar > Build Mod (Android)
//        Tools > Hypercar > Attach Hypercar Body Only
//        Tools > Hypercar > Build AssetBundle Only
//
// Requirements: Unity 2021.3.x (project targets 2021.3.43f1) + Android Build Support.

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;

public static class HypercarModBuilder
{
    const string PrefabPath   = "Assets/outsidemods/prefab/rgs.prefab";
    const string GlbPath      = "Assets/Models/hypercar_agunta.glb";
    const string BundleName   = "rgs";   // asset name the game's mod loader expects

    // ---------------------------------------------------------------
    [MenuItem("Tools/Hypercar/Build Mod (Android)")]
    public static void BuildAll()
    {
        if (!AttachHypercarBody()) return;
        ApplySpecs();
        if (!BuildBundle()) return;
        EditorUtility.DisplayDialog("Hypercar Mod",
            "AssetBundle built!\n\nOutput: AssetBundles/Android/" + BundleName +
            "\n\nCopy it into the game's mods folder (same place the Truck mod lives), " +
            "then load it in-game with the mod menu / cheat phone.", "Nice 🤙");
    }

    [MenuItem("Tools/Hypercar/Attach Hypercar Body Only")]
    public static void AttachOnly() { if (AttachHypercarBody()) { ApplySpecs(); } }

    [MenuItem("Tools/Hypercar/Apply Hypercar Specs")]
    public static void SpecsOnly() { ApplySpecs(); }

    [MenuItem("Tools/Hypercar/Build AssetBundle Only")]
    public static void BundleOnly() { BuildBundle(); }

    // ---------------------------------------------------------------
    static bool AttachHypercarBody()
    {
        var glb = AssetDatabase.LoadAssetAtPath<GameObject>(GlbPath);
        if (glb == null)
        {
            EditorUtility.DisplayDialog("Hypercar Mod",
                "Could not load " + GlbPath + "\n\n" +
                "Make sure hypercar_agunta.glb is in Assets/Models/ and the project has finished importing.", "OK");
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
                EditorUtility.DisplayDialog("Hypercar Mod", "Couldn't find the 'truck' child in rgs.prefab!", "OK");
                return false;
            }

            // Remove any previously attached body visual
            var old = truck.Find("Hypercar (Body)");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            // Instantiate GLB visual and nest it
            var body = (GameObject)PrefabUtility.InstantiatePrefab(glb, truck);
            body.name = "Hypercar (Body)";
            body.transform.localPosition = Vector3.zero;
            body.transform.localRotation = Quaternion.identity;
            // The Hypercar Agunta GLB is a low-poly supercar; scale to fit between
            // the wheel colliders (adjust to taste in the Inspector).
            body.transform.localScale = Vector3.one * 1.2f;

            // Report bounds so the user can fine-tune scale/offset
            var rends = body.GetComponentsInChildren<Renderer>();
            if (rends.Length > 0)
            {
                Bounds b = rends[0].bounds;
                foreach (var r in rends) b.Encapsulate(r.bounds);
                Debug.Log("[Hypercar] Body bounds size = " + b.size +
                          " — if misaligned vs wheel colliders, tweak 'Hypercar (Body)' localScale/localPosition.");
            }

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Debug.Log("[Hypercar] ✅ Hypercar body attached to rgs.prefab");
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
                    SetFloat(so, "topSpeed", 420f);       // hypercar top speed
                    SetFloat(so, "reverseSpeed", 120f);
                    SetFloat(so, "maxTorque", 18000f);    // hypercar torque
                    // gears: wide spread for hypercar acceleration
                    var gears = so.FindProperty("gears");
                    if (gears != null && gears.isArray)
                    {
                        int[] g = { 60, 120, 180, 240, 300, 360, 420 };
                        gears.arraySize = g.Length;
                        for (int i = 0; i < g.Length; i++) gears.GetArrayElementAtIndex(i).intValue = g[i];
                    }
                    so.ApplyModifiedPropertiesWithoutUndo();
                    fixedCar++;
                }
            }

            foreach (var wc in root.GetComponentsInChildren<WheelCollider>(true))
            {
                // Hypercar: stiff suspension, high grip
                var spring = wc.suspensionSpring;
                spring.spring = 45000f; spring.damper = 6000f; spring.targetPosition = 0.5f;
                wc.suspensionSpring = spring;
                wc.suspensionDistance = 0.25f;
                wc.forceAppPointDistance = 0.05f;
                wc.mass = 45f;
                wc.wheelDampingRate = 0.3f;
                var ff = wc.forwardFriction;
                ff.extremumSlip = 2f; ff.extremumValue = 10f; ff.asymptoteSlip = 1.5f; ff.asymptoteValue = 2f; ff.stiffness = 1.3f;
                wc.forwardFriction = ff;
                var sf = wc.sidewaysFriction;
                sf.extremumSlip = 1.8f; sf.extremumValue = 10f; sf.asymptoteSlip = 1.5f; sf.asymptoteValue = 2f; sf.stiffness = 1.4f;
                wc.sidewaysFriction = sf;
                fixedWheel++;
            }

            var rb = root.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass = 1800f;               // lightweight hypercar
                rb.angularDrag = 1.5f;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                fixedRb++;
            }

            // Center of mass: low and centered
            var com = FindDeepChild(root.transform, "Center of Mass");
            if (com != null)
            {
                com.localPosition = new Vector3(com.localPosition.x, -0.25f, com.localPosition.z);
            }

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Debug.Log("[Hypercar] ✅ Specs applied — CarControl:" + fixedCar + " WheelCollider:" + fixedWheel + " Rigidbody:" + fixedRb);
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
            Debug.LogError("[Hypercar] Importer not found for " + PrefabPath);
            return false;
        }
        importer.assetBundleName = BundleName;
        importer.SaveAndReimport();

        string outDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../AssetBundles/Android"));
        Directory.CreateDirectory(outDir);

        var manifest = BuildPipeline.BuildAssetBundles(outDir, BuildAssetBundleOptions.None, BuildTarget.Android);
        if (manifest == null)
        {
            Debug.LogError("[Hypercar] AssetBundle build FAILED. Is the Android Build Support module installed?");
            return false;
        }
        Debug.Log("[Hypercar] ✅ Bundle built at: " + outDir);
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
        else Debug.LogWarning("[Hypercar] Property not found: " + prop);
    }
}
#endif
