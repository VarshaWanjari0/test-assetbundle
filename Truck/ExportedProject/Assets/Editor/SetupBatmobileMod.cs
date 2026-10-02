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
            Debug.Log("[SetupBatmobileMod] Restoring proven 5c75921 architecture (no falling on touch) with 2.5x larger sit button...");

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
                            Debug.Log("[SetupBatmobileMod] Body mesh assigned successfully!");
                        }
                    }
                }
            }

            // 2. Setup 4 Wheels with calibrated positions (identical to 5c75921)
            float wheelRadius = 0.66f;
            Vector3 posFL = new Vector3(-2.00f, 0.74f,  3.58f);
            Vector3 posFR = new Vector3( 2.00f, 0.74f,  3.58f);
            Vector3 posRL = new Vector3(-2.38f, 0.74f, -2.96f);
            Vector3 posRR = new Vector3( 2.38f, 0.74f, -2.96f);

            SetupWheel(root, "Wheel collider/Col FL", "Wheel Model/FL", "wheel_FL", posFL, wheelRadius, "Assets/Models/wheel_FL.obj");
            SetupWheel(root, "Wheel collider/Col FR", "Wheel Model/FR", "wheel_FL", posFR, wheelRadius, "Assets/Models/wheel_FR.obj");
            SetupWheel(root, "Wheel collider/Col RL", "Wheel Model/RL", "wheel_RL", posRL, wheelRadius, "Assets/Models/wheel_RL.obj");
            SetupWheel(root, "Wheel collider/Col RR", "Wheel Model/RR", "wheel_RL", posRR, wheelRadius, "Assets/Models/wheel_RL.obj");

            // 3. Hide old truck doors so they do not overlap (EXACTLY like 5c75921 - keep door colliders intact!)
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

            // 4. Cockpit Seating (Centered in Batmobile cabin, lowered 0.60m down inside cockpit)
            Vector3 cockpitSitPos = new Vector3(0.0f, -0.80f, -1.80f);
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
            if (interiorCam != null) interiorCam.localPosition = new Vector3(0.0f, 0.40f, 0.20f); // 0.60m up to driver eye level

            // Door Interaction Point & 2.5x Larger Trigger Area
            Transform doorPos = root.transform.Find("DoorPos");
            if (doorPos != null)
            {
                doorPos.localPosition = new Vector3(2.60f, 0.25f, -0.40f);
                doorPos.localScale = Vector3.one;
            }

            // Camera: Orbit geometric center of the Batmobile
            Transform cam = root.transform.Find("Cam");
            if (cam != null) cam.localPosition = new Vector3(0.0f, 1.60f, 0.0f);

            RidingCar rc = root.GetComponent<RidingCar>();
            if (rc != null)
            {
                rc.CamDis = 14;
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

            // Rear jet exhaust particle: 5x larger with electric cyan nitro flame!
            Transform smoke = root.transform.Find("ExhustedSmoke (1)");
            if (smoke != null)
            {
                smoke.localPosition = new Vector3(0.0f, 1.00f, -5.70f);
                smoke.localScale = new Vector3(5.0f, 5.0f, 5.0f); // 5x larger exhaust smoke!
                ParticleSystem ps = smoke.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var main = ps.main;
                    main.startSize = new ParticleSystem.MinMaxCurve(2.5f, 3.5f); // 5x larger particle size
                    main.startSpeed = new ParticleSystem.MinMaxCurve(8.0f, 18.0f); // High-speed nitro jet stream
                    main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
                    main.startColor = new ParticleSystem.MinMaxGradient(
                        new Color(0.15f, 0.75f, 1.0f, 0.95f), // Cyan electric nitro
                        new Color(0.35f, 0.90f, 1.0f, 1.0f)   // Glowing hot core
                    );
                }
                ParticleSystemRenderer psr = smoke.GetComponent<ParticleSystemRenderer>();
                if (psr != null)
                {
                    Material nitroMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Material/Smoke4.mat");
                    if (nitroMat != null)
                    {
                        nitroMat.SetColor("_TintColor", new Color(0.15f, 0.75f, 1.0f, 0.95f)); // Electric cyan nitro flame!
                        psr.sharedMaterial = nitroMat;
                    }
                }
            }

            // NO-FALL FIX: Player Protect solid shield starting 0.24m above road
            Transform playerProtect = root.transform.Find("Player Protect");
            if (playerProtect != null)
            {
                playerProtect.localPosition = new Vector3(0.45f, 1.05f, -0.40f);
                BoxCollider ppCol = playerProtect.GetComponent<BoxCollider>();
                if (ppCol != null)
                {
                    ppCol.center = Vector3.zero;
                    ppCol.size = new Vector3(1.20f, 1.20f, 1.20f);
                    ppCol.enabled = true;
                }
            }

            Transform triggerKill = root.transform.Find("TriggerKill");
            if (triggerKill != null) triggerKill.localPosition = new Vector3(0.0f, 0.70f, 5.80f);

            // 5. Root Colliders (Proven single solid chassis box + 2.5x larger sit button)
            BoxCollider[] colliders = root.GetComponents<BoxCollider>();
            int nonTriggerCount = 0;
            foreach (var col in colliders)
            {
                if (col.isTrigger)
                {
                    // 2.5x larger sit button trigger
                    col.center = new Vector3(2.80f, 1.00f, -0.40f);
                    col.size = new Vector3(4.50f, 2.50f, 5.00f);
                }
                else
                {
                    if (nonTriggerCount == 0)
                    {
                        // Clean solid chassis box that NEVER knocked down the player
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

            // 6. Indestructibility, 50% Tuned Acceleration (60k torque), Increased 8000 Mass, and Crash-Ejection Removal
            CarControl cc = root.GetComponent<CarControl>();
            if (cc != null)
            {
                cc.topSpeed = 5000f;
                cc.reverseSpeed = 5000f;
                cc.maxTorque = 60000f; // 50% acceleration (down from 120000f) for smooth heavy power
            }

            Rigidbody carRb = root.GetComponent<Rigidbody>();
            if (carRb != null)
            {
                carRb.mass = 8000f; // Increased car weight (8000 kg heavy armored Batmobile feel)
                carRb.drag = 0.005f; // Zero air drag so speed never drops during jumps or at high speeds!
                carRb.angularDrag = 1.0f;
            }

            ExplosionVehicle ev = root.GetComponent<ExplosionVehicle>();
            if (ev != null)
            {
                ev.health = 999999999;
                ev.enabled = false;
            }

            // Remove crash character ejection feature: disable CarImpactCheck and protect doors
            CarImpactCheck cic = root.GetComponent<CarImpactCheck>();
            if (cic != null)
            {
                cic.damage = false;
                cic.enabled = false; // Disable component so OnCollisionEnter crash ejection never fires!
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
                cjc.DropThisDoor = false; // Keep doors permanently attached on high-speed crash
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
            Debug.Log("[SetupBatmobileMod] 🎉 Successfully configured Batmobile matching proven 5c75921 architecture!");
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
                        if (tireMat != null)
                        {
                            tireMat.DisableKeyword("_EMISSION");
                            tireMat.SetColor("_Color", new Color(0.35f, 0.35f, 0.38f, 1f)); // Distinct lighter slate grey tire rubber
                            tireMat.SetColor("_EmissionColor", Color.black);
                            tireMat.SetFloat("_Metallic", 0.08f);
                            tireMat.SetFloat("_Glossiness", 0.30f);
                        }
                        if (rimMat != null)
                        {
                            rimMat.DisableKeyword("_EMISSION");
                            rimMat.SetColor("_Color", new Color(0.98f, 0.98f, 1.0f, 1f)); // Bright brilliant light alloy rims
                            rimMat.SetColor("_EmissionColor", Color.black);
                            rimMat.SetFloat("_Metallic", 0.96f);
                            rimMat.SetFloat("_Glossiness", 0.92f);
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
