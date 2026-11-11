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
    public void CustomTexture() { }
    private System.Collections.IEnumerator LoadImage(System.String path) { return default; }
    public void Yellow() { }
    public void Green() { }
    public void Blue() { }
    public void White() { }
    public void Red() { }
    public void Violet() { }
    public void Orange() { }
    public void Black() { }
    public void RandomColor() { }
    public void RandomColorCycle() { }
    public void ResetRandomColorCycle() { }
    public void ChangeWheel() { }
    public void ColorRGS(System.Single value) { }
    public void ColorRim(System.Single value) { }
    public void ColorBlackWheel(System.Single value) { }
    public void ResetCarModification() { }
    public void HideWheel() { }
    public void MaxwheelSizeFrontMax() { }
    public void MaxwheelSizeFrontMin() { }
    public void MaxwheelSizeBackMax() { }
    public void MaxwheelSizeBackMin() { }
    public color_select() { }
}