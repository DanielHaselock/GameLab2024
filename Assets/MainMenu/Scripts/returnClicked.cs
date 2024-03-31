using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class returnClicked : MonoBehaviour
{
    public GameObject returnObj; 
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
    }
}
