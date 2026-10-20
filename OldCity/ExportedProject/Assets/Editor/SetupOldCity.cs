using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SetupOldCity
{
    [MenuItem("Build/Setup OldCity")]
    public static void Build()
    {
        Debug.Log("[SetupOldCity] Starting city hierarchy setup...");

        // 1. Unpack old_map_single.zip if needed
        string modelZip = "Assets/Models/old_map_single.zip";
        string modelObj = "Assets/Models/old_map.obj";
        if (File.Exists(modelZip) && !File.Exists(modelObj))
        {
            Debug.Log("[SetupOldCity] Unzipping old_map_single.zip...");
            System.IO.Compression.ZipFile.ExtractToDirectory(modelZip, "Assets/Models");
        }

        // Move old_map_texture.png to Texture2D
        string rawTex = "Assets/Models/old_map_texture.png";
        string destTex = "Assets/Texture2D/old_map_texture.png";
        if (File.Exists(rawTex) && !File.Exists(destTex))
        {
            File.Copy(rawTex, destTex, true);
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        // 2. Configure ModelImporter for old_map.obj
        ModelImporter mi = AssetImporter.GetAtPath(modelObj) as ModelImporter;
        if (mi != null)
        {
            mi.materialImportMode = ModelImporterMaterialImportMode.None;
            mi.animationType = ModelImporterAnimationType.None;
            mi.importNormals = ModelImporterNormals.Import;
            mi.SaveAndReimport();
        }

        // 3. Configure TextureImporter for CityV1.png
        TextureImporter ti = AssetImporter.GetAtPath("Assets/Texture2D/CityV1.png") as TextureImporter;
        if (ti != null)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.SaveAndReimport();
        }

        // 4. Create Material for Old City
        string matPath = "Assets/Material/OldCityMaterial.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null)
        {
            Directory.CreateDirectory("Assets/Material");
            mat = new Material(Shader.Find("Standard"));
            mat.name = "OldCityMaterial";
            Texture2D mapTex = AssetDatabase.LoadAssetAtPath<Texture2D>(destTex);
            if (mapTex != null)
            {
                mat.mainTexture = mapTex;
            }
            AssetDatabase.CreateAsset(mat, matPath);
        }

        // 5. Load Sprite icon
        Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/CityV1.asset");
        if (iconSprite == null)
        {
            iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Texture2D/CityV1.png");
        }
        Debug.Log("[SetupOldCity] Loaded icon sprite: " + (iconSprite != null ? iconSprite.name : "null"));

        // 6. Load Model Prefab
        GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelObj);
        if (modelPrefab == null)
        {
            Debug.LogError("[SetupOldCity] Failed to load modelPrefab at " + modelObj);
            return;
        }

        // 7. Assemble cm7.prefab
        // Hierarchy matching original cm7file:
        // cm7 (pos: -269, 37.45, -1723.3, rot: Euler(0, 75.1, 0))
        //   -> "Old Map" (with currenthuman component)
        //        -> CityMesh (with all submesh parts: MeshFilter, MeshRenderer, MeshCollider convex=false)
        GameObject cm7GO = new GameObject("cm7");
        cm7GO.transform.position = new Vector3(-269f, 37.45f, -1723.3f);
        cm7GO.transform.rotation = new Quaternion(0f, -0.6094535f, 0f, 0.79282176f);
        cm7GO.transform.localScale = Vector3.one;

        GameObject oldmapGO = new GameObject("Old Map");
        oldmapGO.transform.SetParent(cm7GO.transform, false);
        oldmapGO.transform.localPosition = Vector3.zero;
        oldmapGO.transform.localRotation = Quaternion.identity;
        oldmapGO.transform.localScale = Vector3.one;

        currenthuman ch = oldmapGO.AddComponent<currenthuman>();
        ch.img = iconSprite;
        ch.humans = 0;

        // Instantiate model prefab under Old Map
        GameObject modelInstance = UnityEngine.Object.Instantiate(modelPrefab);
        modelInstance.name = "CityMesh";
        modelInstance.transform.SetParent(oldmapGO.transform, false);
        modelInstance.transform.localPosition = Vector3.zero;
        modelInstance.transform.localRotation = Quaternion.identity;
        modelInstance.transform.localScale = Vector3.one;

        // Attach MeshCollider and Material to EVERY child submesh part
        MeshFilter[] mfs = modelInstance.GetComponentsInChildren<MeshFilter>(true);
        Debug.Log("[SetupOldCity] Found " + mfs.Length + " mesh parts in model.");
        foreach (MeshFilter mf in mfs)
        {
            if (mf.sharedMesh != null)
            {
                MeshCollider mc = mf.gameObject.GetComponent<MeshCollider>();
                if (mc == null) mc = mf.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;
                mc.convex = false;
            }

            MeshRenderer mr = mf.gameObject.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sharedMaterial = mat;
            }
        }

        // Save cm7.prefab
        string cm7PrefabPath = "Assets/test/megacity/cm7.prefab";
        Directory.CreateDirectory("Assets/test/megacity");
        PrefabUtility.SaveAsPrefabAsset(cm7GO, cm7PrefabPath);
        Debug.Log("[SetupOldCity] Successfully saved cm7.prefab at " + cm7PrefabPath);

        // 8. Assemble rgs.prefab for compatibility
        GameObject rgsGO = new GameObject("rgs");
        rgsGO.transform.position = new Vector3(-269f, 37.45f, -1723.3f);
        rgsGO.transform.rotation = new Quaternion(0f, -0.6094535f, 0f, 0.79282176f);
        rgsGO.transform.localScale = Vector3.one;
        ModMe mm = rgsGO.AddComponent<ModMe>();
        mm.Scale = true;

        GameObject rgsOldmapGO = new GameObject("Old Map");
        rgsOldmapGO.transform.SetParent(rgsGO.transform, false);
        rgsOldmapGO.transform.localPosition = Vector3.zero;
        rgsOldmapGO.transform.localRotation = Quaternion.identity;
        rgsOldmapGO.transform.localScale = Vector3.one;

        currenthuman ch2 = rgsOldmapGO.AddComponent<currenthuman>();
        ch2.img = iconSprite;
        ch2.humans = 0;

        GameObject rgsModelInstance = UnityEngine.Object.Instantiate(modelInstance);
        rgsModelInstance.name = "CityMesh";
        rgsModelInstance.transform.SetParent(rgsOldmapGO.transform, false);
        rgsModelInstance.transform.localPosition = Vector3.zero;
        rgsModelInstance.transform.localRotation = Quaternion.identity;
        rgsModelInstance.transform.localScale = Vector3.one;

        string rgsPrefabPath = "Assets/outsidemods/prefab/rgs.prefab";
        Directory.CreateDirectory("Assets/outsidemods/prefab");
        PrefabUtility.SaveAsPrefabAsset(rgsGO, rgsPrefabPath);
        Debug.Log("[SetupOldCity] Successfully saved rgs.prefab at " + rgsPrefabPath);

        UnityEngine.Object.DestroyImmediate(cm7GO);
        UnityEngine.Object.DestroyImmediate(rgsGO);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
