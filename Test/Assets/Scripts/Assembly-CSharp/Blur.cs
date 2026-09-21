using UnityEngine;

public class Blur : MonoBehaviour
{
    public int iterations;
    public float blurSpread = 0.6f;
    public Shader blurShader;
    public float threshold = 0.25f;
    public float intensity = 0.75f;
    public float blurSize = 1f;
    public int blurIterations = 1;
    public int blurType;
    public Shader fastBloomShader;
}
