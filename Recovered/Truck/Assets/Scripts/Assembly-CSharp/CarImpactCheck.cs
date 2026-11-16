// Schema-only compatibility stub: NO game logic. See README.
using UnityEngine;

public class CarImpactCheck : MonoBehaviour
{
    public UnityEngine.GameObject glassImpact;
    public UnityEngine.GameObject bodyImpactBig;
    public UnityEngine.GameObject bodyImpactSmall;
    public UnityEngine.GameObject sparkImpact;
    public UnityEngine.AudioSource crashSound;
    public UnityEngine.AudioClip[] smallCrash;
    public UnityEngine.AudioClip[] mediumCrash;
    public UnityEngine.AudioClip[] largeCrash;
    public System.Single _colDist;
    public System.Boolean damage;
}
