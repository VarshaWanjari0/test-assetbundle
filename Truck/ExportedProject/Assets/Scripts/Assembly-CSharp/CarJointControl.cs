using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CarJointControl : UnityEngine.MonoBehaviour
{
    // Fields
    public GlassGlobal glass;;
    public UnityEngine.HingeJoint hingJoint;;
    public UnityEngine.Rigidbody _rigidbody;;
    public System.Boolean isDoor;;
    public UnityEngine.AudioClip[] doorSounds;;
    public UnityEngine.AudioSource doorAudio;;
    public System.Boolean swingOnX;;
    public System.Boolean swingOnZ;;
    public System.Boolean maxLock;;
    private System.Single maxLimit;;
    private System.Single minLimit;;
    public System.Int32 health;;
    public System.Boolean DropThisDoor;;
    // Methods
    private System.Void Awake() { }
    private System.Void Update() { }
    private System.Void OnCollisionEnter(UnityEngine.Collision _Hit) { }
    public System.Void DropThisPiece() { }
    public System.Void LockJoint() { }
    public System.Void UnLockJoint() { }
    private System.Void CheckCaput() { }
    public CarJointControl() { }
}
public class GlassGlobal : MonoBehaviour { }
