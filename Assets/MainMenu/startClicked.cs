using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class startClicked : MonoBehaviour
{
    public Camera cam; 
    CameraMovement camScript;
    public GameObject goToSettingsButton;
    public GameObject creditsButton;

    public TMP_Text gameName;
    public bool closeDoor;

    void Start()
    {
        camScript = cam.GetComponent<CameraMovement>();
        closeDoor=false;
    }

    void Update(){}

    public void OnButtonClick()
    {
        if(closeDoor==false){
            camScript.started = true;
            goToSettingsButton.gameObject.SetActive(false);
            creditsButton.gameObject.SetActive(false);
            gameName.gameObject.SetActive(false);;
            this.gameObject.SetActive(false);

        }
    }
}
