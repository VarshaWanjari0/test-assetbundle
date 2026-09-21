using UnityEngine;
using System.Collections;

public class RidingCar : MonoBehaviour
{
    public bool canIntract = true;
    public Transform leftHand;
    public Transform rightHand;
    public Transform leftFoot;
    public Transform rightFoot;
    public Transform sitPos;
    public Transform doorPos;
    public Transform camTarget;
    public GameObject carEnterButton;
    public GameObject interiorCam;
    public bool ridingCar;
    public Component playerControl;
    public CarControl _car;
    public Component frontGlass;
    public HingeJoint[] doorsHings;
    public int CamDis = 15;
}
