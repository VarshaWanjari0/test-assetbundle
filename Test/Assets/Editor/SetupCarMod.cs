using UnityEditor;
using UnityEngine;
using System.IO;

public class SetupCarMod
{
    // Call this method from your existing workflow!
    [MenuItem("Tools/Mod Car Prefab")]
    public static void Build()
    {
        string modelPath = "Assets/Models/newcar.obj";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(modelPath) == null)
        {
            modelPath = "Assets/Models/newcar.glb";
        }
        GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (modelAsset == null)
        {
            Debug.LogError("❌ Could not find model at: " + modelPath);
            return;
        }

        string prefabPath = "Assets/outsidemods/prefab/rgs.prefab";
        string prefabFolder = Path.GetDirectoryName(prefabPath);
        if (!Directory.Exists(prefabFolder)) Directory.CreateDirectory(prefabFolder);

        // 1. Create Root Car Object
        GameObject root = new GameObject("rgs");

        // 2. Instantiate Model as Child (90 deg rotation correction for Mustang)
        GameObject carModel = Object.Instantiate(modelAsset, root.transform);
        carModel.name = "Model";
        carModel.transform.localPosition = Vector3.zero;
        carModel.transform.localRotation = Quaternion.Euler(0, 90, 0);

        // 3. Physics Rigidbody & Body Hitbox
        Rigidbody rb = root.AddComponent<Rigidbody>();
        rb.mass = 1500f;
        rb.drag = 0.05f;
        rb.angularDrag = 0.05f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        BoxCollider box = root.AddComponent<BoxCollider>();
        box.size = new Vector3(1.85f, 1.25f, 4.15f);
        box.center = new Vector3(0, 0.65f, 0);

        // 4. Create Wheel Colliders
        GameObject wheelColParent = new GameObject("Wheel collider");
        wheelColParent.transform.SetParent(root.transform, false);

        WheelCollider[] colliders = new WheelCollider[4];
        string[] colNames = { "Col FL", "Col FR", "Col RL", "Col RR" };
        Vector3[] colPositions = {
            new Vector3(-0.85f, 0.35f,  1.35f),
            new Vector3( 0.85f, 0.35f,  1.35f),
            new Vector3(-0.85f, 0.35f, -1.35f),
            new Vector3( 0.85f, 0.35f, -1.35f)
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject colObj = new GameObject(colNames[i]);
            colObj.transform.SetParent(wheelColParent.transform, false);
            colObj.transform.localPosition = colPositions[i];
            
            WheelCollider wc = colObj.AddComponent<WheelCollider>();
            wc.radius = 0.33f;
            wc.mass = 35f;
            JointSpring spr = wc.suspensionSpring;
            spr.spring = 35000f;
            spr.damper = 4500f;
            spr.targetPosition = 0.5f;
            wc.suspensionSpring = spr;
            wc.suspensionDistance = 0.15f;
            colliders[i] = wc;
        }

        // 5. Connect Maureen67 Visual Wheels
        Transform[] tireMeshes = new Transform[4];
        Transform[] allTransforms = carModel.GetComponentsInChildren<Transform>();
        foreach (Transform t in allTransforms)
        {
            string n = t.name.ToLower();
            if (n.Contains("fl")) tireMeshes[0] = t;
            else if (n.Contains("fr")) tireMeshes[1] = t;
            else if (n.Contains("rl")) tireMeshes[2] = t;
            else if (n.Contains("rr")) tireMeshes[3] = t;
        }

        // 6. Anchors: Seat, Door, Camera
        GameObject sitPos = new GameObject("SitPosL");
        sitPos.transform.SetParent(root.transform, false);
        sitPos.transform.localPosition = new Vector3(-0.35f, 0.55f, 0.05f);

        GameObject doorPos = new GameObject("DoorPos");
        doorPos.transform.SetParent(root.transform, false);
        doorPos.transform.localPosition = new Vector3(-1.15f, 0.15f, 0.15f);

        GameObject camTarget = new GameObject("Cam");
        camTarget.transform.SetParent(root.transform, false);
        camTarget.transform.localPosition = new Vector3(0, 1.2f, 0);

        // 7. Attach & Link Restored Vehicle Scripts
        CarControl cc = root.AddComponent<CarControl>();
        cc.wheelColliders = colliders;
        cc.tireMeshes = tireMeshes;
        cc.m_rigidBody = rb;
        cc.topSpeed = 220f;
        cc.maxTorque = 12000f;

        RidingCar rc = root.AddComponent<RidingCar>();
        rc.sitPos = sitPos.transform;
        rc.doorPos = doorPos.transform;
        rc.camTarget = camTarget.transform;
        rc._car = cc;
        rc.canIntract = true;

        ExplosionVehicle ev = root.AddComponent<ExplosionVehicle>();
        ev.health = 100f;
        ev.explosionForce = 5000f;
        ev.explosionRadius = 5f;

        CarImpactCheck cic = root.AddComponent<CarImpactCheck>();
        cic.damage = 1;
        cic._colDist = 8f;

        // 8. Overwrite existing rgs.prefab with new Mustang setup
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);

        // 9. Assign AssetBundle Name to replace original rgs bundle
        AssetImporter importer = AssetImporter.GetAtPath(prefabPath);
        if (importer != null)
        {
            importer.assetBundleName = "rgs";
        }

        // 10. Build AssetBundle for Android
        string buildDir = "Assets/AssetBundles";
        if (!Directory.Exists(buildDir)) Directory.CreateDirectory(buildDir);
        BuildPipeline.BuildAssetBundles(buildDir, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);

        string altBuildDir = "Builds";
        if (!Directory.Exists(altBuildDir)) Directory.CreateDirectory(altBuildDir);
        BuildPipeline.BuildAssetBundles(altBuildDir, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);

        Debug.Log("🎉 NEW CAR PREFAB (rgs.prefab) UPDATED & ASSETBUNDLE BAKED SUCCESSFULLY!");
    }
}
