// Schema-only compatibility stub: NO game logic. See README.
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
public class BloomOptimized : MonoBehaviour
{
    public enum BlurType { Standard = 0, Sgx = 1 }
    public System.Single threshold;
    public System.Single intensity;
    public System.Single blurSize;
    public System.Int32 blurIterations;
    public BlurType blurType;
    public UnityEngine.Shader fastBloomShader;
}
}
