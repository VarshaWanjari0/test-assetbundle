using UnityEngine;

// WheelSpinner — rotates visual tire meshes to match WheelCollider rotation.
// dump.cs CarControl exposes wheelColliders[] and tireMeshes[]; UpdateMeshesPositions()
// is expected to sync them. This script implements that sync at runtime.
//
// Add this to the same GameObject as CarControl, or to each wheel, and assign
// the matching WheelCollider + tire Mesh Transform.
public class WheelSpinner : MonoBehaviour
{
    public WheelCollider wheelCollider;
    public Transform tireMesh;

    void Update()
    {
        if (wheelCollider == null || tireMesh == null) return;

        tireMesh.Rotate(wheelCollider.rpm / 60f * 360f * Time.deltaTime, 0f, 0f);
    }
}
