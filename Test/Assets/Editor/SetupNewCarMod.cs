using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SetupNewCarMod
{
    const float S = 2.5f; // 2.5X SCALE AS REQUESTED!

    [MenuItem("Tools/Build New Car Prefab")]
    public static void Build()
    {
        string prefabPath = "Assets/outsidemods/prefab/rgs.prefab";
        string prefabFolder = Path.GetDirectoryName(prefabPath);
        if (!Directory.Exists(prefabFolder)) Directory.CreateDirectory(prefabFolder);

        // 1. Prepare Materials & Textures
        Material matTire = GetOrCreateMat("RB1c_Tire_1k", "Assets/Models/RB1c_Tire_1k.png", Color.white, false);
        Material matGlass = GetOrCreateMat("UCB_Lights_and_Glass_Transperent", "Assets/Models/UCB_Lights_and_Glass_Transperent.png", new Color(1f, 1f, 1f, 0.5f), true);
        Material matLights = GetOrCreateMat("UCB_Lights_and_Glass", "Assets/Models/UCB_Lights_and_Glass.png", Color.white, false);
        Material matBody = GetOrCreateMat("Maureen67_Bodymat", "", new Color(0.05f, 0.09f, 0.08f, 1f), false);
        Material matBottom = GetOrCreateMat("UCB_BOTTOM", "Assets/Models/UCB_BOTTOM.png", Color.white, false);
        Material matInterior = GetOrCreateMat("UCB_Interiors_1", "Assets/Models/UCB_Interiors_1.png", Color.white, false);
        Material matBadges = GetOrCreateMat("Carbadges_misc_U", "Assets/Models/Carbadges_misc_U.png", Color.white, false);
        Material matPlates = GetOrCreateMat("Numberplates_Misk_U", "Assets/Models/Numberplates_Misk_U.png", Color.white, false);

        Dictionary<string, Material> matMap = new Dictionary<string, Material>(System.StringComparer.OrdinalIgnoreCase)
        {
            { "RB1c_Tire_1k", matTire },
            { "UCB_Lights_and_Glass_Transperent", matGlass },
            { "UCB_Lights_and_Glass", matLights },
            { "Maureen67_Bodymat", matBody },
            { "UCB_BOTTOM", matBottom },
            { "UCB_Interiors_1", matInterior },
            { "Carbadges_misc_U", matBadges },
            { "Numberplates_Misk_U", matPlates }
        };

        // 2. Root Car Object (LAYER 9 FOR INDIAN BIKES INTERACTION)
        GameObject root = new GameObject("rgs");
        root.layer = 9;

        // Rigidbody (calibrated for 2.5x vehicle)
        Rigidbody rb = root.AddComponent<Rigidbody>();
        rb.mass = 3000f;
        rb.drag = 0.05f;
        rb.angularDrag = 0.05f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.centerOfMass = new Vector3(0f, 0.20f * S, 0f);

        // Solid Body BoxCollider (Hull) - Sized so player can walk up to door without clipping
        BoxCollider hullCol = root.AddComponent<BoxCollider>();
        hullCol.isTrigger = false;
        hullCol.size = new Vector3(1.65f * S, 1.05f * S, 3.90f * S);
        hullCol.center = new Vector3(0f, 0.65f * S, 0f);

        // Enter Vehicle Trigger BoxCollider (Accessible on ground from both sides)
        BoxCollider enterTrigger = root.AddComponent<BoxCollider>();
        enterTrigger.isTrigger = true;
        enterTrigger.size = new Vector3(3.2f * S, 1.8f * S, 3.5f * S);
        enterTrigger.center = new Vector3(0f, 0.9f * S, 0.3f * S);

        // 3. Anchors (ALL ON LAYER 9)
        // DoorPos is outside the solid hull so player walking to it will reach it without collision obstruction!
        GameObject doorPos = new GameObject("DoorPos");
        doorPos.transform.SetParent(root.transform, false);
        doorPos.transform.localPosition = new Vector3(2.85f, 0.25f, 0.75f);
        doorPos.transform.localRotation = Quaternion.Euler(0f, -20f, 0f);
        doorPos.layer = 9;

        GameObject sitPos = new GameObject("SitPosL");
        sitPos.transform.SetParent(root.transform, false);
        sitPos.transform.localPosition = new Vector3(0.65f, 0.85f, 0.25f);
        sitPos.layer = 9;

        GameObject camTarget = new GameObject("Cam");
        camTarget.transform.SetParent(root.transform, false);
        camTarget.transform.localPosition = new Vector3(0f, 1.40f * S, -0.20f * S);

        GameObject com = new GameObject("Center of Mass");
        com.transform.SetParent(root.transform, false);
        com.transform.localPosition = new Vector3(0f, 0.20f * S, 0f);

        GameObject leftFoot = new GameObject("LeftFoot");
        leftFoot.transform.SetParent(root.transform, false);
        leftFoot.transform.localPosition = new Vector3(0.50f, 0.35f, 0.70f);

        GameObject rightFoot = new GameObject("RightFoot");
        rightFoot.transform.SetParent(root.transform, false);
        rightFoot.transform.localPosition = new Vector3(0.70f, 0.35f, 0.70f);

        // 4. Doors with HingeJoint and CarJointControl (Required by RideCar coroutine!)
        GameObject doorsParent = new GameObject("Doors");
        doorsParent.transform.SetParent(root.transform, false);
        doorsParent.layer = 9;

        GameObject doorFL = new GameObject("DoorFL");
        doorFL.transform.SetParent(doorsParent.transform, false);
        doorFL.transform.localPosition = new Vector3(-0.95f * S, 0.5f * S, 0.3f * S);
        doorFL.layer = 9;
        Rigidbody rbFL = doorFL.AddComponent<Rigidbody>();
        rbFL.mass = 50f;
        HingeJoint hjFL = doorFL.AddComponent<HingeJoint>();
        hjFL.connectedBody = rb;
        hjFL.axis = Vector3.up;
        hjFL.useLimits = true;
        JointLimits limFL = hjFL.limits; limFL.min = 0f; limFL.max = 70f; hjFL.limits = limFL;
        CarJointControl cjcFL = doorFL.AddComponent<CarJointControl>();
        cjcFL.hingJoint = hjFL;
        cjcFL._rigidbody = rbFL;
        cjcFL.isDoor = 1;
        cjcFL.DropThisDoor = 1;

        GameObject doorFR = new GameObject("DoorFR");
        doorFR.transform.SetParent(doorsParent.transform, false);
        doorFR.transform.localPosition = new Vector3(0.95f * S, 0.5f * S, 0.3f * S);
        doorFR.layer = 9;
        Rigidbody rbFR = doorFR.AddComponent<Rigidbody>();
        rbFR.mass = 50f;
        HingeJoint hjFR = doorFR.AddComponent<HingeJoint>();
        hjFR.connectedBody = rb;
        hjFR.axis = Vector3.up;
        hjFR.useLimits = true;
        JointLimits limFR = hjFR.limits; limFR.min = 0f; limFR.max = 70f; hjFR.limits = limFR;
        CarJointControl cjcFR = doorFR.AddComponent<CarJointControl>();
        cjcFR.hingJoint = hjFR;
        cjcFR._rigidbody = rbFR;
        cjcFR.isDoor = 1;
        cjcFR.DropThisDoor = 1;

        // 5. Visual Models Parent (carzyCar / truck)
        GameObject carzyCar = new GameObject("carzyCar");
        carzyCar.transform.SetParent(root.transform, false);
        carzyCar.layer = 9;

        GameObject truckObj = new GameObject("truck");
        truckObj.transform.SetParent(carzyCar.transform, false);
        truckObj.layer = 9;

        GameObject bodyAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/car_body.obj");
        if (bodyAsset != null)
        {
            GameObject bodyInst = Object.Instantiate(bodyAsset, truckObj.transform);
            bodyInst.name = "CarBody";
            bodyInst.transform.localPosition = Vector3.zero;
            bodyInst.transform.localRotation = Quaternion.identity;
            bodyInst.transform.localScale = Vector3.one * S;
            ApplyMaterials(bodyInst, matMap);
        }

        // 6. Wheel Colliders Parent
        GameObject wheelColParent = new GameObject("Wheel collider");
        wheelColParent.transform.SetParent(root.transform, false);

        WheelCollider[] colliders = new WheelCollider[4];
        string[] colNames = { "Col FL", "Col FR", "Col RL", "Col RR" };
        Vector3[] colPositions = {
            new Vector3(-0.757f, 0.33f,  1.069f) * S,
            new Vector3( 0.757f, 0.33f,  1.069f) * S,
            new Vector3(-0.741f, 0.34f, -1.271f) * S,
            new Vector3( 0.741f, 0.34f, -1.271f) * S
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject colGo = new GameObject(colNames[i]);
            colGo.transform.SetParent(wheelColParent.transform, false);
            colGo.transform.localPosition = colPositions[i];

            WheelCollider wc = colGo.AddComponent<WheelCollider>();
            wc.radius = ((i < 2) ? 0.32f : 0.34f) * S;
            wc.mass = 45f;
            wc.suspensionDistance = 0.15f;
            JointSpring spr = wc.suspensionSpring;
            spr.spring = 38000f;
            spr.damper = 4000f;
            spr.targetPosition = 0.5f;
            wc.suspensionSpring = spr;
            colliders[i] = wc;
        }

        // 7. Visual Wheels Parent
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
                wModel.transform.localScale = Vector3.one * S;
                ApplyMaterials(wModel, matMap);
            }
        }

        // 8. Interior & Steering Wheel
        GameObject interior = new GameObject("Interior");
        interior.transform.SetParent(root.transform, false);

        GameObject steerDummy = new GameObject("steering_dummy");
        steerDummy.transform.SetParent(interior.transform, false);
        steerDummy.transform.localPosition = new Vector3(-0.288f, 0.576f, 0.083f) * S;

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
            stModel.transform.localScale = Vector3.one * S;
            ApplyMaterials(stModel, matMap);
        }

        GameObject leftHand = new GameObject("LeftHand");
        leftHand.transform.SetParent(steerNode.transform, false);
        leftHand.transform.localPosition = new Vector3(-0.15f, 0f, 0f) * S;

        GameObject rightHand = new GameObject("RightHand");
        rightHand.transform.SetParent(steerNode.transform, false);
        rightHand.transform.localPosition = new Vector3(0.15f, 0f, 0f) * S;

        // 9. Attach Vehicle MonoBehaviours
        CarControl cc = root.AddComponent<CarControl>();
        cc.wheelColliders = colliders;
        cc.tireMeshes = tireMeshes;
        cc.m_rigidBody = rb;
        cc.centerOfMass = com.transform;
        cc.handel = steerNode.transform;
        cc.maxTorque = 15000f;
        cc.topSpeed = 240f;
        cc.reverseSpeed = 50f;
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
        rc.CamDis = 15;
        rc.doorsHings = new HingeJoint[] { hjFR, hjFL };

        CarImpactCheck cic = root.AddComponent<CarImpactCheck>();
        cic.crashSound = aCrash;
        cic.damage = 1;
        cic._colDist = 10f;

        ExplosionVehicle ev = root.AddComponent<ExplosionVehicle>();
        ev.health = 100f;
        ev.explosionForce = 5000f;
        ev.explosionRadius = 5f;

        // 10. Tag all assets into rgs bundle
        string[] allAssets = new string[] {
            prefabPath,
            "Assets/Models/car_body.obj",
            "Assets/Models/wheel_FL.obj",
            "Assets/Models/wheel_FR.obj",
            "Assets/Models/wheel_RL.obj",
            "Assets/Models/wheel_RR.obj",
            "Assets/Models/steering_wheel.obj",
            "Assets/Models/RB1c_Tire_1k.png",
            "Assets/Models/UCB_Lights_and_Glass_Transperent.png",
            "Assets/Models/UCB_Lights_and_Glass.png",
            "Assets/Models/UCB_BOTTOM.png",
            "Assets/Models/UCB_Interiors_1.png",
            "Assets/Models/Carbadges_misc_U.png",
            "Assets/Models/Numberplates_Misk_U.png",
            "Assets/Models/RB1c_Tire_1k.mat",
            "Assets/Models/UCB_Lights_and_Glass_Transperent.mat",
            "Assets/Models/UCB_Lights_and_Glass.mat",
            "Assets/Models/Maureen67_Bodymat.mat",
            "Assets/Models/UCB_BOTTOM.mat",
            "Assets/Models/UCB_Interiors_1.mat",
            "Assets/Models/Carbadges_misc_U.mat",
            "Assets/Models/Numberplates_Misk_U.mat"
        };
        foreach (string ap in allAssets)
        {
            AssetImporter imp = AssetImporter.GetAtPath(ap);
            if (imp != null) imp.assetBundleName = "rgs";
        }

        // 11. Save Prefab
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);

        AssetImporter importer = AssetImporter.GetAtPath(prefabPath);
        if (importer != null)
        {
            importer.assetBundleName = "rgs";
        }

        Debug.Log("🎉 2.5x NewCar prefab 'rgs.prefab' with CarJointControl, unobstructed DoorPos, and HingeJoints successfully baked!");
    }

    static Material GetOrCreateMat(string matName, string texPath, Color col, bool transparent)
    {
        string path = "Assets/Models/" + matName + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader shader = Shader.Find(transparent ? "Transparent/Diffuse" : "Standard");
            if (shader == null) shader = Shader.Find("Mobile/Diffuse");
            if (shader == null) shader = Shader.Find("Diffuse");
            mat = new Material(shader);
            if (!string.IsNullOrEmpty(texPath))
            {
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                if (tex != null) mat.mainTexture = tex;
            }
            mat.color = col;
            if (transparent)
            {
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
            AssetDatabase.CreateAsset(mat, path);
        }
        return mat;
    }

    static void ApplyMaterials(GameObject rootObj, Dictionary<string, Material> matMap)
    {
        MeshRenderer[] renderers = rootObj.GetComponentsInChildren<MeshRenderer>(true);
        foreach (MeshRenderer mr in renderers)
        {
            Material[] mats = mr.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] != null)
                {
                    string cleanName = mats[i].name.Replace(" (Instance)", "").Trim();
                    if (matMap.ContainsKey(cleanName))
                    {
                        mats[i] = matMap[cleanName];
                    }
                }
            }
            string objName = mr.gameObject.name;
            foreach (var kvp in matMap)
            {
                if (objName.IndexOf(kvp.Key, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    for (int i = 0; i < mats.Length; i++) mats[i] = kvp.Value;
                    break;
                }
            }
            mr.sharedMaterials = mats;
        }
    }
}
