using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SetupBuildingProp
{
    // 2X BIGGER THAN BEFORE (Was 2.5f, now 5.0f for massive 2x sky presence)
    const float S = 5.0f;

    [MenuItem("Tools/Build Firecracker Prop")]
    public static void Build()
    {
        string modelPath = "Assets/Models/firecracker_skyshots_10x.glb";
        string wavPath = "Assets/Audio/firecracker_skyshots_10x.wav";
        string prefabPath = "Assets/outsidemods/prefab/rgs.prefab";
        string controllerPath = "Assets/AnimatorController/Firecracker.controller";
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

        // 3. Multi-Color Palette (All 10 skyshot color themes)
        var colorPalette = new Dictionary<string, Color>()
        {
            { "Mat_Gold", new Color(1.0f, 0.70f, 0.12f) },
            { "Mat_GoldTrim", new Color(0.92f, 0.68f, 0.16f) },
            { "Mat_Amber", new Color(1.0f, 0.42f, 0.05f) },
            { "Mat_White", new Color(1.0f, 0.98f, 0.92f) },
            { "Mat_Cyan", new Color(0.04f, 0.88f, 1.0f) },
            { "Mat_Magenta", new Color(1.0f, 0.07f, 0.52f) },
            { "Mat_Violet", new Color(0.60f, 0.12f, 1.0f) },
            { "Mat_Mint", new Color(0.10f, 1.0f, 0.58f) },
            { "Mat_Yellow", new Color(1.0f, 0.92f, 0.08f) },
            { "Mat_Ruby", new Color(1.0f, 0.06f, 0.12f) },
            { "Mat_Rose", new Color(1.0f, 0.22f, 0.66f) },
            { "Mat_Green", new Color(0.05f, 1.0f, 0.22f) },
            { "Mat_Lime", new Color(0.62f, 1.0f, 0.06f) },
            { "Mat_Silver", new Color(0.85f, 0.94f, 1.0f) },
            { "Mat_Coral", new Color(1.0f, 0.28f, 0.08f) },
            { "Mat_Blue", new Color(0.08f, 0.36f, 1.0f) },
            { "Mat_DarkTube", new Color(0.10f, 0.10f, 0.12f) },
            { "Mat_BoxWrap", new Color(0.68f, 0.06f, 0.09f) }
        };

        Shader unlitShader = Shader.Find("Unlit/Color");
        if (unlitShader == null) unlitShader = Shader.Find("Mobile/Unlit (Supports Lightmap)");
        if (unlitShader == null) unlitShader = Shader.Find("Standard");

        Shader particleShader = Shader.Find("Mobile/Particles/Additive");
        if (particleShader == null) particleShader = unlitShader;

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
            bool isPyro = !kvp.Key.Contains("DarkTube") && !kvp.Key.Contains("BoxWrap") && !kvp.Key.Contains("GoldTrim");

            if (m == null)
            {
                m = new Material(isPyro ? particleShader : unlitShader);
                m.color = kvp.Value;
                if (texAsset != null && m.HasProperty("_MainTex")) m.mainTexture = texAsset;
                AssetDatabase.CreateAsset(m, mp);
            }
            else
            {
                m.shader = isPyro ? particleShader : unlitShader;
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
            AnimatorState loopState = sm.AddState("LoopSkyshots");
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
        bc.size = new Vector3(0.8f * S, 0.4f * S, 0.8f * S);

        // 6. Instantiate GLB Model
        GameObject modelInst = Object.Instantiate(modelAsset, root.transform);
        modelInst.name = "Firecracker_Battery";
        modelInst.transform.localPosition = Vector3.zero;
        modelInst.transform.localRotation = Quaternion.identity;
        modelInst.transform.localScale = Vector3.one * S;

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

        // 7. Mesh Optimization (Low-Poly Vertex Cache Coherence for Mobile GPU)
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
        Debug.Log("Optimized " + optimizedMeshes.Count + " meshes for mobile GPU cache and performance.");

        // 8. Assign Multi-Colored Materials
        Renderer[] renderers = modelInst.GetComponentsInChildren<Renderer>(true);
        Debug.Log("Assigning original animation colors to " + renderers.Length + " renderers");
        foreach (Renderer r in renderers)
        {
            r.gameObject.layer = 0;
            string desc = r.gameObject.name;
            if (r.sharedMaterial != null) desc += " " + r.sharedMaterial.name;
            MeshFilter mf = r.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null) desc += " " + mf.sharedMesh.name;

            Material selectedMat = createdMats["Mat_Gold"]; // Default to golden sparkle

            // Launcher Hardware
            if (desc.Contains("Tube") || desc.Contains("MortarTube")) selectedMat = createdMats["Mat_DarkTube"];
            else if (desc.Contains("LauncherBox") || desc.Contains("Battery_Casing") || desc.Contains("Launcher_Red")) selectedMat = createdMats["Mat_BoxWrap"];
            else if (desc.Contains("Trim") || desc.Contains("Gold_Trim") || desc.Contains("Battery_Trim")) selectedMat = createdMats["Mat_GoldTrim"];
            // Sky & Burst Colors
            else if (desc.Contains("Cyan")) selectedMat = createdMats["Mat_Cyan"];
            else if (desc.Contains("Magenta") || desc.Contains("Hot")) selectedMat = createdMats["Mat_Magenta"];
            else if (desc.Contains("Violet")) selectedMat = createdMats["Mat_Violet"];
            else if (desc.Contains("Mint")) selectedMat = createdMats["Mat_Mint"];
            else if (desc.Contains("Amber") || desc.Contains("Kamuro")) selectedMat = createdMats["Mat_Amber"];
            else if (desc.Contains("White") || desc.Contains("Flash") || desc.Contains("Muzzle")) selectedMat = createdMats["Mat_White"];
            else if (desc.Contains("Emerald") || desc.Contains("Green")) selectedMat = createdMats["Mat_Green"];
            else if (desc.Contains("Lime")) selectedMat = createdMats["Mat_Lime"];
            else if (desc.Contains("Yellow") || desc.Contains("Solar")) selectedMat = createdMats["Mat_Yellow"];
            else if (desc.Contains("Ruby") || desc.Contains("Crimson")) selectedMat = createdMats["Mat_Ruby"];
            else if (desc.Contains("Rose") || desc.Contains("Pink")) selectedMat = createdMats["Mat_Rose"];
            else if (desc.Contains("Coral") || desc.Contains("Sunset")) selectedMat = createdMats["Mat_Coral"];
            else if (desc.Contains("Sapphire") || desc.Contains("Blue")) selectedMat = createdMats["Mat_Blue"];
            else if (desc.Contains("Silver") || desc.Contains("Titanium")) selectedMat = createdMats["Mat_Silver"];
            else if (desc.Contains("Gold") || desc.Contains("Imperial")) selectedMat = createdMats["Mat_Gold"];

            r.sharedMaterial = selectedMat;
        }

        // 9. Balanced Area Lighting (Scaled 2x for massive presence)
        GameObject groundLightObj = new GameObject("GroundAmbientLight");
        groundLightObj.transform.SetParent(root.transform, false);
        groundLightObj.transform.localPosition = new Vector3(0f, 0.4f * S, 0f);
        Light groundLight = groundLightObj.AddComponent<Light>();
        groundLight.type = LightType.Point;
        groundLight.range = 35f * (S / 2.5f);
        groundLight.intensity = 1.0f;
        groundLight.color = new Color(1.0f, 0.65f, 0.25f);

        GameObject skyLightObj = new GameObject("SkyBurstAtmosphereLight");
        skyLightObj.transform.SetParent(root.transform, false);
        skyLightObj.transform.localPosition = new Vector3(0f, 32f * S, 0f);
        Light skyLight = skyLightObj.AddComponent<Light>();
        skyLight.type = LightType.Point;
        skyLight.range = 80f * (S / 2.5f);
        skyLight.intensity = 1.4f;
        skyLight.color = new Color(1.0f, 0.85f, 0.65f);

        // 10. AudioSource (42s loop)
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

        Debug.Log("🎉 Setup complete: 2x Big Skyshot with GPU mesh optimization, multi-color palette, and synced 42s audio!");
    }
}
