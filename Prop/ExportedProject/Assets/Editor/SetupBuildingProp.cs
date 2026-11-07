using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.IO;

public class SetupBuildingProp
{
    const float S = 2.5f; // Scale launcher box so it sits visibly and realistically on the ground

    [MenuItem("Tools/Build Firecracker Prop")]
    public static void Build()
    {
        string fbxPath = "Assets/Models/firecracker_skyshots_10x.fbx";
        string wavPath = "Assets/Audio/firecracker_skyshots_10x.wav";
        string prefabPath = "Assets/outsidemods/prefab/rgs.prefab";
        string controllerPath = "Assets/AnimatorController/Firecracker.controller";
        string matDir = "Assets/Material";

        if (!Directory.Exists(matDir)) Directory.CreateDirectory(matDir);
        if (!Directory.Exists("Assets/AnimatorController")) Directory.CreateDirectory("Assets/AnimatorController");
        if (!Directory.Exists(Path.GetDirectoryName(prefabPath))) Directory.CreateDirectory(Path.GetDirectoryName(prefabPath));

        // 1. Configure ModelImporter for Legacy/Generic Loop Animation
        ModelImporter importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
        if (importer != null)
        {
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.materialImportMode = ModelImporterMaterialImportMode.None; // We generate clean mobile shaders
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

        // 2. Extract AnimationClip from FBX
        AnimationClip animClip = null;
        Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        foreach (Object obj in subAssets)
        {
            if (obj is AnimationClip && !obj.name.StartsWith("__preview__"))
            {
                animClip = obj as AnimationClip;
                break;
            }
        }

        // 3. Create or Update AnimatorController with Loop State
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        if (animClip != null)
        {
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            AnimatorState loopState = stateMachine.AddState("LoopSkyshots");
            loopState.motion = animClip;
            stateMachine.defaultState = loopState;
        }

        // 4. Create Mobile Shaders / Materials for Firework Sparks and Launcher
        Shader additiveShader = Shader.Find("Mobile/Particles/Additive");
        if (additiveShader == null) additiveShader = Shader.Find("Particles/Standard Unlit");
        if (additiveShader == null) additiveShader = Shader.Find("Unlit/Color");

        Shader diffuseShader = Shader.Find("Mobile/Diffuse");
        if (diffuseShader == null) diffuseShader = Shader.Find("Standard");

        // 5. Build Root GameObject on Layer 0
        GameObject root = new GameObject("rgs");
        root.layer = 0;

        ModMe mm = root.AddComponent<ModMe>();
        mm.Scale = true;

        BoxCollider bc = root.AddComponent<BoxCollider>();
        bc.center = new Vector3(0f, 0.15f * S, 0f);
        bc.size = new Vector3(0.6f * S, 0.35f * S, 0.35f * S); // Exact snug fit around launcher battery

        // 6. Instantiate FBX Hierarchy
        GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        GameObject modelInst = Object.Instantiate(modelAsset, root.transform);
        modelInst.name = "Firecracker_Battery";
        modelInst.transform.localPosition = Vector3.zero;
        modelInst.transform.localRotation = Quaternion.identity;
        modelInst.transform.localScale = Vector3.one * S;

        // Assign Animator & Controller
        Animator animator = modelInst.GetComponent<Animator>();
        if (animator == null) animator = modelInst.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate; // Ensures animation runs even when looking up/away

        // Setup Renderer Materials with Colors & Additive Shaders
        Renderer[] renderers = modelInst.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderers)
        {
            r.gameObject.layer = 0;
            string rName = r.gameObject.name;
            Material mat = new Material(rName.Contains("Pyro") || rName.Contains("Flash") || rName.Contains("Orb") || rName.Contains("Comet") || rName.Contains("Ring") ? additiveShader : diffuseShader);

            if (rName.Contains("Gold")) mat.color = new Color(1.0f, 0.75f, 0.15f);
            else if (rName.Contains("Cyan")) mat.color = new Color(0.05f, 0.9f, 1.0f);
            else if (rName.Contains("Magenta") || rName.Contains("Hot")) mat.color = new Color(1.0f, 0.1f, 0.6f);
            else if (rName.Contains("White") || rName.Contains("Flash")) mat.color = Color.white;
            else if (rName.Contains("Amber")) mat.color = new Color(1.0f, 0.5f, 0.1f);
            else if (rName.Contains("Green") || rName.Contains("Lime")) mat.color = new Color(0.1f, 1.0f, 0.3f);
            else if (rName.Contains("Ruby") || rName.Contains("Red")) mat.color = new Color(0.9f, 0.1f, 0.15f);
            else if (rName.Contains("Violet") || rName.Contains("Blue")) mat.color = new Color(0.4f, 0.2f, 1.0f);
            else if (rName.Contains("Trim")) mat.color = new Color(0.95f, 0.8f, 0.2f);
            else if (rName.Contains("Tube")) mat.color = new Color(0.12f, 0.12f, 0.15f);
            else mat.color = new Color(0.7f, 0.08f, 0.1f); // Red wrap

            r.sharedMaterial = mat;
        }

        // 7. Dynamic Real-Time Point Lights for Blast Glow
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

        // 8. Attach Synced 42s Audio
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

        // 9. Save Prefab & Tag into 'rgs' AssetBundle
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);

        AssetImporter.GetAtPath(prefabPath).assetBundleName = "rgs";
        AssetImporter.GetAtPath(fbxPath).assetBundleName = "rgs";
        AssetImporter.GetAtPath(wavPath).assetBundleName = "rgs";
        AssetImporter.GetAtPath(controllerPath).assetBundleName = "rgs";

        Debug.Log("🎉 Setup complete: AnimatorController created, loop enabled, materials assigned, and tagged into rgs bundle!");
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
