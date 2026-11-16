// Schema-only compatibility stub: NO game logic. See README.
using UnityEngine;

public class ExplosionVehicle : MonoBehaviour
{
    public System.Int32 health;
    public UnityEngine.GameObject Fire;
    public UnityEngine.Transform explosionPrefab;
    public System.Single minTime;
    public System.Single maxTime;
    public System.Single explosionRadius;
    public System.Single explosionForce;
    public explosionType type;
    public CarJointControl[] carjoint;
    public UnityEngine.GameObject[] DestryedObject;
}
