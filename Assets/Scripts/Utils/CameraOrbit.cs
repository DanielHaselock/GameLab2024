using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    [SerializeField] private Transform camTarget;

    [SerializeField] private float rotationSpeed = 10;
    
    // Update is called once per frame
    void Update()
    {
        transform.LookAt(camTarget);
        transform.RotateAround(camTarget.position, Vector3.up, rotationSpeed * Time.deltaTime);        
    }
}
