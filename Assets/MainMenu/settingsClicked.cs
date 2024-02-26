using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class settingsClicked : MonoBehaviour
{
    public GameObject settingsObj; 
    settingsMovement settingsMove;
    public TMP_Text gameName;
    public GameObject startButton;

    void Start()
    {
        settingsMove = settingsObj.GetComponent<settingsMovement>();
    }

    void Update(){}

    public void OnButtonClick()
    {
        gameName.gameObject.SetActive(false);
        startButton.gameObject.SetActive(false);
        settingsMove.settingsClicked = true;
        gameObject.SetActive(false);
        
    }
}
