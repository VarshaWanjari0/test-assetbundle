using UnityEngine;

public class CarImpactCheck : MonoBehaviour
{
    public GameObject glassImpact;
    public GameObject bodyImpactBig;
    public GameObject bodyImpactSmall;
    public GameObject sparkImpact;
    public AudioSource crashSound;
    public AudioClip[] smallCrash;
    public AudioClip[] mediumCrash;
    public AudioClip[] largeCrash;
    public float _colDist = 8f;
    public int damage = 1;
}
