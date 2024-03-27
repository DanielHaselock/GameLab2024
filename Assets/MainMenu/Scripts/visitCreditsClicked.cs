using UnityEngine;
using TMPro;

public class visitCreditsClicked : MonoBehaviour
{
    public GameObject visitCreditObj; 
    public GameObject settingsButton;
    visitCredits visitCredits;
    public TMP_Text gameName;
    public GameObject startButton;
    public bool closeDoor;

    void Start()
    {
        visitCredits = visitCreditObj.GetComponent<visitCredits>();
        closeDoor = startButton.GetComponent<startClicked>().closeDoor;
    }

    void Update(){closeDoor = startButton.GetComponent<startClicked>().closeDoor;}

    public void OnButtonClick()
    {
        if(closeDoor==false){
            gameName.gameObject.SetActive(false);
            startButton.gameObject.SetActive(false);
            settingsButton.gameObject.SetActive(false);
            visitCredits.creditsClicked = true;
            gameObject.SetActive(false);
        }
    }
}
