using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CarImpactCheck : UnityEngine.MonoBehaviour
{
    // Fields
    public UnityEngine.GameObject glassImpact;
    public UnityEngine.GameObject bodyImpactBig;
    public UnityEngine.GameObject bodyImpactSmall;
    public UnityEngine.GameObject sparkImpact;
    private UnityEngine.Vector3 _contactPoint;
    private UnityEngine.Vector3 _contactDirection;
    public UnityEngine.AudioSource crashSound;
    public UnityEngine.AudioClip[] smallCrash;
    public UnityEngine.AudioClip[] mediumCrash;
    public UnityEngine.AudioClip[] largeCrash;
    private UnityEngine.Rigidbody[] bodyParts;
    private UnityEngine.Transform player;
    public System.Single _colDist;
    public System.Boolean damage;
    private System.Int32 playerMod;
    // Methods
    private void Start() { }
    private void Update() { }
    private void OnCollisionEnter(UnityEngine.Collision _hit) { }
    private void OnCollisionStay(UnityEngine.Collision _hit) { }
    public CarImpactCheck() { }
}