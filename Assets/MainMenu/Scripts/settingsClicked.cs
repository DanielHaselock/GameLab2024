using UnityEngine;
using TMPro;

public class settingsClicked : MonoBehaviour
{
    public GameObject settingsObj; 
    settingsMovement settingsMove;
    public TMP_Text gameName;
    public GameObject startButton;
    public GameObject creditsButton;
    public bool closeDoor;
    public GameObject fade;

    void Start()
    {
        settingsMove = settingsObj.GetComponent<settingsMovement>();
        closeDoor = startButton.GetComponent<startClicked>().closeDoor;
    }

    void Update(){closeDoor = startButton.GetComponent<startClicked>().closeDoor;}

    public void OnButtonClick()
    {
        if(closeDoor==false)
        {
            gameName.gameObject.SetActive(false);
            startButton.gameObject.SetActive(false);
            creditsButton.gameObject.SetActive(false);
            settingsMove.settingsClicked = true;
            fade.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
