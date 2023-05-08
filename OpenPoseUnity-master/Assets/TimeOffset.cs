using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeOffset : MonoBehaviour
{
    AudioSource audioSource;
    public float TimeOffsetSeconds = 0.0f;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.time = TimeOffsetSeconds;
    }

    void Update()
    {
        
    }
}