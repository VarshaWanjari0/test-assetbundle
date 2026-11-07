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
        string modelPath = "Assets/Models/firecracker_skyshots_10x.dae";
        string wavPath = "Assets/Audio/firecracker_skyshots_10x.wav";
        string prefabPath = "Assets/outsidemods/prefab/rgs.prefab";
        string controllerPath = "Assets/AnimatorController/Firecracker.controller";

        if (!Directory.Exists("Assets/AnimatorController")) Directory.CreateDirectory("Assets/AnimatorController");
        if (!Directory.Exists(Path.GetDirectoryName(prefabPath))) Directory.CreateDirectory(Path.GetDirectoryName(prefabPath));

        // 1. Configure ModelImporter settings without redundant in-memory reimport
        ModelImporter importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
        if (importer != null && importer.animationType != ModelImporterAnimationType.Generic)
        {
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.importAnimation = true;
        }

        // 2. Load DAE Asset
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
                Debug.Log("Found AnimationClip in DAE: " + animClip.name);
                break;
            }
        }

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
        bc.center = new Vector3(0f, 0.15f * S, 0f);
        bc.size = new Vector3(0.6f * S, 0.35f * S, 0.35f * S);

        // 6. Instantiate Model
        GameObject modelInst = Object.Instantiate(modelAsset, root.transform);
        modelInst.name = "Firecracker_Battery";
        modelInst.transform.localPosition = Vector3.zero;
        modelInst.transform.localRotation = Quaternion.identity;
        modelInst.transform.localScale = Vector3.one * S;

        Animator animator = modelInst.GetComponent<Animator>();
        if (animator == null) animator = modelInst.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        // 7. Shaders & Material Colors
        Shader particleShader = Shader.Find("Mobile/Particles/Additive");
        if (particleShader == null) particleShader = Shader.Find("Particles/Standard Unlit");
        if (particleShader == null) particleShader = Shader.Find("Unlit/Color");
        if (particleShader == null) particleShader = Shader.Find("Mobile/Diffuse");
        if (particleShader == null) particleShader = Shader.Find("Standard");

        Shader solidShader = Shader.Find("Mobile/Diffuse");
        if (solidShader == null) solidShader = Shader.Find("Standard");

        Renderer[] renderers = modelInst.GetComponentsInChildren<Renderer>(true);
        Debug.Log("Found " + renderers.Length + " renderers in modelInst");
        foreach (Renderer r in renderers)
        {
            r.gameObject.layer = 0;
            string n = r.gameObject.name;
            bool isPyro = n.Contains("Pyro") || n.Contains("Flash") || n.Contains("Orb") || n.Contains("Comet") || n.Contains("Ring") || n.Contains("Streak");
            Material mat = new Material(isPyro ? particleShader : solidShader);

            if (n.Contains("Gold")) mat.color = new Color(1.0f, 0.75f, 0.15f);
            else if (n.Contains("Cyan")) mat.color = new Color(0.05f, 0.9f, 1.0f);
            else if (n.Contains("Magenta") || n.Contains("Hot")) mat.color = new Color(1.0f, 0.1f, 0.6f);
            else if (n.Contains("White") || n.Contains("Flash")) mat.color = Color.white;
            else if (n.Contains("Amber")) mat.color = new Color(1.0f, 0.5f, 0.1f);
            else if (n.Contains("Green") || n.Contains("Lime")) mat.color = new Color(0.1f, 1.0f, 0.3f);
            else if (n.Contains("Ruby") || n.Contains("Red")) mat.color = new Color(0.9f, 0.1f, 0.15f);
            else if (n.Contains("Violet") || n.Contains("Blue")) mat.color = new Color(0.4f, 0.2f, 1.0f);
            else if (n.Contains("Trim")) mat.color = new Color(0.95f, 0.8f, 0.2f);
            else if (n.Contains("Tube")) mat.color = new Color(0.12f, 0.12f, 0.15f);
            else mat.color = new Color(0.7f, 0.08f, 0.1f);

            r.sharedMaterial = mat;
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

        Debug.Log("🎉 Setup complete: DAE loaded with " + renderers.Length + " renderers, AnimatorController linked, and tagged into rgs bundle!");
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
