using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
public class returnClicked : MonoBehaviour
{
    public GameObject returnObj;
    public GameObject startGameButton;
    public GameObject nameField;
    returnMovement returnMove;
    public GameObject fade;

    void Start()
    {
        returnMove = returnObj.GetComponent<returnMovement>();
    }

    void Update(){}

    public void OnButtonClick()
    {
        returnMove.returning = true;
        fade.SetActive(true);
        gameObject.SetActive(false);
        startGameButton.SetActive(false);
        nameField.SetActive(false);
    }
}
