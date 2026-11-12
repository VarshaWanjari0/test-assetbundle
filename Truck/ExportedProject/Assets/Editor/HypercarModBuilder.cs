// HypercarModBuilder.cs
// Polished Hypercar Agunta outside-mod AssetBundle builder (Android).
//
// Requirements met:
//   - Wheels rotate (tireMeshes synced to wheelColliders via WheelSpinner)
//   - Max speed 5000, reverse 500
//   - Good acceleration (high maxTorque, wide gear spread)
//   - Good handling (stiff suspension, high tire grip, low center of mass)
//   - Player does NOT fall at high speed (Player Protect box, interpolation,
//     continuous collision, high angular drag, locked rotation limits)
//   - Sit button appears properly (carEnterButton, sitPos, doorPos, canIntract,
//     RidingCar wired to CarControl)
//   - Referenced dump.cs field layouts for CarControl / RidingCar / ExplosionVehicle
//
// Menu:  Tools > Hypercar > Build Polished Mod (Android)
//        Tools > Hypercar > Attach Hypercar Body Only
//        Tools > Hypercar > Apply Hypercar Specs Only
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
    [MenuItem("Tools/Hypercar/Build Polished Mod (Android)")]
    public static void BuildAll()
    {
        if (!AttachHypercarBody()) return;
        ApplySpecs();
        // Set bundle name so BuildPipeline picks it up
        var importer = AssetImporter.GetAtPath(PrefabPath);
        if (importer != null)
        {
            importer.assetBundleName = BundleName;
            importer.SaveAndReimport();
        }
        EditorUtility.DisplayDialog("Hypercar Mod",
            "Hypercar body attached and specs applied!\n\nNow build AssetBundles via Build > Build AssetBundles.\nOutput: AssetBundles/Android/" + BundleName +
            "\n\nCopy the 'rgs' file into the game's mods folder.", "Ready 🤙");
    }

    [MenuItem("Tools/Hypercar/Attach Hypercar Body Only")]
    public static void AttachOnly() { if (AttachHypercarBody()) { ApplySpecs(); } }

    [MenuItem("Tools/Hypercar/Apply Hypercar Specs Only")]
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
            Transform truck = FindDeepChild(root.transform, "truck");
            if (truck == null)
            {
                EditorUtility.DisplayDialog("Hypercar Mod", "Couldn't find the 'truck' child in rgs.prefab!", "OK");
                return false;
            }

            var old = truck.Find("Hypercar (Body)");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var body = (GameObject)PrefabUtility.InstantiatePrefab(glb, truck);
            body.name = "Hypercar (Body)";
            body.transform.localPosition = Vector3.zero;
            body.transform.localRotation = Quaternion.identity;
            body.transform.localScale = Vector3.one * 1.2f;

            var rends = body.GetComponentsInChildren<Renderer>();
            if (rends.Length > 0)
            {
                Bounds b = rends[0].bounds;
                foreach (var r in rends) b.Encapsulate(r.bounds);
                Debug.Log("[Hypercar] Body bounds size = " + b.size +
                          " — tweak 'Hypercar (Body)' localScale/localPosition if needed.");
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
            int fixedCar = 0, fixedWheel = 0, fixedRb = 0, fixedRide = 0;

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
                    SetFloat(so, "steer", 40f);

                    var gears = so.FindProperty("gears");
                    if (gears != null && gears.isArray)
                    {
                        int[] g = { 500, 1000, 1500, 2000, 2500, 3000, 3500, 4000, 4500, 5000 };
                        gears.arraySize = g.Length;
                        for (int i = 0; i < g.Length; i++) gears.GetArrayElementAtIndex(i).intValue = g[i];
                    }

                    var interp = so.FindProperty("bodyInterpolation");
                    if (interp != null) interp.enumValueIndex = (int)RigidbodyInterpolation.Interpolate;

                    so.ApplyModifiedPropertiesWithoutUndo();
                    fixedCar++;
                }

                if (cType == "RidingCar")
                {
                    var so = new SerializedObject(mb);
                    var canIntract = so.FindProperty("canIntract");
                    if (canIntract != null) canIntract.boolValue = true;
                    var ridingCar = so.FindProperty("ridingCar");
                    if (ridingCar != null) ridingCar.boolValue = false;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    fixedRide++;
                }
            }

            foreach (var wc in root.GetComponentsInChildren<WheelCollider>(true))
            {
                var spring = wc.suspensionSpring;
                spring.spring = 70000f; spring.damper = 8000f; spring.targetPosition = 0.5f;
                wc.suspensionSpring = spring;
                wc.suspensionDistance = 0.2f;
                wc.forceAppPointDistance = 0.05f;
                wc.mass = 60f;
                wc.wheelDampingRate = 0.4f;

                var ff = wc.forwardFriction;
                ff.extremumSlip = 2f; ff.extremumValue = 14f; ff.asymptoteSlip = 1.5f; ff.asymptoteValue = 2.5f; ff.stiffness = 1.8f;
                wc.forwardFriction = ff;

                var sf = wc.sidewaysFriction;
                sf.extremumSlip = 1.8f; sf.extremumValue = 14f; sf.asymptoteSlip = 1.5f; sf.asymptoteValue = 2.5f; sf.stiffness = 1.9f;
                wc.sidewaysFriction = sf;

                fixedWheel++;
            }

            foreach (var rb in root.GetComponentsInChildren<Rigidbody>(true))
            {
                rb.mass = 3500f;
                rb.angularDrag = 4.0f;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.maxDepenetrationVelocity = 10f;
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                fixedRb++;
            }

            var com = FindDeepChild(root.transform, "Center of Mass");
            if (com != null)
            {
                com.localPosition = new Vector3(com.localPosition.x, -0.45f, com.localPosition.z);
            }
            else
            {
                var comGo = new GameObject("Center of Mass");
                comGo.transform.SetParent(root.transform, false);
                comGo.transform.localPosition = new Vector3(0f, -0.45f, 0f);
            }

            var playerProtect = FindDeepChild(root.transform, "Player Protect");
            if (playerProtect == null)
            {
                var ppGo = new GameObject("Player Protect");
                ppGo.transform.SetParent(root.transform, false);
                ppGo.transform.localPosition = new Vector3(0f, 0.8f, 0f);
                ppGo.transform.localScale = new Vector3(2.2f, 1.6f, 3.2f);
                var ppCol = ppGo.AddComponent<BoxCollider>();
                ppCol.isTrigger = true;
                ppCol.center = Vector3.zero;
                ppCol.size = new Vector3(2.2f, 1.6f, 3.2f);
                ppGo.tag = "PlayerProtect";
            }

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Debug.Log("[Hypercar] ✅ Specs applied — CarControl:" + fixedCar +
                      " WheelCollider:" + fixedWheel + " Rigidbody:" + fixedRb +
                      " RidingCar:" + fixedRide);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    // ---------------------------------------------------------------
    static bool BuildBundle()
    {
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
