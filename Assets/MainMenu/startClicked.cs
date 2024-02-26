using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class startClicked : MonoBehaviour
{
    public Camera cam; 
    CameraMovement camScript;

    void Start()
    {
        camScript = cam.GetComponent<CameraMovement>();
    }

    void Update(){}

    public void OnButtonClick()
    {
        camScript.started = true;
    }
}
