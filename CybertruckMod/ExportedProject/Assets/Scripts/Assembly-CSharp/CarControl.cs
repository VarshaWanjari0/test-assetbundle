using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CarControl : UnityEngine.MonoBehaviour
{
    // Fields
    public ControlType controlType;
    private System.Int32 Controls;
    public System.Single rpm;
    public System.Single currentSpeed;
    public System.Single maxTorque;
    public System.Single topSpeed;
    public System.Single reverseSpeed;
    private System.Single tempBrakeTorque;
    public System.Single steer;
    private System.Single steerAngle;
    public System.Single accelerate;
    private System.Single mySideFriction;
    private System.Single myForwardFriction;
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
    private System.Single engineRpmPointer;
    public UnityEngine.Transform rpmSpeedometer;
    public UnityEngine.Transform speedSpeedometer;
    public UnityEngine.ParticleSystem[] exhaustSmokes;
    public UnityEngine.GameObject HeadLight;
    public UnityEngine.GameObject[] breakLight;
    private System.Boolean on;
    public UnityEngine.AudioSource headlightAudio;
    public UnityEngine.AudioClip[] headlightSounds;
    public System.Boolean CarSteer;
    public UnityEngine.UI.Text speedText;
    // Methods
    private void Start() { }
    private void Update() { }
    private void FixedUpdate() { }
    private void Control() { }
    private void CarLights() { }
    private void UpdateMeshesPositions() { }
    private void CarEngine() { }
    private void OnDrawGizmos() { }
    public CarControl() { }
}