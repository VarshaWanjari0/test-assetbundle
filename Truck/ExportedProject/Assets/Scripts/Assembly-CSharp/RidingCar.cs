using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class RidingCar : UnityEngine.MonoBehaviour
{
    // Fields
    public System.Boolean canIntract;;
    public UnityEngine.Transform leftHand;;
    public UnityEngine.Transform rightHand;;
    public UnityEngine.Transform leftFoot;;
    public UnityEngine.Transform rightFoot;;
    public UnityEngine.Transform sitPos;;
    public UnityEngine.Transform doorPos;;
    public UnityEngine.Transform camTarget;;
    public UnityEngine.GameObject carEnterButton;;
    public UnityEngine.GameObject interiorCam;;
    private System.Single collision_hit;;
    private System.Boolean getInPos1;;
    private System.Boolean getsInCar;;
    public System.Boolean ridingCar;;
    private System.Boolean doorAnim;;
    private System.Single transformSpeed;;
    private System.Single doorAngles;;
    private System.Single doorSpeed;;
    private System.Single playerDistance;;
    private System.Single ridingWeight;;
    public PlayerControl playerControl;;
    private UnityEngine.Animator _playerAnim;;
    private PlayerGun playerGun;;
    public CarControl _car;;
    public GlassGlobal frontGlass;;
    public UnityEngine.HingeJoint[] doorsHings;;
    private UnityEngine.Rigidbody[] rigParts;;
    private System.Int32 level;;
    public System.Int32 CamDis;;
    private System.Int32 playerMod;;
    // Methods
    private System.Void Start() { }
    private System.Void Update() { }
    private System.Void CarCamera() { }
    private System.Void PlayerControls() { }
    private System.Collections.IEnumerator RideCar() { return default; }
    private System.Collections.IEnumerator GetOffCar() { return default; }
    private System.Void OnTriggerEnter(UnityEngine.Collider hit) { }
    private System.Void OnCollisionEnter(UnityEngine.Collision hit) { }
    private System.Void LoseControl() { }
    private System.Void EnableLayerCollisions() { }
    private System.Void DisableLayerCollisions() { }
    public RidingCar() { }
}
public class PlayerControl : MonoBehaviour { }

public class PlayerGun : MonoBehaviour { }

public class GlassGlobal : MonoBehaviour { }
