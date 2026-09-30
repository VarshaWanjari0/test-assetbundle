using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class SetupBatmobileMod
{
    [MenuItem("Tools/Build Batmobile Mod")]
    public static void Build()
    {
        string prefabPath = "Assets/outsidemods/prefab/rgs.prefab";
        if (!File.Exists(prefabPath))
        {
            Debug.LogError("Prefab not found at: " + prefabPath);
            return;
        }

        // Tag all model assets first so AssetDatabase imports them with sub-meshes
        string[] modelFiles = new string[] {
            "Assets/Models/batmobile_body.obj",
            "Assets/Models/wheel_FL.obj",
            "Assets/Models/wheel_FR.obj",
            "Assets/Models/wheel_RL.obj",
            "Assets/Models/wheel_RR.obj"
        };

        foreach (string mf in modelFiles)
        {
            AssetDatabase.ImportAsset(mf, ImportAssetOptions.ForceUpdate);
        }

        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            Debug.Log("[SetupBatmobileMod] Configuring Batmobile on rgs.prefab...");

            // 1. Swap Truck Body Mesh on carzyCar/truck/Cargodoor_left
            Transform carzyCar = root.transform.Find("carzyCar");
            if (carzyCar != null)
            {
                carzyCar.localPosition = Vector3.zero;
                carzyCar.localRotation = Quaternion.identity;
                carzyCar.localScale = Vector3.one;

                Transform truck = carzyCar.Find("truck");
                if (truck != null)
                {
                    truck.localPosition = Vector3.zero;
                    truck.localRotation = Quaternion.identity;
                    truck.localScale = Vector3.one;

                    Transform body = truck.Find("Cargodoor_left");
                    if (body != null)
                    {
                        body.localPosition = Vector3.zero;
                        body.localRotation = Quaternion.identity;
                        body.localScale = Vector3.one;

                        Mesh bodyMesh = GetMeshFromAsset("Assets/Models/batmobile_body.obj");
                        if (bodyMesh != null)
                        {
                            MeshFilter mf = body.GetComponent<MeshFilter>();
                            if (mf != null) mf.sharedMesh = bodyMesh;

                            MeshRenderer mr = body.GetComponent<MeshRenderer>();
                            if (mr != null)
                            {
                                mr.enabled = true;
                                Material bodyMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Material/Body_white.mat");
                                if (bodyMat != null) mr.sharedMaterials = new Material[] { bodyMat };
                            }
                            Debug.Log("[SetupBatmobileMod] Body mesh assigned successfully!");
                        }
                    }
                }
            }

            // 2. Setup 4 Wheels with calibrated positions and sharedMesh assignment
            float wheelRadius = 0.71f;
            Vector3 posFL = new Vector3(-2.13f, 0.79f, 3.82f);
            Vector3 posFR = new Vector3( 2.13f, 0.79f, 3.82f);
            Vector3 posRL = new Vector3(-2.54f, 0.79f, -3.16f);
            Vector3 posRR = new Vector3( 2.54f, 0.79f, -3.16f);

            SetupWheel(root, "Wheel collider/Col FL", "Wheel Model/FL", "wheel_FL", posFL, wheelRadius, "Assets/Models/wheel_FL.obj");
            SetupWheel(root, "Wheel collider/Col FR", "Wheel Model/FR", "wheel_FL", posFR, wheelRadius, "Assets/Models/wheel_FR.obj");
            SetupWheel(root, "Wheel collider/Col RL", "Wheel Model/RL", "wheel_RL", posRL, wheelRadius, "Assets/Models/wheel_RL.obj");
            SetupWheel(root, "Wheel collider/Col RR", "Wheel Model/RR", "wheel_RL", posRR, wheelRadius, "Assets/Models/wheel_RR.obj");

            // 3. Hide old truck doors so they do not overlap
            Transform doorFL = root.transform.Find("Doors/DoorFL/Door_Right");
            if (doorFL != null)
            {
                MeshRenderer mr = doorFL.GetComponent<MeshRenderer>();
                if (mr != null) mr.enabled = false;
            }
            Transform doorFR = root.transform.Find("Doors/DoorFR/Door_Right (1)");
            if (doorFR != null)
            {
                MeshRenderer mr = doorFR.GetComponent<MeshRenderer>();
                if (mr != null) mr.enabled = false;
            }

            // 4. Cockpit & Interaction Points
            Transform doorPos = root.transform.Find("DoorPos");
            if (doorPos != null) doorPos.localPosition = new Vector3(2.60f, 0.25f, -0.40f);

            Transform sitPos = root.transform.Find("SitPosL");
            if (sitPos != null) sitPos.localPosition = new Vector3(0.45f, 1.05f, -0.40f);

            Transform interiorCam = root.transform.Find("Interior/InteriorCam");
            if (interiorCam != null) interiorCam.localPosition = new Vector3(0.45f, 1.35f, -0.35f);

            Transform cam = root.transform.Find("Cam");
            if (cam != null) cam.localPosition = new Vector3(0.0f, 3.20f, -7.50f);

            Transform steerDummy = root.transform.Find("steering_dummy");
            if (steerDummy != null) steerDummy.localPosition = new Vector3(0.45f, 1.20f, -0.10f);

            Transform smoke = root.transform.Find("ExhustedSmoke (1)");
            if (smoke != null) smoke.localPosition = new Vector3(0.0f, 0.90f, -6.10f); // Turbine exhaust

            Transform playerProtect = root.transform.Find("Player Protect");
            if (playerProtect != null)
            {
                playerProtect.localPosition = new Vector3(0.45f, 1.05f, -0.40f);
                BoxCollider ppCol = playerProtect.GetComponent<BoxCollider>();
                if (ppCol != null)
                {
                    ppCol.center = Vector3.zero;
                    ppCol.size = new Vector3(1.20f, 1.20f, 1.20f);
                }
            }

            Transform triggerKill = root.transform.Find("TriggerKill");
            if (triggerKill != null) triggerKill.localPosition = new Vector3(0.0f, 0.70f, 5.80f);

            // 5. Root Colliders (Main chassis + Door entry trigger)
            BoxCollider[] colliders = root.GetComponents<BoxCollider>();
            int nonTriggerCount = 0;
            foreach (var col in colliders)
            {
                if (col.isTrigger)
                {
                    col.center = new Vector3(2.60f, 0.80f, -0.40f);
                    col.size = new Vector3(1.80f, 1.50f, 2.00f);
                }
                else
                {
                    if (nonTriggerCount == 0)
                    {
                        col.center = new Vector3(0.0f, 1.20f, 0.0f);
                        col.size = new Vector3(3.60f, 1.80f, 11.50f);
                        nonTriggerCount++;
                    }
                    else
                    {
                        col.size = Vector3.zero;
                    }
                }
            }

            // 6. Tag all assets into AssetBundle 'rgs'
            string[] allFiles = Directory.GetFiles("Assets", "*.*", SearchOption.AllDirectories);
            foreach (string file in allFiles)
            {
                if (file.EndsWith(".meta") || file.EndsWith(".cs") || file.EndsWith(".unity")) continue;
                string uPath = file.Replace('\\', '/');
                AssetImporter imp = AssetImporter.GetAtPath(uPath);
                if (imp != null) imp.assetBundleName = "rgs";
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Debug.Log("[SetupBatmobileMod] 🎉 Successfully configured Batmobile rgs.prefab!");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void SetupWheel(GameObject root, string colPath, string modelPath, string meshChildName, Vector3 pos, float radius, string modelAsset)
    {
        Transform col = root.transform.Find(colPath);
        if (col != null)
        {
            col.localPosition = pos;
            WheelCollider wc = col.GetComponent<WheelCollider>();
            if (wc != null) wc.radius = radius;
        }

        Transform m = root.transform.Find(modelPath);
        if (m != null)
        {
            m.localPosition = pos;
            m.localRotation = Quaternion.identity;
            m.localScale = Vector3.one;

            Transform child = m.Find(meshChildName);
            if (child != null)
            {
                child.localPosition = Vector3.zero;
                child.localRotation = Quaternion.identity;
                child.localScale = Vector3.one;

                Mesh mesh = GetMeshFromAsset(modelAsset);
                if (mesh != null)
                {
                    MeshFilter mf = child.GetComponent<MeshFilter>();
                    if (mf != null) mf.sharedMesh = mesh;

                    MeshRenderer mr = child.GetComponent<MeshRenderer>();
                    if (mr != null)
                    {
                        mr.enabled = true;
                        Material tireMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Material/tier_1o.mat");
                        Material rimMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Material/wheel_truck_stamp_spec.mat");
                        if (tireMat != null && rimMat != null)
                        {
                            mr.sharedMaterials = new Material[] { tireMat, rimMat };
                        }
                    }
                    Debug.Log("[SetupBatmobileMod] Assigned wheel mesh to " + modelPath + "/" + meshChildName);
                }
            }
        }
    }

    private static Mesh GetMeshFromAsset(string path)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (Object obj in assets)
        {
            if (obj is Mesh m && !string.IsNullOrEmpty(m.name))
            {
                return m;
            }
        }
        foreach (Object obj in assets)
        {
            if (obj is Mesh m) return m;
        }
        Debug.LogError("No Mesh found at: " + path);
        return null;
    }
}
