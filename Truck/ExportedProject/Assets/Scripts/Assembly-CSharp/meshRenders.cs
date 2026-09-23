using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class meshRenders : UnityEngine.MonoBehaviour
{
    // Fields
    public UnityEngine.MeshRenderer mesh;
    public System.Int32 mat;
    public UnityEngine.Material[] ColorCycle;
    public System.Boolean CycleOFF;
    private UnityEngine.Material[] originalMaterials;
    private System.Boolean once;
    // Methods
    public void ColorCycleFunction() { }
    public void ResetColorCycle() { }
    public meshRenders() { }
}