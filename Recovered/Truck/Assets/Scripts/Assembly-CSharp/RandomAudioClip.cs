// Schema-only compatibility stub: NO game logic. See README.
using UnityEngine;

public class RandomAudioClip : MonoBehaviour
{
    public UnityEngine.AudioSource audioSource;
    public UnityEngine.AudioClip[] audioClips;
    // Present in v91; absent from the original bundle component type tree.
    public System.Boolean onlyClipChange;
}
