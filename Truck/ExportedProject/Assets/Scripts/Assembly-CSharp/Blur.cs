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
    protected System.Void OnDisable() { }
    protected System.Void Start() { }
    public System.Void FourTapCone(UnityEngine.RenderTexture source, UnityEngine.RenderTexture dest, System.Int32 iteration) { }
    private System.Void DownSample4x(UnityEngine.RenderTexture source, UnityEngine.RenderTexture dest) { }
    private System.Void OnRenderImage(UnityEngine.RenderTexture source, UnityEngine.RenderTexture destination) { }
    public Blur() { }
}