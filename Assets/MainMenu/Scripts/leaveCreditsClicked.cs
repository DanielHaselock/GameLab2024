using UnityEngine;

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
