using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformRotationZero : MonoBehaviour
{
    Transform transformForRotationZero;
    // Start is called before the first frame update
    void Start()
    {
        
        transformForRotationZero = gameObject.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        transformForRotationZero.rotation = Quaternion.Euler(0, 0, 0);
    }
}
