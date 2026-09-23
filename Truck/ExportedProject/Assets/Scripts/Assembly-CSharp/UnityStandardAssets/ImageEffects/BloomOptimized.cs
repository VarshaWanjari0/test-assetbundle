using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    public class BloomOptimized : MonoBehaviour
    {
        public float threshold = 0.25f;
        public float intensity = 0.75f;
        public float blurSize = 1.0f;
        public int blurIterations = 1;
        public Shader fastBloomShader;
    }
}