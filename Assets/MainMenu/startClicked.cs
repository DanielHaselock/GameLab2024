using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class startClicked : MonoBehaviour
{
    public Camera cam; 
    CameraMovement camScript;
    public GameObject goToSettingsButton;

    void Start()
    {
        camScript = cam.GetComponent<CameraMovement>();
    }

    void Update(){}

    public void OnButtonClick()
    {
        camScript.started = true;
        goToSettingsButton.gameObject.SetActive(false);
        this.gameObject.SetActive(false);
    }
}
