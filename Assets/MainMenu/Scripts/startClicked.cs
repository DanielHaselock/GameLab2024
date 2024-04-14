using UnityEngine;
using TMPro;

public class startClicked : MonoBehaviour
{
    public GameObject startPosition; 
    startMovement startScript;
    public GameObject goToSettingsButton;
    public GameObject creditsButton;
    public GameObject gameName;
    public bool closeDoor;
    public GameObject fade;

    void Start()
    {
        startScript = startPosition.GetComponent<startMovement>();
        closeDoor=false;
    }

    void Update(){}

    public void OnButtonClick()
    {
        if(closeDoor==false){
            startScript.started = true;
            goToSettingsButton.gameObject.SetActive(false);
            creditsButton.gameObject.SetActive(false);
            gameName.gameObject.SetActive(false);
            fade.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
