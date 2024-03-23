using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class leaveCreditsClicked : MonoBehaviour
{
    public GameObject leaveCreditsObj; 

    void Start()
    {}

    void Update(){}

    public void OnButtonClick()
    {
        
        leaveCreditsObj.GetComponent<leaveCredits>().leavingCreditsBool = true;
        gameObject.SetActive(false);
        
    }
}
