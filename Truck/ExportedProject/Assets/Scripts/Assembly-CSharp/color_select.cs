using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class color_select : UnityEngine.MonoBehaviour
{
    // Fields
    public UnityEngine.Color[] _color;
    public meshRenders[] meshRenderers;
    public wheelModified[] Wheels;
    public UnityEngine.GameObject[] wheelCollider;
    public UnityEngine.Material[] carMaterials;
    public System.Boolean wheelModfied;
    public System.Boolean wheelSize;
    public System.Single maxSize;
    private System.Int32 w;
    // Methods
    public System.Void CustomTexture() { }
    private System.Collections.IEnumerator LoadImage(System.String path) { return default; }
    public System.Void Yellow() { }
    public System.Void Green() { }
    public System.Void Blue() { }
    public System.Void White() { }
    public System.Void Red() { }
    public System.Void Violet() { }
    public System.Void Orange() { }
    public System.Void Black() { }
    public System.Void RandomColor() { }
    public System.Void RandomColorCycle() { }
    public System.Void ResetRandomColorCycle() { }
    public System.Void ChangeWheel() { }
    public System.Void ColorRGS(System.Single value) { }
    public System.Void ColorRim(System.Single value) { }
    public System.Void ColorBlackWheel(System.Single value) { }
    public System.Void ResetCarModification() { }
    public System.Void HideWheel() { }
    public System.Void MaxwheelSizeFrontMax() { }
    public System.Void MaxwheelSizeFrontMin() { }
    public System.Void MaxwheelSizeBackMax() { }
    public System.Void MaxwheelSizeBackMin() { }
    public color_select() { }
}