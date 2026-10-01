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

        // Force import model assets
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
            Debug.Log("[SetupBatmobileMod] Configuring 2X Batmobile on rgs.prefab...");

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
                                Material batMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Material/Batmobile_Body.mat");
                                if (batMat != null)
                                {
                                    mr.sharedMaterials = new Material[] { batMat };
                                }
                            }
                            Debug.Log("[SetupBatmobileMod] Dark stealth Batmobile body mesh assigned!");
                        }
                    }
                }
            }

            // 2. Setup 4 Wheels with 2X scale positions and radius = 1.42m
            float wheelRadius = 1.42f;
            Vector3 posFL = new Vector3(-4.26f, 1.57f,  7.64f);
            Vector3 posFR = new Vector3( 4.26f, 1.57f,  7.64f);
            Vector3 posRL = new Vector3(-5.08f, 1.57f, -6.32f);
            Vector3 posRR = new Vector3( 5.08f, 1.57f, -6.32f);

            SetupWheel(root, "Wheel collider/Col FL", "Wheel Model/FL", "wheel_FL", posFL, wheelRadius, "Assets/Models/wheel_FL.obj");
            SetupWheel(root, "Wheel collider/Col FR", "Wheel Model/FR", "wheel_FL", posFR, wheelRadius, "Assets/Models/wheel_FR.obj");
            SetupWheel(root, "Wheel collider/Col RL", "Wheel Model/RL", "wheel_RL", posRL, wheelRadius, "Assets/Models/wheel_RL.obj");
            SetupWheel(root, "Wheel collider/Col RR", "Wheel Model/RR", "wheel_RL", posRR, wheelRadius, "Assets/Models/wheel_RR.obj");

            // 3. Remove/Hide steering model as requested
            Transform steerDummy = root.transform.Find("steering_dummy");
            if (steerDummy != null)
            {
                foreach (Renderer r in steerDummy.GetComponentsInChildren<Renderer>(true))
                {
                    r.enabled = false;
                }
                steerDummy.localScale = Vector3.zero;
            }

            // Hide old truck doors
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

            // 4. Make Player sit inside deep in the back (Z = -3.5m) so character is completely invisible inside
            Vector3 invisibleSitPos = new Vector3(0.0f, 1.80f, -3.50f);
            Transform sitPos = root.transform.Find("SitPosL");
            if (sitPos != null) sitPos.localPosition = invisibleSitPos;

            Transform leftFoot = root.transform.Find("LeftFoot");
            if (leftFoot != null) leftFoot.localPosition = invisibleSitPos;

            Transform rightFoot = root.transform.Find("RightFoot");
            if (rightFoot != null) rightFoot.localPosition = invisibleSitPos;

            Transform interior = root.transform.Find("Interior");
            if (interior != null) interior.localPosition = invisibleSitPos;

            Transform interiorCam = root.transform.Find("Interior/InteriorCam");
            if (interiorCam != null) interiorCam.localPosition = new Vector3(0.0f, 0.40f, 0.20f);

            Transform playerProtect = root.transform.Find("Player Protect");
            if (playerProtect != null)
            {
                playerProtect.localPosition = invisibleSitPos;
                BoxCollider ppCol = playerProtect.GetComponent<BoxCollider>();
                if (ppCol != null)
                {
                    ppCol.center = Vector3.zero;
                    ppCol.size = new Vector3(2.50f, 2.00f, 2.50f);
                }
            }

            // Door approach position on the right side
            Transform doorPos = root.transform.Find("DoorPos");
            if (doorPos != null) doorPos.localPosition = new Vector3(5.50f, 0.50f, 0.00f);

            // 5. Fix Rotating Camera: Orbit geometric center of the car (0, 2.6, 0)
            Transform cam = root.transform.Find("Cam");
            if (cam != null) cam.localPosition = new Vector3(0.0f, 2.60f, 0.00f);

            RidingCar rc = root.GetComponent<RidingCar>();
            if (rc != null) rc.CamDis = 26; // Increased distance to frame 2X car perfectly

            // Rear jet exhaust particle
            Transform smoke = root.transform.Find("ExhustedSmoke (1)");
            if (smoke != null) smoke.localPosition = new Vector3(0.0f, 1.80f, -12.20f);

            Transform triggerKill = root.transform.Find("TriggerKill");
            if (triggerKill != null) triggerKill.localPosition = new Vector3(0.0f, 1.40f, 11.50f);

            // 6. Resize Hitbox (Solid BoxCollider + Door Entry Trigger)
            BoxCollider[] colliders = root.GetComponents<BoxCollider>();
            int nonTriggerCount = 0;
            foreach (var col in colliders)
            {
                if (col.isTrigger)
                {
                    // Door Entry Trigger
                    col.center = new Vector3(5.50f, 1.50f, 0.00f);
                    col.size = new Vector3(4.00f, 3.00f, 6.00f);
                }
                else
                {
                    if (nonTriggerCount == 0)
                    {
                        // 2X Batmobile Main Solid Hitbox: 9.5m wide, 3.8m high, 23.5m long
                        col.center = new Vector3(0.0f, 2.60f, 0.0f);
                        col.size = new Vector3(9.50f, 3.80f, 23.50f);
                        nonTriggerCount++;
                    }
                    else
                    {
                        col.size = Vector3.zero;
                    }
                }
            }

            // 7. Tag all assets into AssetBundle 'rgs'
            string[] allFiles = Directory.GetFiles("Assets", "*.*", SearchOption.AllDirectories);
            foreach (string file in allFiles)
            {
                if (file.EndsWith(".meta") || file.EndsWith(".cs") || file.EndsWith(".unity")) continue;
                string uPath = file.Replace('\\', '/');
                AssetImporter imp = AssetImporter.GetAtPath(uPath);
                if (imp != null) imp.assetBundleName = "rgs";
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Debug.Log("[SetupBatmobileMod] 🎉 Successfully configured 2X Batmobile with stealth paint, dual-texture wheels, center camera, and invisible cockpit seating!");
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
            if (wc != null)
            {
                wc.radius = radius;
                wc.suspensionDistance = 0.5f;
            }
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
                    Debug.Log("[SetupBatmobileMod] Assigned dual-material wheel mesh to " + modelPath + "/" + meshChildName);
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
