using UnityEngine;
public class leaveSettingsClicked : MonoBehaviour
{
    public GameObject leaveSettingsObj; 
    public GameObject settingsMenu;
    public GameObject fade;

    void Start(){}
    void Update(){}

    public void OnButtonClick()
    {
        leaveSettingsObj.GetComponent<leavingSettings>().leavingSettingsBool = true;
        settingsMenu.SetActive(false);
        fade.SetActive(true);
        gameObject.SetActive(false);
    }
}
