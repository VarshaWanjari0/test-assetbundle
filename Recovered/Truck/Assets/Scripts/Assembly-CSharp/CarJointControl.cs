// Schema-only compatibility stub: NO game logic. See README.
using UnityEngine;

public class CarJointControl : MonoBehaviour
{
    public GlassGlobal glass;
    public UnityEngine.HingeJoint hingJoint;
    public UnityEngine.Rigidbody _rigidbody;
    public System.Boolean isDoor;
    public UnityEngine.AudioClip[] doorSounds;
    public UnityEngine.AudioSource doorAudio;
    public System.Boolean swingOnX;
    public System.Boolean swingOnZ;
    public System.Boolean maxLock;
    public System.Int32 health;
    public System.Boolean DropThisDoor;
}
