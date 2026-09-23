using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class RandomAudioClip : UnityEngine.MonoBehaviour
{
    // Fields
    public UnityEngine.AudioSource audioSource;
    public UnityEngine.AudioClip[] audioClips;
    public System.Boolean onlyClipChange;
    // Methods
    private System.Void Start() { }
    public RandomAudioClip() { }
}