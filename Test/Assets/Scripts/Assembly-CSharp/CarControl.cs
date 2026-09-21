using UnityEngine;

public class CarControl : MonoBehaviour
{
    public int controlType;
    public float rpm;
    public float currentSpeed;
    public float maxTorque = 10000f;
    public float topSpeed = 200f;
    public float reverseSpeed = 40f;
    public float steer;
    public float accelerate;
    public Transform handel;
    public Rigidbody m_rigidBody;
    public Transform centerOfMass;
    public WheelCollider[] wheelColliders;
    public Transform[] tireMeshes;
    public int controlable;
    public int gasAndBrake;
    public int bodyInterpolation = 1;
    public AudioSource engineLoop;
    public AudioSource engineLoad;
    public int gear;
    public int[] gears = new int[] { 50, 100, 150, 200 };
    public Transform rpmSpeedometer;
    public Transform speedSpeedometer;
    public ParticleSystem[] exhaustSmokes;
    public GameObject HeadLight;
    public GameObject[] breakLight;
    public AudioSource headlightAudio;
    public AudioClip[] headlightSounds;
    public int CarSteer = 1;
}
