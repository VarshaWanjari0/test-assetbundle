using UnityEditor;
using UnityEngine;
using System.IO;

public class SetupBuildingProp
{
    const float S = 3.5f; // 3.5X SCALE (3-4X LARGE AS REQUESTED)

    [MenuItem("Tools/Build Building Prop")]
    public static void Build()
    {
        string prefabPath = "Assets/outsidemods/prefab/rgs.prefab";
        string prefabDir = Path.GetDirectoryName(prefabPath);
        if (!Directory.Exists(prefabDir)) Directory.CreateDirectory(prefabDir);

        // 1. Root GameObject on Layer 0 (Default Environment/Ground Layer)
        // Layer 0 ensures the player's ground-check raycast detects upper floors as solid ground
        // instead of falling infinitely through Layer 20!
        GameObject root = new GameObject("rgs");
        root.layer = 0;

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
            buildingInst.transform.localScale = Vector3.one * S; // 3.5x scale

            // Assign Layer 0 and attach 1:1 non-convex MeshCollider to each mesh part
            Transform[] allChildren = buildingInst.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in allChildren)
            {
                t.gameObject.layer = 0; // Layer 0 for solid ground detection
                MeshFilter mf = t.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    MeshCollider mc = t.gameObject.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.sharedMesh;
                    mc.convex = false; // 1:1 EXACT NON-CONVEX COLLIDER
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

        Debug.Log("🎉 Successfully created 3.5x enterable building prop on Layer 0 with 1:1 MeshColliders!");
    }
}
