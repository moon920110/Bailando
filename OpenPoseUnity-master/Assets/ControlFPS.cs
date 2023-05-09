using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlFPS : MonoBehaviour
{
    public int targetFrameRate = 60;
    void Awake() {
        Application.targetFrameRate = targetFrameRate;
    }
}
