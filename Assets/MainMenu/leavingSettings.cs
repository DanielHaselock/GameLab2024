using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExitGames.Client.Photon.StructWrapping;

public class leavingSettings : MonoBehaviour
{
    public bool leavingSettingsBool;
    public float moveSpeed = 5f;
    public GameObject cam;
    public TMP_Text gameName;
    public GameObject settings;
    public GameObject startButton;
    private Transform targetPoint;
    private Transform startingPoint;

    void Start()
    {
        leavingSettingsBool = false;
        
    }

    void Update()
{
    startingPoint = cam.GetComponent<CameraMovement>().startingPoint;
    if (leavingSettingsBool)
    {
        //CAMERA MOVEMENT
        float rotationSpeedCamera = 5f;
        Vector3 targetCameraPosition = new Vector3(6.5f, 3.5f, 3.5f);
        cam.transform.position = Vector3.MoveTowards(cam.transform.position, targetCameraPosition, moveSpeed * Time.deltaTime);
        //cam.transform.position = Vector3.MoveTowards(cam.transform.position, targetPoint.position, moveSpeed * Time.deltaTime);
            
        // Check if camera is within 1 unit of pointB
            if (Vector3.Distance(cam.transform.position, startingPoint.position) < 1f || targetPoint == startingPoint)
            {
                targetPoint = startingPoint;
                float targetCameraRotationY = -122f;
                float targetCameraRotationX = 16f;
                Quaternion targetCameraRotation = Quaternion.Euler(targetCameraRotationX, targetCameraRotationY, 0f);
                cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, targetCameraRotation, rotationSpeedCamera * Time.deltaTime);
                gameName.gameObject.SetActive(true);

                if(Vector3.Distance(cam.transform.position, targetCameraPosition)<0.01f)
                {
                    startButton.gameObject.SetActive(true);
                    settings.gameObject.SetActive(true);
                    leavingSettingsBool=false;

                }
            }
        
        
        
    }

    
}


}