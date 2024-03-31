using UnityEngine;
public class leaveSettingsClicked : MonoBehaviour
{
    public GameObject leaveSettingsObj; 
    public GameObject fade;

    void Start(){}
    void Update(){}

    public void OnButtonClick()
    {
        leaveSettingsObj.GetComponent<leavingSettings>().leavingSettingsBool = true;
        fade.SetActive(true);
        gameObject.SetActive(false);
    }
}
