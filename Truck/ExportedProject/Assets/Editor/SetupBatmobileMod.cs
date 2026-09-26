using UnityEditor;
using UnityEngine;
using System.IO;

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

        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            // 1. Swap Truck Body Mesh
            Transform truckVisual = root.transform.Find("truck");
            if (truckVisual != null)
            {
                foreach (Renderer r in truckVisual.GetComponentsInChildren<Renderer>(true))
                {
                    r.enabled = false;
                }
                GameObject bodyAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/batmobile_body.obj");
                if (bodyAsset != null)
                {
                    GameObject bodyInst = Object.Instantiate(bodyAsset, truckVisual);
                    bodyInst.name = "Batmobile_Body";
                    bodyInst.transform.localPosition = Vector3.zero;
                    bodyInst.transform.localRotation = Quaternion.identity;
                    bodyInst.transform.localScale = Vector3.one;
                }
            }

            // 2. Adjust Wheel Colliders & Meshes (radius = 0.71m)
            float r = 0.71f;
            SetWheel(root, "Wheel collider/Col FL", "Wheel Model/FL", new Vector3(-2.11f, 0.79f, 3.82f), r, "Assets/Models/wheel_FL.obj");
            SetWheel(root, "Wheel collider/Col FR", "Wheel Model/FR", new Vector3(2.11f, 0.79f, 3.82f), r, "Assets/Models/wheel_FR.obj");
            SetWheel(root, "Wheel collider/Col RL", "Wheel Model/RL", new Vector3(-2.52f, 0.79f, -3.16f), r, "Assets/Models/wheel_RL.obj");
            SetWheel(root, "Wheel collider/Col RR", "Wheel Model/RR", new Vector3(2.52f, 0.79f, -3.16f), r, "Assets/Models/wheel_RR.obj");

            // 3. Adjust Interaction Points & Cameras on Layer 9
            Transform doorPos = root.transform.Find("DoorPos");
            if (doorPos != null) doorPos.localPosition = new Vector3(-2.60f, 0.25f, -0.40f);

            Transform sitPos = root.transform.Find("SitPosL");
            if (sitPos != null) sitPos.localPosition = new Vector3(-0.45f, 1.00f, -0.40f);

            Transform interiorCam = root.transform.Find("InteriorCam");
            if (interiorCam != null) interiorCam.localPosition = new Vector3(-0.45f, 1.35f, -0.35f);

            Transform cam = root.transform.Find("Cam");
            if (cam != null) cam.localPosition = new Vector3(0.0f, 2.80f, -6.50f);

            // 4. Update Colliders on Root
            BoxCollider[] colliders = root.GetComponents<BoxCollider>();
            int nonTriggerCount = 0;
            foreach (var col in colliders)
            {
                if (col.isTrigger)
                {
                    col.center = new Vector3(-2.60f, 1.00f, -0.40f);
                    col.size = new Vector3(2.00f, 2.00f, 2.50f);
                }
                else
                {
                    if (nonTriggerCount == 0)
                    {
                        col.center = new Vector3(0.00f, 1.20f, 0.00f);
                        col.size = new Vector3(3.60f, 1.80f, 11.50f);
                        nonTriggerCount++;
                    }
                    else
                    {
                        col.size = Vector3.zero; // Disable extra upper truck collider
                    }
                }
            }

            // 5. Tag all assets into AssetBundle 'rgs'
            string[] assets = new string[] {
                prefabPath,
                "Assets/Models/batmobile_body.obj",
                "Assets/Models/batmobile.mtl",
                "Assets/Models/wheel_FL.obj",
                "Assets/Models/wheel_FR.obj",
                "Assets/Models/wheel_RL.obj",
                "Assets/Models/wheel_RR.obj"
            };
            foreach (string a in assets)
            {
                AssetImporter imp = AssetImporter.GetAtPath(a);
                if (imp != null) imp.assetBundleName = "rgs";
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Debug.Log("[SetupBatmobileMod] Successfully saved Batmobile rgs.prefab!");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void SetWheel(GameObject root, string colPath, string modelPath, Vector3 pos, float radius, string modelAsset)
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
            foreach (Renderer r in m.GetComponentsInChildren<Renderer>(true)) r.enabled = false;
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(modelAsset);
            if (asset != null)
            {
                GameObject inst = Object.Instantiate(asset, m);
                inst.name = "WheelMesh";
                inst.transform.localPosition = Vector3.zero;
                inst.transform.localRotation = Quaternion.identity;
                inst.transform.localScale = Vector3.one;
            }
        }
    }
}
