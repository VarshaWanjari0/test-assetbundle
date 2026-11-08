using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SetupBuildingProp
{
    const float S = 2.0f; // 2X SCALE AS REQUESTED

    [MenuItem("Tools/Build Fireworks Prop")]
    public static void Build()
    {
        string modelPath = "Assets/Models/Fireworks.glb";
        string wavPath = "Assets/Audio/Fireworks.wav";
        string prefabPath = "Assets/outsidemods/prefab/rgs.prefab";
        string controllerPath = "Assets/AnimatorController/Fireworks.controller";
        string matDir = "Assets/Material";
        string texDir = "Assets/Texture2D";

        if (!Directory.Exists(matDir)) Directory.CreateDirectory(matDir);
        if (!Directory.Exists(texDir)) Directory.CreateDirectory(texDir);
        if (!Directory.Exists("Assets/AnimatorController")) Directory.CreateDirectory("Assets/AnimatorController");
        if (!Directory.Exists(Path.GetDirectoryName(prefabPath))) Directory.CreateDirectory(Path.GetDirectoryName(prefabPath));

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        // 1. Load GLB Model Asset
        GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (modelAsset == null)
        {
            throw new System.Exception("FATAL: Failed to load GLB model asset at " + modelPath);
        }
        Debug.Log("Successfully loaded GLB modelAsset: " + modelAsset.name);

        // 2. Find AnimationClip
        AnimationClip animClip = null;
        Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(modelPath);
        foreach (Object obj in subAssets)
        {
            if (obj is AnimationClip && !obj.name.StartsWith("__preview__"))
            {
                animClip = obj as AnimationClip;
                animClip.wrapMode = WrapMode.Loop;
                Debug.Log("Found AnimationClip in GLB: " + animClip.name);
                break;
            }
        }

        // 3. Multi-Color Palette for Fireworks Show
        var colorPalette = new Dictionary<string, Color>()
        {
            { "Mat_FW_Gold", new Color(1.0f, 0.72f, 0.12f) },
            { "Mat_FW_Cyan", new Color(0.04f, 0.88f, 1.0f) },
            { "Mat_FW_Magenta", new Color(1.0f, 0.08f, 0.55f) },
            { "Mat_FW_Lime", new Color(0.55f, 1.0f, 0.10f) },
            { "Mat_FW_Blue", new Color(0.12f, 0.40f, 1.0f) },
            { "Mat_FW_Amber", new Color(1.0f, 0.45f, 0.06f) },
            { "Mat_FW_Silver", new Color(0.92f, 0.96f, 1.0f) },
            { "Mat_FW_White", new Color(1.0f, 1.0f, 1.0f) },
            { "Mat_FW_Ruby", new Color(1.0f, 0.08f, 0.15f) },
            { "Mat_FW_Purple", new Color(0.65f, 0.15f, 1.0f) }
        };

        Shader particleShader = Shader.Find("Mobile/Particles/Additive");
        if (particleShader == null) particleShader = Shader.Find("Particles/Standard Unlit");
        if (particleShader == null) particleShader = Shader.Find("Unlit/Color");
        if (particleShader == null) particleShader = Shader.Find("Standard");

        var createdMats = new Dictionary<string, Material>();
        foreach (var kvp in colorPalette)
        {
            string texPath = texDir + "/" + kvp.Key + ".png";
            if (!File.Exists(texPath))
            {
                Texture2D t = new Texture2D(16, 16, TextureFormat.RGB24, false);
                Color[] cols = new Color[16 * 16];
                for (int i = 0; i < cols.Length; i++) cols[i] = kvp.Value;
                t.SetPixels(cols);
                t.Apply();
                byte[] bytes = t.EncodeToPNG();
                File.WriteAllBytes(texPath, bytes);
                AssetDatabase.ImportAsset(texPath);
            }

            Texture2D texAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            if (texAsset != null) AssetImporter.GetAtPath(texPath).assetBundleName = "rgs";

            string mp = matDir + "/" + kvp.Key + ".mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(mp);
            if (m == null)
            {
                m = new Material(particleShader);
                m.color = kvp.Value;
                if (texAsset != null && m.HasProperty("_MainTex")) m.mainTexture = texAsset;
                AssetDatabase.CreateAsset(m, mp);
            }
            else
            {
                m.shader = particleShader;
                m.color = kvp.Value;
                if (texAsset != null && m.HasProperty("_MainTex")) m.mainTexture = texAsset;
                EditorUtility.SetDirty(m);
            }
            createdMats[kvp.Key] = m;
            AssetImporter.GetAtPath(mp).assetBundleName = "rgs";
        }
        AssetDatabase.SaveAssets();

        // 4. Create AnimatorController with looping state
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        if (animClip != null)
        {
            AnimatorStateMachine sm = controller.layers[0].stateMachine;
            AnimatorState loopState = sm.AddState("LoopFireworks");
            loopState.motion = animClip;
            sm.defaultState = loopState;
        }

        // 5. Build Root GameObject on Layer 0
        GameObject root = new GameObject("rgs");
        root.layer = 0;

        ModMe mm = root.AddComponent<ModMe>();
        mm.Scale = true;

        BoxCollider bc = root.AddComponent<BoxCollider>();
        bc.center = new Vector3(0f, 0.2f * S, 0f);
        bc.size = new Vector3(1.2f * S, 0.4f * S, 1.2f * S);

        // 6. Instantiate Model
        GameObject modelInst = Object.Instantiate(modelAsset, root.transform);
        modelInst.name = "Fireworks_Show";
        modelInst.transform.localPosition = Vector3.zero;
        modelInst.transform.localRotation = Quaternion.identity;
        modelInst.transform.localScale = Vector3.one * S; // 2x SCALE

        // Animator on modelInst matching channel paths
        Animator modelAnimator = modelInst.GetComponent<Animator>();
        if (modelAnimator == null) modelAnimator = modelInst.AddComponent<Animator>();
        modelAnimator.runtimeAnimatorController = controller;
        modelAnimator.applyRootMotion = false;
        modelAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        Animator rootAnimator = root.AddComponent<Animator>();
        rootAnimator.runtimeAnimatorController = controller;
        rootAnimator.applyRootMotion = false;
        rootAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        // 7. Mesh Optimization (Low-Poly Cache & Draw Call Performance)
        HashSet<Mesh> optimizedMeshes = new HashSet<Mesh>();
        MeshFilter[] meshFilters = modelInst.GetComponentsInChildren<MeshFilter>(true);
        foreach (MeshFilter mf in meshFilters)
        {
            if (mf != null && mf.sharedMesh != null && !optimizedMeshes.Contains(mf.sharedMesh))
            {
                MeshUtility.Optimize(mf.sharedMesh); // Optimizes triangle/vertex cache for mobile GPU
                optimizedMeshes.Add(mf.sharedMesh);
            }
        }
        Debug.Log("Optimized " + optimizedMeshes.Count + " meshes for mobile GPU performance.");

        // 8. Assign Multi-Colored Additive Materials
        Renderer[] renderers = modelInst.GetComponentsInChildren<Renderer>(true);
        Debug.Log("Assigning multi-color materials to " + renderers.Length + " renderers");
        foreach (Renderer r in renderers)
        {
            r.gameObject.layer = 0;
            string desc = r.gameObject.name.ToLower();
            MeshFilter mf = r.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null) desc += " " + mf.sharedMesh.name.ToLower();
            if (r.sharedMaterial != null) desc += " " + r.sharedMaterial.name.ToLower();

            Material selectedMat = createdMats["Mat_FW_Gold"]; // Default to vibrant golden burst

            if (desc.Contains("cyan")) selectedMat = createdMats["Mat_FW_Cyan"];
            else if (desc.Contains("magenta") || desc.Contains("pink")) selectedMat = createdMats["Mat_FW_Magenta"];
            else if (desc.Contains("lime") || desc.Contains("green")) selectedMat = createdMats["Mat_FW_Lime"];
            else if (desc.Contains("blue") || desc.Contains("deep_blue")) selectedMat = createdMats["Mat_FW_Blue"];
            else if (desc.Contains("amber")) selectedMat = createdMats["Mat_FW_Amber"];
            else if (desc.Contains("silver")) selectedMat = createdMats["Mat_FW_Silver"];
            else if (desc.Contains("white") || desc.Contains("warm_white")) selectedMat = createdMats["Mat_FW_White"];
            else if (desc.Contains("ruby") || desc.Contains("red")) selectedMat = createdMats["Mat_FW_Ruby"];
            else if (desc.Contains("purple") || desc.Contains("violet")) selectedMat = createdMats["Mat_FW_Purple"];
            else if (desc.Contains("gold")) selectedMat = createdMats["Mat_FW_Gold"];

            r.sharedMaterial = selectedMat;
        }

        // 9. Balanced Area Lighting (Lights up area by a few percent)
        // Ground-level ambient glow
        GameObject groundLightObj = new GameObject("GroundLight");
        groundLightObj.transform.SetParent(root.transform, false);
        groundLightObj.transform.localPosition = new Vector3(0f, 0.5f * S, 0f);
        Light groundLight = groundLightObj.AddComponent<Light>();
        groundLight.type = LightType.Point;
        groundLight.range = 30f * S;
        groundLight.intensity = 0.9f;
        groundLight.color = new Color(1.0f, 0.65f, 0.25f);

        // Sky-level atmospheric burst light
        GameObject skyLightObj = new GameObject("SkyBurstLight");
        skyLightObj.transform.SetParent(root.transform, false);
        skyLightObj.transform.localPosition = new Vector3(0f, 35f * S, 0f);
        Light skyLight = skyLightObj.AddComponent<Light>();
        skyLight.type = LightType.Point;
        skyLight.range = 80f * S;
        skyLight.intensity = 1.3f;
        skyLight.color = new Color(1.0f, 0.85f, 0.65f);

        // 10. Synced 59-Second 3D AudioSource
        AudioClip audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(wavPath);
        if (audioClip != null)
        {
            AudioSource audio = root.AddComponent<AudioSource>();
            audio.clip = audioClip;
            audio.loop = true;
            audio.playOnAwake = true;
            audio.spatialBlend = 1.0f;
            audio.minDistance = 20f;
            audio.maxDistance = 800f;
            audio.volume = 1.0f;
        }

        // 11. Save Prefab & Tag into 'rgs'
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);

        AssetImporter.GetAtPath(prefabPath).assetBundleName = "rgs";
        AssetImporter.GetAtPath(modelPath).assetBundleName = "rgs";
        AssetImporter.GetAtPath(wavPath).assetBundleName = "rgs";
        AssetImporter.GetAtPath(controllerPath).assetBundleName = "rgs";

        Debug.Log("🎉 Fireworks 2x Show Build complete: 2x scale, GPU-optimized meshes, multi-colored additive materials, and synced 59s audio!");
    }
}
