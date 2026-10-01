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
            Debug.Log("[SetupBatmobileMod] Configuring Batmobile based on Truck.glb inspection: single unified solid body, freeze door rigidbodies, disable TriggerKill, 2.5x larger sit button...");

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

            // 2. Setup 4 Wheels for 75% size (radius = 0.66m) matching Truck.glb physics
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

            // 4. CRITICAL FIX: Freeze Truck Door Rigidbodies completely!
            // In Truck.glb, DoorFL and DoorFR have 250kg dynamic rigidbodies.
            // Disabling their colliders without freezing them allowed them to swing/drop under gravity and strike the player!
            Transform doors = root.transform.Find("Doors");
            if (doors != null)
            {
                foreach (var rb in doors.GetComponentsInChildren<Rigidbody>(true))
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                    rb.detectCollisions = false;
                    rb.mass = 0.001f;
                }
                foreach (var col in doors.GetComponentsInChildren<Collider>(true))
                {
                    col.enabled = false;
                }
                foreach (var mr in doors.GetComponentsInChildren<Renderer>(true))
                {
                    mr.enabled = false;
                }
            }

            // 5. Cockpit Seating: Tucked inside car with head safe
            Vector3 cockpitSitPos = new Vector3(0.0f, -0.65f, -1.80f);
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
            if (interiorCam != null) interiorCam.localPosition = new Vector3(0.0f, 1.60f, 0.0f);

            Transform playerProtect = root.transform.Find("Player Protect");
            if (playerProtect != null)
            {
                playerProtect.localPosition = new Vector3(0.0f, 1.00f, -1.80f);
                BoxCollider ppCol = playerProtect.GetComponent<BoxCollider>();
                if (ppCol != null)
                {
                    ppCol.enabled = false;
                }
            }

            // 6. Door Position & 2.5x Larger Sit Trigger
            // Placing DoorPos at (2.80, 0.40, 0.30) right beside the 2.40m car body
            Vector3 doorPosition = new Vector3(2.80f, 0.40f, 0.30f);
            Transform doorPos = root.transform.Find("DoorPos");
            if (doorPos != null)
            {
                doorPos.localPosition = doorPosition;
                doorPos.localScale = Vector3.one;
            }

            // Camera Look-at Pivot
            Transform cam = root.transform.Find("Cam");
            if (cam != null) cam.localPosition = new Vector3(0.0f, 1.60f, 0.0f);

            RidingCar rc = root.GetComponent<RidingCar>();
            if (rc != null) rc.CamDis = 14;

            // 7. Indestructibility, 75% Acceleration (60,000 Torque), and Powerful Braking
            CarControl cc = root.GetComponent<CarControl>();
            if (cc != null)
            {
                cc.topSpeed = 5000f;
                cc.reverseSpeed = 5000f;
                cc.maxTorque = 60000f; // 75% acceleration
            }

            Rigidbody carRb = root.GetComponent<Rigidbody>();
            if (carRb != null)
            {
                carRb.mass = 4000f;
                carRb.drag = 0.25f; // Controlled drag to aid powerful braking
                carRb.angularDrag = 1.5f;
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
                cic.enabled = false;
            }

            // Rear jet exhaust particle
            Transform smoke = root.transform.Find("ExhustedSmoke (1)");
            if (smoke != null) smoke.localPosition = new Vector3(0.0f, 1.00f, -5.70f);

            // 8. Disable TriggerKill completely so it can NEVER knock down the player on touch!
            Transform triggerKill = root.transform.Find("TriggerKill");
            if (triggerKill != null)
            {
                BoxCollider tkCol = triggerKill.GetComponent<BoxCollider>();
                if (tkCol != null)
                {
                    tkCol.size = Vector3.zero;
                    tkCol.enabled = false;
                }
                triggerKill.gameObject.SetActive(false);
            }

            // 9. Single Unified Solid Box Collider (Exactly Like Original Truck.glb!)
            // Truck.glb had ONE single 12.9m long box collider covering the entire body!
            // No stepped gaps, no seams where character controller can trip!
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

            // 2.5x Larger Sit Trigger covering the entire right side of the car
            triggerCol.center = new Vector3(3.20f, 1.20f, 0.30f);
            triggerCol.size = new Vector3(4.50f, 2.80f, 6.00f);

            while (solidCols.Count < 1)
            {
                BoxCollider newSolid = root.AddComponent<BoxCollider>();
                newSolid.isTrigger = false;
                solidCols.Add(newSolid);
            }

            // Main Unified Solid Body: Width 4.80m (shields all 4 wheels), Height 1.80m (Y 0.45 to 2.25m), Length 11.40m
            solidCols[0].center = new Vector3(0.0f, 1.35f, 0.35f);
            solidCols[0].size = new Vector3(4.80f, 1.80f, 11.40f);
            solidCols[0].enabled = true;
            solidCols[0].isTrigger = false;

            // Disable any extra colliders on root to maintain clean single-collider architecture like Truck.glb
            for (int i = 1; i < solidCols.Count; i++)
            {
                solidCols[i].size = Vector3.zero;
                solidCols[i].enabled = false;
            }

            // 10. Tag all assets into AssetBundle 'rgs'
            string[] allFiles = Directory.GetFiles("Assets", "*.*", SearchOption.AllDirectories);
            foreach (string file in allFiles)
            {
                if (file.EndsWith(".meta") || file.EndsWith(".cs") || file.EndsWith(".unity")) continue;
                string uPath = file.Replace('\\', '/');
                AssetImporter imp = AssetImporter.GetAtPath(uPath);
                if (imp != null) imp.assetBundleName = "rgs";
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Debug.Log("[SetupBatmobileMod] 🎉 Successfully configured Batmobile matching Truck.glb architecture!");
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
                js.spring = 45000f;
                js.damper = 5500f;
                js.targetPosition = 0.45f;
                wc.suspensionSpring = js;

                // High traction friction curves for powerful braking and grip
                wc.wheelDampingRate = 0.5f;
                WheelFrictionCurve ff = wc.forwardFriction;
                ff.stiffness = 2.0f;
                wc.forwardFriction = ff;

                WheelFrictionCurve sf = wc.sidewaysFriction;
                sf.stiffness = 1.5f;
                wc.sidewaysFriction = sf;
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
