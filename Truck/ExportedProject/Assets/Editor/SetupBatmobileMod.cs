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

        // Force import Cybertruck model assets
        string[] modelFiles = new string[] {
            "Assets/Models/cybertruck_body.obj",
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
            Debug.Log("[SetupCyberTruckMod] Configuring CyberTruck vehicle...");

            // 1. Swap Body Mesh on carzyCar/truck/Cargodoor_left
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

                        Mesh bodyMesh = GetMeshFromAsset("Assets/Models/cybertruck_body.obj");
                        if (bodyMesh != null)
                        {
                            MeshFilter mf = body.GetComponent<MeshFilter>();
                            if (mf != null) mf.sharedMesh = bodyMesh;

                            MeshRenderer mr = body.GetComponent<MeshRenderer>();
                            if (mr != null)
                            {
                                mr.enabled = true;
                                Material bodyMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Material/Body_white.mat");
                                if (bodyMat != null)
                                {
                                    // Stainless steel Cybertruck finish
                                    bodyMat.SetColor("_Color", new Color(0.82f, 0.84f, 0.86f, 1f));
                                    bodyMat.SetFloat("_Metallic", 0.90f);
                                    bodyMat.SetFloat("_Glossiness", 0.75f);
                                    mr.sharedMaterials = new Material[] { bodyMat };
                                }
                            }
                            Debug.Log("[SetupCyberTruckMod] Cybertruck body mesh assigned!");
                        }
                    }
                }
            }

            // 2. Setup 4 Rotating Wheels with Cybertruck Wheelbase and Track Width
            float wheelRadius = 0.45f;
            Vector3 posFL = new Vector3(-1.00f, 0.70f,  1.60f);
            Vector3 posFR = new Vector3( 1.00f, 0.70f,  1.60f);
            Vector3 posRL = new Vector3(-1.00f, 0.70f, -2.07f);
            Vector3 posRR = new Vector3( 1.00f, 0.70f, -2.07f);

            SetupWheel(root, "Wheel collider/Col FL", "Wheel Model/FL", "wheel_FL", posFL, wheelRadius, "Assets/Models/wheel_FL.obj");
            SetupWheel(root, "Wheel collider/Col FR", "Wheel Model/FR", "wheel_FL", posFR, wheelRadius, "Assets/Models/wheel_FR.obj");
            SetupWheel(root, "Wheel collider/Col RL", "Wheel Model/RL", "wheel_RL", posRL, wheelRadius, "Assets/Models/wheel_RL.obj");
            SetupWheel(root, "Wheel collider/Col RR", "Wheel Model/RR", "wheel_RL", posRR, wheelRadius, "Assets/Models/wheel_RR.obj");

            // 3. Hide old truck doors
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

            // 4. Cockpit Seating & Cameras
            Vector3 cockpitSitPos = new Vector3(-0.42f, 0.88f, 0.20f);
            Transform sitPos = root.transform.Find("SitPosL");
            if (sitPos != null)
            {
                sitPos.localPosition = cockpitSitPos;
                sitPos.localScale = new Vector3(0.85f, 0.85f, 0.85f);
            }

            Transform leftFoot = root.transform.Find("LeftFoot");
            if (leftFoot != null) leftFoot.localPosition = cockpitSitPos;

            Transform rightFoot = root.transform.Find("RightFoot");
            if (rightFoot != null) rightFoot.localPosition = cockpitSitPos;

            Transform interior = root.transform.Find("Interior");
            if (interior != null) interior.localPosition = cockpitSitPos;

            Transform interiorCam = root.transform.Find("Interior/InteriorCam");
            if (interiorCam != null) interiorCam.localPosition = new Vector3(-0.42f, 1.25f, 0.35f);

            Transform doorPos = root.transform.Find("DoorPos");
            if (doorPos != null)
            {
                doorPos.localPosition = new Vector3(1.30f, 0.50f, 0.20f);
                doorPos.localScale = Vector3.one;
            }

            Transform cam = root.transform.Find("Cam");
            if (cam != null) cam.localPosition = new Vector3(0.0f, 1.60f, 0.20f);

            RidingCar rc = root.GetComponent<RidingCar>();
            if (rc != null)
            {
                rc.CamDis = 10;
                rc.frontGlass = null;
            }

            Transform steerDummy = root.transform.Find("steering_dummy");
            if (steerDummy != null)
            {
                foreach (Renderer r in steerDummy.GetComponentsInChildren<Renderer>(true))
                {
                    r.enabled = false;
                }
                steerDummy.localScale = Vector3.zero;
            }

            // Player Protect Solid Shield
            Transform playerProtect = root.transform.Find("Player Protect");
            if (playerProtect != null)
            {
                playerProtect.localPosition = new Vector3(0.0f, 1.05f, 0.20f);
                BoxCollider ppCol = playerProtect.GetComponent<BoxCollider>();
                if (ppCol != null)
                {
                    ppCol.center = Vector3.zero;
                    ppCol.size = new Vector3(1.85f, 1.45f, 2.40f);
                    ppCol.enabled = true;
                }
            }

            // Disable TriggerKill
            Transform triggerKill = root.transform.Find("TriggerKill");
            if (triggerKill != null) triggerKill.gameObject.SetActive(false);

            // 5. Root Colliders (Solid chassis box + interaction trigger)
            BoxCollider[] colliders = root.GetComponents<BoxCollider>();
            int nonTriggerCount = 0;
            foreach (var col in colliders)
            {
                if (col.isTrigger)
                {
                    col.center = new Vector3(1.30f, 0.80f, 0.20f);
                    col.size = new Vector3(2.50f, 2.00f, 3.50f);
                }
                else
                {
                    if (nonTriggerCount == 0)
                    {
                        col.center = new Vector3(0.0f, 1.15f, 0.0f);
                        col.size = new Vector3(2.20f, 1.30f, 5.60f);
                        nonTriggerCount++;
                    }
                    else
                    {
                        col.size = Vector3.zero;
                    }
                }
            }

            // 6. Indestructibility, 5000 Top Speed, 500 Reverse Speed, Good Acceleration, Mass 4000
            CarControl cc = root.GetComponent<CarControl>();
            if (cc != null)
            {
                cc.topSpeed = 5000f;
                cc.reverseSpeed = 500f;
                cc.maxTorque = 68000f; // Good acceleration
            }

            Rigidbody carRb = root.GetComponent<Rigidbody>();
            if (carRb != null)
            {
                carRb.mass = 4000f; // 4000 kg mass
                carRb.drag = 0.005f; // Zero air drag so speed never drops during jumps
                carRb.angularDrag = 1.0f;
            }

            ExplosionVehicle ev = root.GetComponent<ExplosionVehicle>();
            if (ev != null)
            {
                ev.health = 999999999;
                ev.enabled = false;
            }

            // Prevent crash character ejection
            CarImpactCheck cic = root.GetComponent<CarImpactCheck>();
            if (cic != null)
            {
                cic.damage = false;
                cic.enabled = false;
                cic._colDist = 999999f;
                cic.glassImpact = null;
                cic.bodyImpactBig = null;
                cic.bodyImpactSmall = null;
                cic.sparkImpact = null;
                cic.crashSound = null;
                cic.smallCrash = new AudioClip[0];
                cic.mediumCrash = new AudioClip[0];
                cic.largeCrash = new AudioClip[0];
            }

            CarJointControl[] cjcs = root.GetComponentsInChildren<CarJointControl>(true);
            foreach (var cjc in cjcs)
            {
                cjc.DropThisDoor = false;
                cjc.health = 999999999;
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
            Debug.Log("[SetupCyberTruckMod] 🎉 Successfully configured CyberTruck vehicle!");
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
                var spring = wc.suspensionSpring;
                spring.spring = 45000f;
                spring.damper = 5500f;
                spring.targetPosition = 0.40f;
                wc.suspensionSpring = spring;
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
                            tireMat.SetColor("_Color", new Color(0.20f, 0.20f, 0.22f, 1f));
                            tireMat.SetFloat("_Metallic", 0.10f);
                            tireMat.SetFloat("_Glossiness", 0.25f);
                        }
                        if (rimMat != null)
                        {
                            rimMat.SetColor("_Color", new Color(0.12f, 0.12f, 0.14f, 1f));
                            rimMat.SetFloat("_Metallic", 0.85f);
                            rimMat.SetFloat("_Glossiness", 0.70f);
                        }
                        if (tireMat != null && rimMat != null)
                        {
                            mr.sharedMaterials = new Material[] { tireMat, rimMat };
                        }
                    }
                    Debug.Log("[SetupCyberTruckMod] Assigned wheel mesh to " + modelPath + "/" + meshChildName);
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
