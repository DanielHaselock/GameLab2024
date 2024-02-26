using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class leaveSettingsClicked : MonoBehaviour
{
    public GameObject leaveSettingsObj; 
    //leaveSettingMovement leaveSettingsMove;

    void Start()
    {
        //leaveSettingsMove = leaveSettingsObj.GetComponent<leaveSettingMovement>();
    }

    void Update(){}

    public void OnButtonClick()
    {
        
        leaveSettingsObj.GetComponent<leavingSettings>().leavingSettingsBool = true;
        gameObject.SetActive(false);
        
    }
}
