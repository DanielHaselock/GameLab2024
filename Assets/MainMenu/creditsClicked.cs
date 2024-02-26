using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class creditsClicked : MonoBehaviour
{
    public GameObject creditsObj; 
    creditMovement creditsMove;
    public TMP_Text gameName;
    public GameObject startButton;

    void Start()
    {
        creditsMove = creditsObj.GetComponent<creditMovement>();
    }

    void Update(){}

    public void OnButtonClick()
    {
        gameName.gameObject.SetActive(false);
        startButton.gameObject.SetActive(false);
        creditsMove.creditsClicked = true;
        this.gameObject.SetActive(false);
    }
}
