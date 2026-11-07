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
        string modelPath = "Assets/Models/firecracker_skyshots_10x.fbx";
        string wavPath = "Assets/Audio/firecracker_skyshots_10x.wav";
        string prefabPath = "Assets/outsidemods/prefab/rgs.prefab";
        string controllerPath = "Assets/AnimatorController/Firecracker.controller";
        string matDir = "Assets/Material";

        if (!Directory.Exists(matDir)) Directory.CreateDirectory(matDir);
        if (!Directory.Exists("Assets/AnimatorController")) Directory.CreateDirectory("Assets/AnimatorController");
        if (!Directory.Exists(Path.GetDirectoryName(prefabPath))) Directory.CreateDirectory(Path.GetDirectoryName(prefabPath));

        // 1. Configure ModelImporter
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        ModelImporter importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
        if (importer != null)
        {
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.importAnimation = true;
            ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
            if (clips != null && clips.Length > 0)
            {
                foreach (var clip in clips)
                {
                    clip.loopTime = true;
                    clip.wrapMode = WrapMode.Loop;
                }
                importer.clipAnimations = clips;
            }
            importer.SaveAndReimport();
        }

        // 2. Load Model Asset
        GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (modelAsset == null)
        {
            throw new System.Exception("FATAL: Failed to load model asset at " + modelPath);
        }
        Debug.Log("Successfully loaded modelAsset: " + modelAsset.name);

        // 3. Find AnimationClip
        AnimationClip animClip = null;
        Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(modelPath);
        foreach (Object obj in subAssets)
        {
            if (obj is AnimationClip && !obj.name.StartsWith("__preview__"))
            {
                animClip = obj as AnimationClip;
                Debug.Log("Found AnimationClip in model: " + animClip.name);
                break;
            }
        }

        // 4. Create Persistent Materials on Disk
        Shader targetShader = Shader.Find("Mobile/Diffuse");
        if (targetShader == null) targetShader = Shader.Find("Standard");

        Shader addShader = Shader.Find("Mobile/Particles/Additive");
        if (addShader == null) addShader = targetShader;

        var matDefs = new System.Collections.Generic.Dictionary<string, Color>()
        {
            { "Mat_Gold", new Color(1.0f, 0.8f, 0.1f) },
            { "Mat_Cyan", new Color(0.05f, 0.9f, 1.0f) },
            { "Mat_Magenta", new Color(1.0f, 0.1f, 0.6f) },
            { "Mat_White", Color.white },
            { "Mat_Amber", new Color(1.0f, 0.5f, 0.1f) },
            { "Mat_Green", new Color(0.1f, 1.0f, 0.3f) },
            { "Mat_Red", new Color(0.9f, 0.1f, 0.15f) },
            { "Mat_Violet", new Color(0.5f, 0.2f, 1.0f) },
            { "Mat_DarkTube", new Color(0.12f, 0.12f, 0.15f) },
            { "Mat_BoxWrap", new Color(0.75f, 0.08f, 0.1f) }
        };

        var createdMats = new System.Collections.Generic.Dictionary<string, Material>();
        foreach (var kvp in matDefs)
        {
            string mp = matDir + "/" + kvp.Key + ".mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(mp);
            if (m == null)
            {
                bool isParticle = kvp.Key.Contains("Gold") || kvp.Key.Contains("Cyan") || kvp.Key.Contains("White") || kvp.Key.Contains("Magenta");
                m = new Material(isParticle ? addShader : targetShader);
                m.color = kvp.Value;
                AssetDatabase.CreateAsset(m, mp);
            }
            else
            {
                m.color = kvp.Value;
                EditorUtility.SetDirty(m);
            }
            createdMats[kvp.Key] = m;
            AssetImporter.GetAtPath(mp).assetBundleName = "rgs";
        }
        AssetDatabase.SaveAssets();

        // 5. Create AnimatorController with looping state
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        if (animClip != null)
        {
            AnimatorStateMachine sm = controller.layers[0].stateMachine;
            AnimatorState loopState = sm.AddState("LoopSkyshots");
            loopState.motion = animClip;
            sm.defaultState = loopState;
        }

        // 6. Build Root GameObject on Layer 0
        GameObject root = new GameObject("rgs");
        root.layer = 0;

        ModMe mm = root.AddComponent<ModMe>();
        mm.Scale = true;

        BoxCollider bc = root.AddComponent<BoxCollider>();
        bc.center = new Vector3(0f, 0.2f * S, 0f);
        bc.size = new Vector3(0.8f * S, 0.4f * S, 0.8f * S);

        // Put Animator on root like original decompiled rgs.prefab
        Animator rootAnimator = root.AddComponent<Animator>();
        rootAnimator.runtimeAnimatorController = controller;
        rootAnimator.applyRootMotion = false;
        rootAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        // 7. Instantiate Model into root
        GameObject modelInst = Object.Instantiate(modelAsset, root.transform);
        modelInst.name = "Firecracker_Battery";
        modelInst.transform.localPosition = Vector3.zero;
        modelInst.transform.localRotation = Quaternion.identity;
        modelInst.transform.localScale = Vector3.one * S;

        // Assign persistent disk materials and Layer 0
        Renderer[] renderers = modelInst.GetComponentsInChildren<Renderer>(true);
        Debug.Log("Assigning persistent materials to " + renderers.Length + " renderers");
        foreach (Renderer r in renderers)
        {
            r.gameObject.layer = 0;
            string n = r.gameObject.name;
            Material selectedMat = createdMats["Mat_BoxWrap"];

            if (n.Contains("Tube")) selectedMat = createdMats["Mat_DarkTube"];
            else if (n.Contains("Gold")) selectedMat = createdMats["Mat_Gold"];
            else if (n.Contains("Cyan")) selectedMat = createdMats["Mat_Cyan"];
            else if (n.Contains("Magenta") || n.Contains("Hot")) selectedMat = createdMats["Mat_Magenta"];
            else if (n.Contains("White") || n.Contains("Flash")) selectedMat = createdMats["Mat_White"];
            else if (n.Contains("Amber")) selectedMat = createdMats["Mat_Amber"];
            else if (n.Contains("Green")) selectedMat = createdMats["Mat_Green"];
            else if (n.Contains("Violet") || n.Contains("Blue")) selectedMat = createdMats["Mat_Violet"];

            r.sharedMaterial = selectedMat;
        }

        // 8. Blast Lights
        Color[] blastColors = new Color[] {
            new Color(1f, 0.84f, 0.0f), new Color(1f, 0.6f, 0.1f), new Color(0.9f, 0.95f, 1f),
            new Color(0.0f, 0.9f, 1.0f), new Color(1.0f, 0.1f, 0.6f), new Color(0.2f, 1.0f, 0.3f),
            new Color(1.0f, 0.2f, 0.2f), new Color(0.6f, 0.2f, 1.0f), new Color(1.0f, 0.9f, 0.3f),
            new Color(1.0f, 0.4f, 0.8f)
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
                l.range = 100f;
                l.intensity = 7f;
                l.color = blastColors[(i - 1) % blastColors.Length];
            }
        }

        // 9. Audio
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

        // 10. Save Prefab & Tag into 'rgs'
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);

        AssetImporter.GetAtPath(prefabPath).assetBundleName = "rgs";
        AssetImporter.GetAtPath(modelPath).assetBundleName = "rgs";
        AssetImporter.GetAtPath(wavPath).assetBundleName = "rgs";
        AssetImporter.GetAtPath(controllerPath).assetBundleName = "rgs";

        Debug.Log("🎉 Setup complete: Model loaded with persistent disk materials, Animator on root, and tagged into rgs bundle!");
    }

    private static Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform res = FindChildRecursive(parent.GetChild(i), name);
            if (res != null) return res;
        }
        return null;
    }
}
