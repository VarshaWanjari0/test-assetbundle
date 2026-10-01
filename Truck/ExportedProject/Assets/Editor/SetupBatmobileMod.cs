using System;
using System.Collections.Generic;
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
            Debug.Log("[SetupBatmobileMod] Configuring Batmobile from working baseline with stable sit button and proper seating...");

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
                        body.localPosition = new Vector3(0.0f, 0.10f, 0.0f);
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
                            Debug.Log("[SetupBatmobileMod] Batmobile body assigned successfully!");
                        }
                    }
                }
            }

            // 2. Setup 4 Wheels for 75% size (radius = 0.66m)
            float wheelRadius = 0.66f;
            Vector3 posFL = new Vector3(-2.00f, 0.74f,  3.58f);
            Vector3 posFR = new Vector3( 2.00f, 0.74f,  3.58f);
            Vector3 posRL = new Vector3(-2.38f, 0.74f, -2.96f);
            Vector3 posRR = new Vector3( 2.38f, 0.74f, -2.96f);

            SetupWheel(root, "Wheel collider/Col FL", "Wheel Model/FL", "wheel_FL", posFL, wheelRadius, "Assets/Models/wheel_FL.obj");
            SetupWheel(root, "Wheel collider/Col FR", "Wheel Model/FR", "wheel_FL", posFR, wheelRadius, "Assets/Models/wheel_FR.obj");
            SetupWheel(root, "Wheel collider/Col RL", "Wheel Model/RL", "wheel_RL", posRL, wheelRadius, "Assets/Models/wheel_RL.obj");
            SetupWheel(root, "Wheel collider/Col RR", "Wheel Model/RR", "wheel_RL", posRR, wheelRadius, "Assets/Models/wheel_RL.obj");

            // 3. Remove steering wheel model
            Transform steerDummy = root.transform.Find("steering_dummy");
            if (steerDummy != null)
            {
                foreach (Renderer r in steerDummy.GetComponentsInChildren<Renderer>(true))
                {
                    r.enabled = false;
                }
                steerDummy.localScale = Vector3.zero;
            }

            // Disable truck doors and their colliders completely so no invisible barriers exist
            Transform doors = root.transform.Find("Doors");
            if (doors != null)
            {
                foreach (var mr in doors.GetComponentsInChildren<MeshRenderer>(true))
                {
                    mr.enabled = false;
                }
                foreach (var col in doors.GetComponentsInChildren<BoxCollider>(true))
                {
                    col.size = Vector3.zero;
                    col.enabled = false;
                }
            }

            // 4. Cockpit Seating: Tucked cleanly inside the cockpit floorpan (0.45m above ground, NOT subterranean!)
            Vector3 cockpitSitPos = new Vector3(0.0f, 0.45f, -1.80f);
            Transform sitPos = root.transform.Find("SitPosL");
            if (sitPos != null) sitPos.localPosition = cockpitSitPos;

            Transform leftFoot = root.transform.Find("LeftFoot");
            if (leftFoot != null) leftFoot.localPosition = cockpitSitPos;

            Transform rightFoot = root.transform.Find("RightFoot");
            if (rightFoot != null) rightFoot.localPosition = cockpitSitPos;

            Transform interior = root.transform.Find("Interior");
            if (interior != null) interior.localPosition = cockpitSitPos;

            Transform interiorCam = root.transform.Find("Interior/InteriorCam");
            if (interiorCam != null) interiorCam.localPosition = new Vector3(0.0f, 1.60f, 0.0f);

            Transform playerProtect = root.transform.Find("Player Protect");
            if (playerProtect != null)
            {
                playerProtect.localPosition = new Vector3(0.0f, 1.00f, -1.80f);
                BoxCollider ppCol = playerProtect.GetComponent<BoxCollider>();
                if (ppCol != null)
                {
                    // Disable player protect collider so it NEVER collides with or trips the player
                    ppCol.enabled = false;
                }
            }

            // 5. Stable Right-Side Door Entry Area & Co-located DoorPos (From Last Working Version)
            Vector3 doorPosition = new Vector3(2.00f, 0.25f, 0.30f);
            Transform doorPos = root.transform.Find("DoorPos");
            if (doorPos != null)
            {
                doorPos.localPosition = doorPosition;
                doorPos.localScale = Vector3.one; // Standard scale so distance checks work 100% reliably
            }

            // Camera Look-at Pivot: Geometric center of vehicle (0, 1.60, 0)
            Transform cam = root.transform.Find("Cam");
            if (cam != null) cam.localPosition = new Vector3(0.0f, 1.60f, 0.0f);

            RidingCar rc = root.GetComponent<RidingCar>();
            if (rc != null) rc.CamDis = 14; // Framed for 11.4m vehicle

            // 5b. Indestructibility & Extreme 5000 Speed / Reverse
            CarControl cc = root.GetComponent<CarControl>();
            if (cc != null)
            {
                cc.topSpeed = 5000f;
                cc.reverseSpeed = 5000f;
                cc.maxTorque = 80000f;
            }

            ExplosionVehicle ev = root.GetComponent<ExplosionVehicle>();
            if (ev != null)
            {
                ev.health = 999999999;
                ev.enabled = false;
            }

            CarImpactCheck cic = root.GetComponent<CarImpactCheck>();
            if (cic != null)
            {
                cic.damage = false;
            }

            // Rear jet exhaust particle
            Transform smoke = root.transform.Find("ExhustedSmoke (1)");
            if (smoke != null) smoke.localPosition = new Vector3(0.0f, 1.00f, -5.70f);

            // Front bumper kill trigger on NPCs (reset localPosition to zero so it doesn't double-offset)
            Transform triggerKill = root.transform.Find("TriggerKill");
            if (triggerKill != null)
            {
                triggerKill.localPosition = Vector3.zero;
                BoxCollider tkCol = triggerKill.GetComponent<BoxCollider>();
                if (tkCol != null)
                {
                    tkCol.center = new Vector3(0.0f, 0.80f, 5.50f);
                    tkCol.size = new Vector3(3.00f, 0.80f, 0.60f);
                }
            }

            // 6. Solid Hitboxes & Right-Side Sit Trigger (From Last Working Version)
            // Solid colliders strictly INSET inside the 3D visual mesh:
            // - Mid body collider width 3.00m (inside 3.10m car body). Player walks directly to door with zero barrier!
            // - Front nose collider width 3.40m inside hood.
            // - Rear chassis collider width 3.80m, ending at Z = -4.90m (zero free-space overhang behind car).
            List<BoxCollider> solidCols = new List<BoxCollider>();
            BoxCollider triggerCol = null;

            foreach (var col in root.GetComponents<BoxCollider>())
            {
                if (col.isTrigger)
                {
                    if (triggerCol == null) triggerCol = col;
                }
                else
                {
                    solidCols.Add(col);
                }
            }

            if (triggerCol == null)
            {
                triggerCol = root.AddComponent<BoxCollider>();
                triggerCol.isTrigger = true;
            }

            // Trigger identical to proven working version 8969078
            triggerCol.center = new Vector3(2.20f, 0.80f, 0.30f);
            triggerCol.size = new Vector3(2.60f, 1.80f, 3.00f);

            while (solidCols.Count < 3)
            {
                BoxCollider newSolid = root.AddComponent<BoxCollider>();
                newSolid.isTrigger = false;
                solidCols.Add(newSolid);
            }

            // Collider 1: Front Nose & Hood
            solidCols[0].center = new Vector3(0.0f, 1.15f, 4.00f);
            solidCols[0].size = new Vector3(3.40f, 1.00f, 3.00f);

            // Collider 2: Mid Body & Cabin (width 3.00m allows player to reach door position 2.00m without tripping)
            solidCols[1].center = new Vector3(0.0f, 1.45f, 0.70f);
            solidCols[1].size = new Vector3(3.00f, 1.60f, 3.80f);

            // Collider 3: Rear Body & Fins (tightly stops at Z = -4.90m, zero overhang)
            solidCols[2].center = new Vector3(0.0f, 1.45f, -3.10f);
            solidCols[2].size = new Vector3(3.80f, 1.60f, 3.60f);

            for (int i = 3; i < solidCols.Count; i++)
            {
                solidCols[i].size = Vector3.zero;
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
            Debug.Log("[SetupBatmobileMod] 🎉 Successfully configured Batmobile from working baseline!");
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
                wc.suspensionDistance = 0.30f;
                JointSpring js = wc.suspensionSpring;
                js.spring = 55000f;
                js.damper = 6500f;
                js.targetPosition = 0.40f;
                wc.suspensionSpring = js;
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
                        if (tireMat != null)
                        {
                            tireMat.DisableKeyword("_EMISSION");
                            tireMat.SetColor("_Color", new Color(0.12f, 0.12f, 0.12f, 1f));
                            tireMat.SetColor("_EmissionColor", Color.black);
                            tireMat.SetFloat("_Metallic", 0.05f);
                            tireMat.SetFloat("_Glossiness", 0.25f);
                        }
                        if (rimMat != null)
                        {
                            rimMat.DisableKeyword("_EMISSION");
                            rimMat.SetColor("_Color", new Color(0.92f, 0.92f, 0.94f, 1f));
                            rimMat.SetColor("_EmissionColor", Color.black);
                            rimMat.SetFloat("_Metallic", 0.92f);
                            rimMat.SetFloat("_Glossiness", 0.85f);
                        }
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
