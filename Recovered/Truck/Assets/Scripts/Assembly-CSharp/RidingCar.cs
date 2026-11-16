// Schema-only compatibility stub: NO game logic. See README.
using UnityEngine;

public class RidingCar : MonoBehaviour
{
    public System.Boolean canIntract;
    public UnityEngine.Transform leftHand;
    public UnityEngine.Transform rightHand;
    public UnityEngine.Transform leftFoot;
    public UnityEngine.Transform rightFoot;
    public UnityEngine.Transform sitPos;
    public UnityEngine.Transform doorPos;
    public UnityEngine.Transform camTarget;
    public UnityEngine.GameObject carEnterButton;
    public UnityEngine.GameObject interiorCam;
    public System.Boolean ridingCar;
    public PlayerControl playerControl;
    public CarControl _car;
    public GlassGlobal frontGlass;
    public UnityEngine.HingeJoint[] doorsHings;
    public System.Int32 CamDis;
}
