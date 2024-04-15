using UnityEngine;

public class leaveCreditsClicked : MonoBehaviour
{
    public GameObject leaveCreditsObj; 
    public GameObject fade;

    void Start()
    {}

    void Update(){}

    public void OnButtonClick()
    {
        leaveCreditsObj.GetComponent<leaveCredits>().leavingCreditsBool = true;
        fade.SetActive(true);
        gameObject.SetActive(false);
    }
}
