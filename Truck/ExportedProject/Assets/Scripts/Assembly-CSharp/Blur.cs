using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Blur : UnityEngine.MonoBehaviour
{
    // Fields
    public System.Int32 iterations;
    public System.Single blurSpread;
    public UnityEngine.Shader blurShader;
    private static UnityEngine.Material m_Material;
    // Methods
    protected UnityEngine.Material get_material() { return default; }
    protected void OnDisable() { }
    protected void Start() { }
    public void FourTapCone(UnityEngine.RenderTexture source, UnityEngine.RenderTexture dest, System.Int32 iteration) { }
    private void DownSample4x(UnityEngine.RenderTexture source, UnityEngine.RenderTexture dest) { }
    private void OnRenderImage(UnityEngine.RenderTexture source, UnityEngine.RenderTexture destination) { }
    public Blur() { }
}