using UnityEditor;
using UnityEngine;
using System.IO;

public class SetupBuildingProp
{
    [MenuItem("Tools/Build Firecracker Prop")]
    public static void Build()
    {
        string fbxPath = "Assets/Models/firecracker_skyshots_10x.fbx";
        string wavPath = "Assets/Audio/firecracker_skyshots_10x.wav";
        string prefabPath = "Assets/outsidemods/prefab/rgs.prefab";
        string prefabDir = Path.GetDirectoryName(prefabPath);
        if (!Directory.Exists(prefabDir)) Directory.CreateDirectory(prefabDir);

        // 1. Configure ModelImporter for loop animation
        ModelImporter importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
        if (importer != null)
        {
            importer.animationType = ModelImporterAnimationType.Generic;
            ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
            if (clips == null || clips.Length == 0)
            {
                clips = new ModelImporterClipAnimation[] { new ModelImporterClipAnimation() { name = "Firecracker_10_Skyshots", firstFrame = 0, lastFrame = 1260 } };
            }
            foreach (var clip in clips)
            {
                clip.loopTime = true;
                clip.wrapMode = WrapMode.Loop;
            }
            importer.clipAnimations = clips;
            importer.SaveAndReimport();
        }

        // 2. Create Root 'rgs' GameObject on Layer 0 (Default)
        GameObject root = new GameObject("rgs");
        root.layer = 0;

        // Attach ModMe with Scale enabled
        ModMe mm = root.AddComponent<ModMe>();
        mm.Scale = true;

        // Base ground BoxCollider
        BoxCollider bc = root.AddComponent<BoxCollider>();
        bc.center = new Vector3(0f, 0.5f, 0f);
        bc.size = new Vector3(2.5f, 1.0f, 2.5f);

        // 3. Load and Instantiate FBX Model
        GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (modelAsset != null)
        {
            GameObject modelInst = Object.Instantiate(modelAsset, root.transform);
            modelInst.name = "Firecracker_Battery";
            modelInst.transform.localPosition = Vector3.zero;
            modelInst.transform.localRotation = Quaternion.identity;
            modelInst.transform.localScale = Vector3.one;

            // Set Layer 0 across all children
            Transform[] allChildren = modelInst.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in allChildren)
            {
                t.gameObject.layer = 0;
            }

            // 4. Attach Dynamic Real-Time Point Lights to Flash/Blast Nodes
            Color[] blastColors = new Color[] {
                new Color(1f, 0.84f, 0.0f),   // Gold Imperial
                new Color(1f, 0.6f, 0.1f),    // Amber Kamuro
                new Color(0.9f, 0.95f, 1f),   // White Flash
                new Color(0.0f, 0.9f, 1.0f),  // Cyan Electric
                new Color(1.0f, 0.1f, 0.6f),  // Magenta Hot
                new Color(0.2f, 1.0f, 0.3f),  // Green
                new Color(1.0f, 0.2f, 0.2f),  // Red
                new Color(0.6f, 0.2f, 1.0f),  // Violet
                new Color(1.0f, 0.9f, 0.3f),  // Yellow Strobe
                new Color(1.0f, 0.4f, 0.8f)   // Multi Neon
            };

            for (int i = 1; i <= 10; i++)
            {
                string targetName = string.Format("S{0:D2}_CoreFlash", i);
                Transform flashTr = FindChildRecursive(modelInst.transform, targetName);
                if (flashTr != null)
                {
                    GameObject lightGo = new GameObject("BlastLight_" + i);
                    lightGo.transform.SetParent(flashTr, false);
                    Light l = lightGo.AddComponent<Light>();
                    l.type = LightType.Point;
                    l.range = 100f; // Illuminates surrounding world, buildings and player
                    l.intensity = 6f;
                    l.color = blastColors[(i - 1) % blastColors.Length];
                }
            }

            // Ensure Animator is present and enabled
            Animator animator = modelInst.GetComponent<Animator>();
            if (animator == null) animator = modelInst.AddComponent<Animator>();
            animator.applyRootMotion = false;
        }
        else
        {
            Debug.LogError("Failed to load FBX model: " + fbxPath);
        }

        // 5. Attach AudioSource with Synced 42-second WAV Audio
        AudioClip audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(wavPath);
        if (audioClip != null)
        {
            AudioSource audio = root.AddComponent<AudioSource>();
            audio.clip = audioClip;
            audio.loop = true;          // Infinite loop
            audio.playOnAwake = true;   // Plays immediately when spawned
            audio.spatialBlend = 1.0f;  // 3D Spatial Audio
            audio.minDistance = 10f;
            audio.maxDistance = 500f;
            audio.volume = 1.0f;
        }
        else
        {
            Debug.LogWarning("Audio clip not found at " + wavPath);
        }

        // 6. Save Prefab and Tag into 'rgs' AssetBundle
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);

        AssetImporter pImp = AssetImporter.GetAtPath(prefabPath);
        if (pImp != null) pImp.assetBundleName = "rgs";

        AssetImporter fbxImp = AssetImporter.GetAtPath(fbxPath);
        if (fbxImp != null) fbxImp.assetBundleName = "rgs";

        AssetImporter wavImp = AssetImporter.GetAtPath(wavPath);
        if (wavImp != null) wavImp.assetBundleName = "rgs";

        Debug.Log("🎉 Successfully generated Firecracker Skyshots 10x Prefab with Synced Audio and World Blast Glow!");
    }

    private static Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform result = FindChildRecursive(parent.GetChild(i), name);
            if (result != null) return result;
        }
        return null;
    }
}
