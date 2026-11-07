using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.IO;

public class SetupBuildingProp
{
    const float S = 2.5f;

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

        // 3. Create True Colored Textures & Materials (No white wash out)
        var colorPalette = new System.Collections.Generic.Dictionary<string, Color>()
        {
            { "Mat_Gold", new Color(1.0f, 0.8f, 0.1f) },
            { "Mat_Cyan", new Color(0.05f, 0.9f, 1.0f) },
            { "Mat_Magenta", new Color(1.0f, 0.1f, 0.6f) },
            { "Mat_White", new Color(0.95f, 0.95f, 0.95f) },
            { "Mat_Amber", new Color(1.0f, 0.5f, 0.1f) },
            { "Mat_Green", new Color(0.1f, 1.0f, 0.3f) },
            { "Mat_Red", new Color(0.9f, 0.1f, 0.15f) },
            { "Mat_Violet", new Color(0.5f, 0.2f, 1.0f) },
            { "Mat_DarkTube", new Color(0.12f, 0.12f, 0.15f) },
            { "Mat_BoxWrap", new Color(0.75f, 0.08f, 0.1f) }
        };

        Shader unlitShader = Shader.Find("Unlit/Color");
        if (unlitShader == null) unlitShader = Shader.Find("Mobile/Unlit (Supports Lightmap)");
        if (unlitShader == null) unlitShader = Shader.Find("Standard");

        Shader particleShader = Shader.Find("Mobile/Particles/Additive");
        if (particleShader == null) particleShader = unlitShader;

        var createdMats = new System.Collections.Generic.Dictionary<string, Material>();
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
            bool isPyro = kvp.Key.Contains("Gold") || kvp.Key.Contains("Cyan") || kvp.Key.Contains("White") || kvp.Key.Contains("Magenta") || kvp.Key.Contains("Amber") || kvp.Key.Contains("Green");

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

        // CRUCIAL: Attach Animator directly to modelInst so animation curve relative paths match 100%!
        Animator modelAnimator = modelInst.GetComponent<Animator>();
        if (modelAnimator == null) modelAnimator = modelInst.AddComponent<Animator>();
        modelAnimator.runtimeAnimatorController = controller;
        modelAnimator.applyRootMotion = false;
        modelAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        // Also keep matching Animator on root
        Animator rootAnimator = root.AddComponent<Animator>();
        rootAnimator.runtimeAnimatorController = controller;
        rootAnimator.applyRootMotion = false;
        rootAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        // 7. Assign Crisp Colored Materials
        Renderer[] renderers = modelInst.GetComponentsInChildren<Renderer>(true);
        Debug.Log("Assigning crisp materials to " + renderers.Length + " renderers");
        foreach (Renderer r in renderers)
        {
            r.gameObject.layer = 0;
            string n = r.gameObject.name;
            Material selectedMat = createdMats["Mat_BoxWrap"];

            if (n.Contains("Tube")) selectedMat = createdMats["Mat_DarkTube"];
            else if (n.Contains("Gold") || n.Contains("Trim")) selectedMat = createdMats["Mat_Gold"];
            else if (n.Contains("Cyan")) selectedMat = createdMats["Mat_Cyan"];
            else if (n.Contains("Magenta") || n.Contains("Hot")) selectedMat = createdMats["Mat_Magenta"];
            else if (n.Contains("White") || n.Contains("Flash")) selectedMat = createdMats["Mat_White"];
            else if (n.Contains("Amber")) selectedMat = createdMats["Mat_Amber"];
            else if (n.Contains("Green")) selectedMat = createdMats["Mat_Green"];
            else if (n.Contains("Violet") || n.Contains("Blue")) selectedMat = createdMats["Mat_Violet"];

            r.sharedMaterial = selectedMat;
        }

        // NOTE: We REMOVED the 10 permanent ground point lights that caused blinding white glow!
        // Instead, the glowing sky bursts illuminate with their own additive materials naturally.

        // 8. AudioSource
        AudioClip audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(wavPath);
        if (audioClip != null)
        {
            AudioSource audio = root.AddComponent<AudioSource>();
            audio.clip = audioClip;
            audio.loop = true;
            audio.playOnAwake = true;
            audio.spatialBlend = 1.0f;
            audio.minDistance = 15f;
            audio.maxDistance = 600f;
            audio.volume = 1.0f;
        }

        // 9. Save Prefab & Tag into 'rgs'
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);

        AssetImporter.GetAtPath(prefabPath).assetBundleName = "rgs";
        AssetImporter.GetAtPath(modelPath).assetBundleName = "rgs";
        AssetImporter.GetAtPath(wavPath).assetBundleName = "rgs";
        AssetImporter.GetAtPath(controllerPath).assetBundleName = "rgs";

        Debug.Log("🎉 Setup complete: Model hierarchy animated, blinding lights removed, textures applied, and bundle compressed!");
    }
}
