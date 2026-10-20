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
        // Hierarchy matching cm7file:
        // cm7 (pos 0,0,0)
        //   -> oldmap (with currenthuman component)
        //        -> submesh parts (each with MeshFilter, MeshRenderer, MeshCollider convex=false)
        GameObject cm7GO = new GameObject("cm7");
        cm7GO.transform.position = Vector3.zero;
        cm7GO.transform.rotation = Quaternion.identity;
        cm7GO.transform.localScale = Vector3.one;

        GameObject oldmapGO = new GameObject("oldmap");
        oldmapGO.transform.SetParent(cm7GO.transform, false);
        oldmapGO.transform.localPosition = Vector3.zero;
        oldmapGO.transform.localRotation = Quaternion.identity;
        oldmapGO.transform.localScale = Vector3.one;

        currenthuman ch = oldmapGO.AddComponent<currenthuman>();
        ch.img = iconSprite;
        ch.humans = 0;

        // Instantiate model prefab and reparent all subparts under oldmap
        GameObject modelInstance = UnityEngine.Object.Instantiate(modelPrefab);
        List<Transform> children = new List<Transform>();
        foreach (Transform t in modelInstance.transform)
        {
            children.Add(t);
        }
        // If model has no children, modelInstance itself has the mesh
        if (children.Count == 0)
        {
            children.Add(modelInstance.transform);
        }

        foreach (Transform part in children)
        {
            part.SetParent(oldmapGO.transform, false);

            MeshFilter mf = part.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                MeshCollider mc = part.gameObject.GetComponent<MeshCollider>();
                if (mc == null) mc = part.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;
                mc.convex = false;
            }

            MeshRenderer mr = part.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sharedMaterial = mat;
            }
        }
        UnityEngine.Object.DestroyImmediate(modelInstance);

        // Save cm7.prefab
        string cm7PrefabPath = "Assets/test/megacity/cm7.prefab";
        Directory.CreateDirectory("Assets/test/megacity");
        PrefabUtility.SaveAsPrefabAsset(cm7GO, cm7PrefabPath);
        Debug.Log("[SetupOldCity] Successfully saved cm7.prefab at " + cm7PrefabPath);

        // 8. Assemble rgs.prefab for compatibility
        GameObject rgsGO = new GameObject("rgs");
        rgsGO.transform.position = Vector3.zero;
        ModMe mm = rgsGO.AddComponent<ModMe>();
        mm.Scale = true;

        GameObject rgsOldmapGO = new GameObject("oldmap");
        rgsOldmapGO.transform.SetParent(rgsGO.transform, false);
        currenthuman ch2 = rgsOldmapGO.AddComponent<currenthuman>();
        ch2.img = iconSprite;
        ch2.humans = 0;

        // Clone oldmap parts to rgsOldmapGO
        foreach (Transform part in oldmapGO.transform)
        {
            GameObject clonePart = UnityEngine.Object.Instantiate(part.gameObject);
            clonePart.name = part.name;
            clonePart.transform.SetParent(rgsOldmapGO.transform, false);
        }

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
