using UnityEditor;
using UnityEngine;
using System.IO;

public class SetupCarMod
{
    [MenuItem("Tools/Setup Car Mod")]
    public static void Build()
    {
        string prefabPath = "Assets/outsidemods/prefab/rgs.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError("Could not find prefab at " + prefabPath);
            return;
        }

        GameObject inst = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (inst == null)
        {
            Debug.LogError("Could not instantiate prefab");
            return;
        }

        // 1. Hide old truck visual mesh
        Transform truckMesh = inst.transform.Find("carzyCar/truck/Cargodoor_left");
        if (truckMesh != null)
        {
            MeshRenderer mr = truckMesh.GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = false;
        }

        // 2. Attach new car body under carzyCar/truck
        GameObject bodyAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/car_body.obj");
        if (bodyAsset != null)
        {
            Transform existingBody = inst.transform.Find("carzyCar/truck/NewCarBody");
            if (existingBody != null) Object.DestroyImmediate(existingBody.gameObject);

            GameObject newBody = Object.Instantiate(bodyAsset, inst.transform.Find("carzyCar/truck"));
            newBody.name = "NewCarBody";
            newBody.transform.localPosition = Vector3.zero;
            newBody.transform.localRotation = Quaternion.identity;
            newBody.transform.localScale = Vector3.one;
        }

        // 3. Attach new wheels under FL, FR, RL, RR
        GameObject wheelFLAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/wheel_FL.obj");
        GameObject wheelRLAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/wheel_RL.obj");

        System.Action<string, GameObject, Vector3> setupWheelVisual = (parentPath, modelAsset, localPos) =>
        {
            Transform parent = inst.transform.Find(parentPath);
            if (parent == null) return;
            parent.localPosition = localPos;

            foreach (Transform child in parent)
            {
                MeshRenderer mr = child.GetComponent<MeshRenderer>();
                if (mr != null) mr.enabled = false;
            }

            if (modelAsset != null)
            {
                Transform existing = parent.Find("NewWheelModel");
                if (existing != null) Object.DestroyImmediate(existing.gameObject);

                GameObject w = Object.Instantiate(modelAsset, parent);
                w.name = "NewWheelModel";
                w.transform.localPosition = Vector3.zero;
                w.transform.localRotation = Quaternion.identity;
                w.transform.localScale = Vector3.one;
            }
        };

        Vector3 posFL = new Vector3(-0.76f, 0.33f,  1.07f);
        Vector3 posFR = new Vector3( 0.76f, 0.33f,  1.07f);
        Vector3 posRL = new Vector3(-0.74f, 0.34f, -1.27f);
        Vector3 posRR = new Vector3( 0.74f, 0.34f, -1.27f);

        setupWheelVisual("carzyCar/Wheel Model/FL", wheelFLAsset, posFL);
        setupWheelVisual("carzyCar/Wheel Model/FR", wheelFLAsset, posFR);
        setupWheelVisual("carzyCar/Wheel Model/RL", wheelRLAsset, posRL);
        setupWheelVisual("carzyCar/Wheel Model/RR", wheelRLAsset, posRR);

        // 4. Update WheelColliders (radius = 0.33m)
        System.Action<string, Vector3, float> setupWheelCollider = (name, pos, radius) =>
        {
            Transform colT = inst.transform.Find("Wheel collider/" + name);
            if (colT == null) return;
            colT.localPosition = pos;
            WheelCollider wc = colT.GetComponent<WheelCollider>();
            if (wc != null)
            {
                wc.radius = radius;
                wc.suspensionDistance = 0.12f;
                wc.mass = 35f;
                JointSpring spr = wc.suspensionSpring;
                spr.spring = 30000f;
                spr.damper = 3500f;
                spr.targetPosition = 0.5f;
                wc.suspensionSpring = spr;
            }
        };

        setupWheelCollider("Col FL", posFL, 0.33f);
        setupWheelCollider("Col FR", posFR, 0.33f);
        setupWheelCollider("Col RL", posRL, 0.34f);
        setupWheelCollider("Col RR", posRR, 0.34f);

        // 5. Attach new steering wheel under Steer
        GameObject steerAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/steering_wheel.obj");
        Transform steerParent = inst.transform.Find("Interior/steering_dummy/Steer");
        if (steerParent != null)
        {
            foreach (Transform child in steerParent)
            {
                if (child.name.Contains("Steering_wheel"))
                {
                    MeshRenderer mr = child.GetComponent<MeshRenderer>();
                    if (mr != null) mr.enabled = false;
                }
            }
            if (steerAsset != null)
            {
                Transform existing = steerParent.Find("NewSteerModel");
                if (existing != null) Object.DestroyImmediate(existing.gameObject);

                GameObject sw = Object.Instantiate(steerAsset, steerParent);
                sw.name = "NewSteerModel";
                sw.transform.localPosition = Vector3.zero;
                sw.transform.localRotation = Quaternion.identity;
                sw.transform.localScale = Vector3.one;
            }
        }

        Transform steeringDummy = inst.transform.Find("Interior/steering_dummy");
        if (steeringDummy != null) steeringDummy.localPosition = new Vector3(-0.29f, 0.65f, 0.15f);

        // 6. Anchors: SitPos, DoorPos, Feet, Cam
        Transform sitPos = inst.transform.Find("SitPosL");
        if (sitPos != null) sitPos.localPosition = new Vector3(-0.30f, 0.45f, -0.15f);

        Transform doorPos = inst.transform.Find("DoorPos");
        if (doorPos != null)
        {
            doorPos.localPosition = new Vector3(-1.10f, 0.25f, -0.10f);
            doorPos.gameObject.layer = 9;
        }

        Transform leftFoot = inst.transform.Find("LeftFoot");
        if (leftFoot != null) leftFoot.localPosition = new Vector3(-0.35f, 0.20f, 0.40f);

        Transform rightFoot = inst.transform.Find("RightFoot");
        if (rightFoot != null) rightFoot.localPosition = new Vector3(-0.25f, 0.20f, 0.40f);

        Transform com = inst.transform.Find("Center of Mass");
        if (com != null) com.localPosition = new Vector3(0.0f, 0.20f, -0.10f);

        Transform cam = inst.transform.Find("Cam");
        if (cam != null) cam.localPosition = new Vector3(0.0f, 1.30f, -0.20f);

        // 7. Hitbox BoxCollider on root
        BoxCollider box = inst.GetComponent<BoxCollider>();
        if (box != null)
        {
            box.size = new Vector3(1.84f, 1.05f, 4.10f);
            box.center = new Vector3(0.0f, 0.65f, 0.0f);
        }

        // 8. Ensure Rigidbody mass is car-like (1500kg)
        Rigidbody rb = inst.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.mass = 1500f;
            rb.centerOfMass = new Vector3(0.0f, 0.20f, -0.10f);
        }

        // 9. Ensure RidingCar canIntract is true
        RidingCar rc = inst.GetComponent<RidingCar>();
        if (rc != null)
        {
            rc.canIntract = true;
        }

        // 10. Save back to original prefab preserving references
        PrefabUtility.SaveAsPrefabAsset(inst, prefabPath);
        Object.DestroyImmediate(inst);

        AssetImporter importer = AssetImporter.GetAtPath(prefabPath);
        if (importer != null)
        {
            importer.assetBundleName = "rgs";
        }

        Debug.Log("✅ NEW CAR INTEGRATED INTO rgs.prefab SUCCESSFULLY!");
    }
}
