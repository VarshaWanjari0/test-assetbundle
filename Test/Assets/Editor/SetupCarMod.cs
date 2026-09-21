using UnityEditor;
using UnityEngine;
using System.IO;

public class SetupCarMod
{
    [MenuItem("Tools/Setup New Car Mod")]
    public static void Build()
    {
        string prefabPath = "Assets/outsidemods/prefab/rgs.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) return;
        
        GameObject inst = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (inst == null) return;

        // 1. Adjust Wheel Colliders
        Transform colParent = inst.transform.Find("Wheel collider");
        if (colParent != null)
        {
            SetPos(colParent, "Col FL", new Vector3(-0.76f, 0.32f, 1.07f), 0.32f);
            SetPos(colParent, "Col FR", new Vector3( 0.76f, 0.32f, 1.07f), 0.32f);
            SetPos(colParent, "Col RL", new Vector3(-0.74f, 0.34f, -1.27f), 0.34f);
            SetPos(colParent, "Col RR", new Vector3( 0.74f, 0.34f, -1.27f), 0.34f);
        }

        // 2. Adjust Visual Wheel Models
        Transform wheelModel = inst.transform.Find("carzyCar/Wheel Model");
        if (wheelModel != null)
        {
            SetPos(wheelModel, "FL", new Vector3(-0.76f, 0.32f, 1.07f));
            SetPos(wheelModel, "FR", new Vector3( 0.76f, 0.32f, 1.07f));
            SetPos(wheelModel, "RL", new Vector3(-0.74f, 0.34f, -1.27f));
            SetPos(wheelModel, "RR", new Vector3( 0.74f, 0.34f, -1.27f));
        }

        // 3. Adjust Anchors: SitPos, DoorPos, Steering, Cam
        SetPos(inst.transform, "SitPosL", new Vector3(-0.30f, 0.50f, -0.15f));
        SetPos(inst.transform, "DoorPos", new Vector3(-1.15f, 0.20f, -0.10f));
        SetPos(inst.transform, "Cam", new Vector3(0.0f, 1.35f, -0.20f));

        Transform steer = inst.transform.Find("Interior/steering_dummy");
        if (steer != null) steer.localPosition = new Vector3(-0.29f, 0.58f, 0.08f);

        // 4. Update Box Collider to Mustang dimensions
        BoxCollider box = inst.GetComponent<BoxCollider>();
        if (box != null)
        {
            box.size = new Vector3(1.84f, 1.23f, 4.10f);
            box.center = new Vector3(0f, 0.62f, 0f);
        }

        PrefabUtility.SaveAsPrefabAsset(inst, prefabPath);
        Object.DestroyImmediate(inst);
        Debug.Log("✅ rgs.prefab successfully updated with newcar geometry!");
    }

    static void SetPos(Transform parent, string name, Vector3 pos, float radius = 0f)
    {
        Transform t = parent.Find(name);
        if (t != null)
        {
            t.localPosition = pos;
            if (radius > 0)
            {
                WheelCollider wc = t.GetComponent<WheelCollider>();
                if (wc != null) wc.radius = radius;
            }
        }
    }
}
