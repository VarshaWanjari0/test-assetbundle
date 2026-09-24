using UnityEditor;
using UnityEngine;
using System.IO;

public class SetupBuildingProp
{
    [MenuItem("Tools/Build Building Prop")]
    public static void Build()
    {
        string prefabPath = "Assets/outsidemods/prefab/rgs.prefab";
        string prefabDir = Path.GetDirectoryName(prefabPath);
        if (!Directory.Exists(prefabDir)) Directory.CreateDirectory(prefabDir);

        // 1. Root GameObject on Layer 20 (Props Layer in Indian Bikes Driving 3D)
        GameObject root = new GameObject("rgs");
        root.layer = 20;

        // Attach ModMe with Scale enabled
        ModMe mm = root.AddComponent<ModMe>();
        mm.Scale = true;

        // 2. Load and Instantiate Building Model
        GameObject buildingAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/building.obj");
        if (buildingAsset != null)
        {
            GameObject buildingInst = Object.Instantiate(buildingAsset, root.transform);
            buildingInst.name = "Building";
            buildingInst.transform.localPosition = Vector3.zero;
            buildingInst.transform.localRotation = Quaternion.identity;
            buildingInst.transform.localScale = Vector3.one;

            // Set Layer 20 and attach 1:1 non-convex MeshCollider to each mesh part
            Transform[] allChildren = buildingInst.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in allChildren)
            {
                t.gameObject.layer = 20;
                MeshFilter mf = t.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    MeshCollider mc = t.gameObject.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.sharedMesh;
                    mc.convex = false; // 1:1 EXACT NON-CONVEX COLLIDER (DOORWAYS & ROOMS OPEN)
                }
            }
        }
        else
        {
            Debug.LogError("Failed to load Assets/Models/building.obj!");
        }

        // 3. Tag all assets into 'rgs' AssetBundle
        string[] allAssets = new string[] {
            prefabPath,
            "Assets/Models/building.obj",
            "Assets/Models/material.mtl",
            "Assets/Models/Concrete1.png",
            "Assets/Models/Wall1.png",
            "Assets/Models/Wall2.png",
            "Assets/Models/Glass.png"
        };
        foreach (string ap in allAssets)
        {
            AssetImporter imp = AssetImporter.GetAtPath(ap);
            if (imp != null) imp.assetBundleName = "rgs";
        }

        // 4. Save Prefab
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);

        AssetImporter prefabImp = AssetImporter.GetAtPath(prefabPath);
        if (prefabImp != null) prefabImp.assetBundleName = "rgs";

        Debug.Log("🎉 Successfully created enterable building prop prefab with 1:1 non-convex MeshColliders!");
    }
}
