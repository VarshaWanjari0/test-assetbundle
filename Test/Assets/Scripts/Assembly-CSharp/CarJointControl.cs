using UnityEngine;

public class CarJointControl : MonoBehaviour
{
    public GameObject glass;
    public HingeJoint hingJoint;
    public Rigidbody _rigidbody;
    public int isDoor = 1;
    public AudioClip[] doorSounds;
    public AudioSource doorAudio;
    public int swingOnX;
    public int swingOnZ;
    public int maxLock;
    public float health = 100f;
    public int DropThisDoor = 1;
}
