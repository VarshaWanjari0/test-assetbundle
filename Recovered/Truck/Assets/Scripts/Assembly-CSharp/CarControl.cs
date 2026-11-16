// Schema-only compatibility stub: NO game logic. See README.
using UnityEngine;

public class CarControl : MonoBehaviour
{
    public ControlType controlType;
    public System.Single rpm;
    public System.Single currentSpeed;
    public System.Single maxTorque;
    public System.Single topSpeed;
    public System.Single reverseSpeed;
    public System.Single steer;
    public System.Single accelerate;
    public UnityEngine.Transform handel;
    public UnityEngine.Rigidbody m_rigidBody;
    public UnityEngine.Transform centerOfMass;
    public UnityEngine.WheelCollider[] wheelColliders;
    public UnityEngine.Transform[] tireMeshes;
    public System.Boolean controlable;
    public System.Boolean gasAndBrake;
    public UnityEngine.RigidbodyInterpolation bodyInterpolation;
    public UnityEngine.AudioSource engineLoop;
    public UnityEngine.AudioSource engineLoad;
    public System.Int32 gear;
    public System.Int32[] gears;
    public UnityEngine.Transform rpmSpeedometer;
    public UnityEngine.Transform speedSpeedometer;
    public UnityEngine.ParticleSystem[] exhaustSmokes;
    public UnityEngine.GameObject HeadLight;
    public UnityEngine.GameObject[] breakLight;
    public UnityEngine.AudioSource headlightAudio;
    public UnityEngine.AudioClip[] headlightSounds;
    public System.Boolean CarSteer;
    // Present in v91; absent from the original bundle component type tree.
    public UnityEngine.UI.Text speedText;
}
