using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class visitCreditsClicked : MonoBehaviour
{
    public GameObject visitCreditObj; 
    public GameObject settingsButton;
    visitCredits visitCredits;
    public TMP_Text gameName;
    public GameObject startButton;

    void Start()
    {
        visitCredits = visitCreditObj.GetComponent<visitCredits>();
    }

    void Update(){}

    public void OnButtonClick()
    {
        gameName.gameObject.SetActive(false);
        startButton.gameObject.SetActive(false);
        settingsButton.gameObject.SetActive(false);
        visitCredits.creditsClicked = true;
        gameObject.SetActive(false);
    }
}
