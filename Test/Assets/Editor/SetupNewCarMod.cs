using UnityEditor;
using UnityEngine;
using System.IO;

public class SetupNewCarMod
{
    [MenuItem("Tools/Build New Car Prefab")]
    public static void Build()
    {
        string prefabPath = "Assets/outsidemods/prefab/rgs.prefab";
        string prefabFolder = Path.GetDirectoryName(prefabPath);
        if (!Directory.Exists(prefabFolder)) Directory.CreateDirectory(prefabFolder);

        // 1. Root Car Object
        GameObject root = new GameObject("rgs");

        // Rigidbody
        Rigidbody rb = root.AddComponent<Rigidbody>();
        rb.mass = 1450f;
        rb.drag = 0.05f;
        rb.angularDrag = 0.05f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.centerOfMass = new Vector3(0f, 0.22f, -0.10f);

        // Solid Body BoxCollider (Hull)
        BoxCollider hullCol = root.AddComponent<BoxCollider>();
        hullCol.isTrigger = false;
        hullCol.size = new Vector3(1.84f, 1.10f, 4.10f);
        hullCol.center = new Vector3(0f, 0.65f, 0f);

        // Enter Vehicle Trigger BoxCollider (CRITICAL for RidingCar.OnTriggerEnter)
        BoxCollider enterTrigger = root.AddComponent<BoxCollider>();
        enterTrigger.isTrigger = true;
        enterTrigger.size = new Vector3(1.4f, 1.5f, 2.0f);
        enterTrigger.center = new Vector3(-1.15f, 0.70f, 0.0f);

        // 2. Anchors: Seat, Door Trigger, Cam, Feet, Center of Mass
        GameObject sitPos = new GameObject("SitPosL");
        sitPos.transform.SetParent(root.transform, false);
        sitPos.transform.localPosition = new Vector3(-0.30f, 0.48f, -0.15f);

        GameObject doorPos = new GameObject("DoorPos");
        doorPos.transform.SetParent(root.transform, false);
        doorPos.transform.localPosition = new Vector3(-1.15f, 0.25f, -0.10f);
        doorPos.layer = 9;

        GameObject camTarget = new GameObject("Cam");
        camTarget.transform.SetParent(root.transform, false);
        camTarget.transform.localPosition = new Vector3(0f, 1.35f, -0.20f);

        GameObject com = new GameObject("Center of Mass");
        com.transform.SetParent(root.transform, false);
        com.transform.localPosition = new Vector3(0f, 0.22f, -0.10f);

        GameObject leftFoot = new GameObject("LeftFoot");
        leftFoot.transform.SetParent(root.transform, false);
        leftFoot.transform.localPosition = new Vector3(-0.35f, 0.20f, 0.45f);

        GameObject rightFoot = new GameObject("RightFoot");
        rightFoot.transform.SetParent(root.transform, false);
        rightFoot.transform.localPosition = new Vector3(-0.25f, 0.20f, 0.45f);

        // 3. Visual Models Parent (carzyCar / truck)
        GameObject carzyCar = new GameObject("carzyCar");
        carzyCar.transform.SetParent(root.transform, false);
        carzyCar.layer = 9;

        GameObject truckObj = new GameObject("truck");
        truckObj.transform.SetParent(carzyCar.transform, false);

        GameObject bodyAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/car_body.obj");
        if (bodyAsset != null)
        {
            GameObject bodyInst = Object.Instantiate(bodyAsset, truckObj.transform);
            bodyInst.name = "CarBody";
            bodyInst.transform.localPosition = Vector3.zero;
            bodyInst.transform.localRotation = Quaternion.identity;
            bodyInst.transform.localScale = Vector3.one;
        }

        // 4. Wheel Colliders Parent
        GameObject wheelColParent = new GameObject("Wheel collider");
        wheelColParent.transform.SetParent(root.transform, false);

        WheelCollider[] colliders = new WheelCollider[4];
        string[] colNames = { "Col FL", "Col FR", "Col RL", "Col RR" };
        Vector3[] colPositions = {
            new Vector3(-0.757f, 0.33f,  1.069f),
            new Vector3( 0.757f, 0.33f,  1.069f),
            new Vector3(-0.741f, 0.34f, -1.271f),
            new Vector3( 0.741f, 0.34f, -1.271f)
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject colGo = new GameObject(colNames[i]);
            colGo.transform.SetParent(wheelColParent.transform, false);
            colGo.transform.localPosition = colPositions[i];

            WheelCollider wc = colGo.AddComponent<WheelCollider>();
            wc.radius = (i < 2) ? 0.32f : 0.34f;
            wc.mass = 35f;
            wc.suspensionDistance = 0.12f;
            JointSpring spr = wc.suspensionSpring;
            spr.spring = 32000f;
            spr.damper = 3500f;
            spr.targetPosition = 0.5f;
            wc.suspensionSpring = spr;
            colliders[i] = wc;
        }

        // 5. Visual Wheels Parent
        GameObject wheelModelParent = new GameObject("Wheel Model");
        wheelModelParent.transform.SetParent(carzyCar.transform, false);

        Transform[] tireMeshes = new Transform[4];
        string[] wheelModelNames = { "FL", "FR", "RL", "RR" };
        string[] wheelAssetPaths = {
            "Assets/Models/wheel_FL.obj",
            "Assets/Models/wheel_FR.obj",
            "Assets/Models/wheel_RL.obj",
            "Assets/Models/wheel_RR.obj"
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject wPivot = new GameObject(wheelModelNames[i]);
            wPivot.transform.SetParent(wheelModelParent.transform, false);
            wPivot.transform.localPosition = colPositions[i];
            tireMeshes[i] = wPivot.transform;

            GameObject wAsset = AssetDatabase.LoadAssetAtPath<GameObject>(wheelAssetPaths[i]);
            if (wAsset != null)
            {
                GameObject wModel = Object.Instantiate(wAsset, wPivot.transform);
                wModel.name = "Model";
                wModel.transform.localPosition = Vector3.zero;
                wModel.transform.localRotation = Quaternion.identity;
                wModel.transform.localScale = Vector3.one;
            }
        }

        // 6. Interior & Steering Wheel
        GameObject interior = new GameObject("Interior");
        interior.transform.SetParent(root.transform, false);

        GameObject steerDummy = new GameObject("steering_dummy");
        steerDummy.transform.SetParent(interior.transform, false);
        steerDummy.transform.localPosition = new Vector3(-0.288f, 0.576f, 0.083f);

        GameObject steerNode = new GameObject("Steer");
        steerNode.transform.SetParent(steerDummy.transform, false);
        steerNode.transform.localRotation = Quaternion.Euler(-20f, 0f, 0f);

        GameObject steerAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/steering_wheel.obj");
        if (steerAsset != null)
        {
            GameObject stModel = Object.Instantiate(steerAsset, steerNode.transform);
            stModel.name = "Steering_wheel";
            stModel.transform.localPosition = Vector3.zero;
            stModel.transform.localRotation = Quaternion.identity;
            stModel.transform.localScale = Vector3.one;
        }

        GameObject leftHand = new GameObject("LeftHand");
        leftHand.transform.SetParent(steerNode.transform, false);
        leftHand.transform.localPosition = new Vector3(-0.15f, 0f, 0f);

        GameObject rightHand = new GameObject("RightHand");
        rightHand.transform.SetParent(steerNode.transform, false);
        rightHand.transform.localPosition = new Vector3(0.15f, 0f, 0f);

        // 7. Attach Vehicle MonoBehaviours
        CarControl cc = root.AddComponent<CarControl>();
        cc.wheelColliders = colliders;
        cc.tireMeshes = tireMeshes;
        cc.m_rigidBody = rb;
        cc.centerOfMass = com.transform;
        cc.handel = steerNode.transform;
        cc.maxTorque = 12000f;
        cc.topSpeed = 220f;
        cc.reverseSpeed = 45f;
        cc.bodyInterpolation = 1;

        GameObject engineSoundGo = new GameObject("EngineSound");
        engineSoundGo.transform.SetParent(root.transform, false);
        AudioSource aLoop = engineSoundGo.AddComponent<AudioSource>();
        aLoop.loop = true;
        aLoop.playOnAwake = false;
        cc.engineLoop = aLoop;

        GameObject engineLoadGo = new GameObject("EngineSound 2");
        engineLoadGo.transform.SetParent(root.transform, false);
        AudioSource aLoad = engineLoadGo.AddComponent<AudioSource>();
        aLoad.loop = true;
        aLoad.playOnAwake = false;
        cc.engineLoad = aLoad;

        GameObject crashSoundGo = new GameObject("CrashSound");
        crashSoundGo.transform.SetParent(root.transform, false);
        AudioSource aCrash = crashSoundGo.AddComponent<AudioSource>();
        aCrash.playOnAwake = false;

        RidingCar rc = root.AddComponent<RidingCar>();
        rc.canIntract = true;
        rc.sitPos = sitPos.transform;
        rc.doorPos = doorPos.transform;
        rc.camTarget = camTarget.transform;
        rc.leftHand = leftHand.transform;
        rc.rightHand = rightHand.transform;
        rc.leftFoot = leftFoot.transform;
        rc.rightFoot = rightFoot.transform;
        rc._car = cc;

        CarImpactCheck cic = root.AddComponent<CarImpactCheck>();
        cic.crashSound = aCrash;
        cic.damage = 1;
        cic._colDist = 8f;

        ExplosionVehicle ev = root.AddComponent<ExplosionVehicle>();
        ev.health = 100f;
        ev.explosionForce = 5000f;
        ev.explosionRadius = 5f;

        // 8. Save Prefab and assign AssetBundle name
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);

        AssetImporter importer = AssetImporter.GetAtPath(prefabPath);
        if (importer != null)
        {
            importer.assetBundleName = "rgs";
        }

        Debug.Log("🎉 New car prefab 'rgs.prefab' successfully generated with 1:1 scale and complete wiring!");
    }
}
