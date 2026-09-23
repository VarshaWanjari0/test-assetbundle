using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ExplosionVehicle : UnityEngine.MonoBehaviour
{
    // Fields
    private System.Single randomTime;;
    private System.Boolean routineStarted;;
    public System.Int32 health;;
    public UnityEngine.GameObject Fire;;
    public UnityEngine.Transform explosionPrefab;;
    public System.Single minTime;;
    public System.Single maxTime;;
    public System.Single explosionRadius;;
    public System.Single explosionForce;;
    public enum explosionType type;;
    private RidingCar ridingCar;;
    private RidingJcb ridingJcb;;
    private RidingTank ridingTank;;
    private RidingBike ridingBike;;
    private RidingCarTraffic ridingCarTraffic;;
    private RidingAirPlane ridingAirPlane;;
    private RidingATV ridingATV;;
    private RidingBoat ridingBoat;;
    private AIVehicleD aIVehicleD;;
    private DestroyGameObject destroyGameObject;;
    private CarImpactCheck carImpactCheck;;
    public CarJointControl[] carjoint;;
    public UnityEngine.GameObject[] DestryedObject;;
    private MaterialChange[] matChange;;
    private System.Boolean fixV;;
    private System.Boolean fixE;;
    // Methods
    private System.Void Start() { }
    private System.Void FixedUpdate() { }
    private System.Collections.IEnumerator Fired() { return default; }
    private System.Collections.IEnumerator Explode() { return default; }
    public ExplosionVehicle() { }
}
public class AIVehicleD : MonoBehaviour { }
