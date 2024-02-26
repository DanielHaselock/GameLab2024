using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExitGames.Client.Photon.StructWrapping;

public class settingsMovement : MonoBehaviour
{
    public Transform settingsPosition;
    public bool settingsClicked;
    public float moveSpeed = 5f;
    public GameObject cam;
    private Transform targetPoint;
    public GameObject settingsLeaveButton;
    void Start()
    {
        settingsClicked = false;
        targetPoint = settingsPosition;
    }

    void Update()
{
    Transform startingPoint = cam.GetComponent<CameraMovement>().startingPoint;
    float rotationSpeedCamera = 5f;
    if (settingsClicked)
    {

        //CAMERA MOVEMENT
        if(targetPoint == settingsPosition)
        {
            
            //float targetCameraRotationY = 0f;
            cam.transform.position = Vector3.MoveTowards(cam.transform.position, targetPoint.position, moveSpeed * Time.deltaTime);
            Quaternion targetCameraRotation = Quaternion.Euler(29f, -111f, 0.6f);
            cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, targetCameraRotation, rotationSpeedCamera * Time.deltaTime);
            
            
        }
        // Check if camera is within 1 unit of pointB
            if (Vector3.Distance(cam.transform.position, settingsPosition.position) < 1f )
            {
                settingsClicked = false;
                settingsLeaveButton.gameObject.SetActive(true);
            }
        
        
        
    }

    
}


}