using UnityEngine;
public class leaveSettingsClicked : MonoBehaviour
{
    public GameObject leaveSettingsObj; 

    void Start(){}
    void Update(){}

    public void OnButtonClick()
    {
        leaveSettingsObj.GetComponent<leavingSettings>().leavingSettingsBool = true;
        gameObject.SetActive(false);
    }
}
