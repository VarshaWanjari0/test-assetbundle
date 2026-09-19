using UnityEngine;

public class ExplosionVehicle : MonoBehaviour
{
    public float health = 100f;
    public GameObject Fire;
    public GameObject explosionPrefab;
    public float minTime = 0.2f;
    public float maxTime = 0.5f;
    public float explosionRadius = 5f;
    public float explosionForce = 5000f;
    public int type;
    public CarJointControl[] carjoint;
    public GameObject[] DestryedObject;
}
