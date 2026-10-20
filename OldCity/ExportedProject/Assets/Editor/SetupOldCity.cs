using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SetupOldCity
{
    [MenuItem("Build/Setup OldCity")]
    public static void Build()
    {
        Debug.Log("[SetupOldCity] Setting up Old City with cm7 and oldmap hierarchy...");

        // 1. Ensure Model is imported
        string modelPath = "Assets/Models/old_map.obj";
        AssetDatabase.ImportAsset(modelPath, ImportAssetOptions.ForceUpdate);
        GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        
        Mesh modelMesh = null;
        if (modelPrefab != null)
        {
            MeshFilter mf = modelPrefab.GetComponentInChildren<MeshFilter>();
            if (mf != null) modelMesh = mf.sharedMesh;
        }
        if (modelMesh == null)
        {
            foreach (UnityEngine.Object obj in AssetDatabase.LoadAllAssetsAtPath(modelPath))
            {
                if (obj is Mesh)
                {
                    modelMesh = (Mesh)obj;
                    break;
                }
            }
        }
        Debug.Log("[SetupOldCity] Model mesh loaded: " + (modelMesh != null ? modelMesh.name : "null"));

        // 2. Load image.png as Sprite
        string spritePath = "Assets/Texture2D/image.png";
        AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceUpdate);
        Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (iconSprite == null)
        {
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(spritePath);
            if (tex != null)
            {
                iconSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
        }
        Debug.Log("[SetupOldCity] Icon sprite loaded: " + (iconSprite != null ? iconSprite.name : "null"));

        // 3. Material setup
        string matPath = "Assets/Material/OldCityMaterial.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null)
        {
            Directory.CreateDirectory("Assets/Material");
            mat = new Material(Shader.Find("Standard"));
            Texture2D mapTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Texture2D/old_map_texture.png");
            if (mapTex != null) mat.mainTexture = mapTex;
            AssetDatabase.CreateAsset(mat, matPath);
        }

        // 4. Build cm7.prefab
        GameObject cm7GO = new GameObject("cm7");
        cm7GO.layer = 0;
        CityCreator cc = cm7GO.AddComponent<CityCreator>();
        cc.currentprops = 0;

        GameObject oldmapGO = new GameObject("oldmap");
        oldmapGO.layer = 0;
        oldmapGO.transform.SetParent(cm7GO.transform, false);

        currenthuman ch = oldmapGO.AddComponent<currenthuman>();
        ch.img = iconSprite;
        ch.humans = 0;

        if (modelMesh != null)
        {
            MeshFilter filter = oldmapGO.AddComponent<MeshFilter>();
            filter.sharedMesh = modelMesh;

            MeshRenderer renderer = oldmapGO.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = mat;

            MeshCollider col = oldmapGO.AddComponent<MeshCollider>();
            col.sharedMesh = modelMesh;
            col.convex = false;
        }

        string cm7PrefabPath = "Assets/test/megacity/cm7.prefab";
        Directory.CreateDirectory("Assets/test/megacity");
        PrefabUtility.SaveAsPrefabAsset(cm7GO, cm7PrefabPath);
        GameObject.DestroyImmediate(cm7GO);
        Debug.Log("[SetupOldCity] Successfully saved cm7.prefab at " + cm7PrefabPath);

        // 5. Build rgs.prefab (outside mod loader compatibility)
        GameObject rgsGO = new GameObject("rgs");
        rgsGO.layer = 0;
        ModMe mm = rgsGO.AddComponent<ModMe>();
        mm.Scale = true;

        GameObject rgsOldmapGO = new GameObject("oldmap");
        rgsOldmapGO.layer = 0;
        rgsOldmapGO.transform.SetParent(rgsGO.transform, false);

        currenthuman ch2 = rgsOldmapGO.AddComponent<currenthuman>();
        ch2.img = iconSprite;
        ch2.humans = 0;

        if (modelMesh != null)
        {
            MeshFilter filter = rgsOldmapGO.AddComponent<MeshFilter>();
            filter.sharedMesh = modelMesh;

            MeshRenderer renderer = rgsOldmapGO.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = mat;

            MeshCollider col = rgsOldmapGO.AddComponent<MeshCollider>();
            col.sharedMesh = modelMesh;
            col.convex = false;
        }

        string rgsPrefabPath = "Assets/outsidemods/prefab/rgs.prefab";
        Directory.CreateDirectory("Assets/outsidemods/prefab");
        PrefabUtility.SaveAsPrefabAsset(rgsGO, rgsPrefabPath);
        GameObject.DestroyImmediate(rgsGO);
        Debug.Log("[SetupOldCity] Successfully saved rgs.prefab at " + rgsPrefabPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
