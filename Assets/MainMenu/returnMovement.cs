using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExitGames.Client.Photon.StructWrapping;

public class returnMovement : MonoBehaviour
{
    public Transform pointB;
    //public Transform pointC;
    public bool returning;
    public float moveSpeed = 5f;
    public GameObject cam;
    public TMP_Text gameName;
    public GameObject door;
    public GameObject startButton;
    private Transform targetPoint;
    private Transform startingPoint;
    public GameObject goToSettingsButton;

    void Start()
    {
        returning = false;
        targetPoint = pointB;
    }

    void Update()
{
    startingPoint = cam.GetComponent<CameraMovement>().startingPoint;
    float rotationSpeedCamera = 5f;
    if (returning)
    {
       
        gameName.gameObject.SetActive(false);

        //CAMERA MOVEMENT
        if(targetPoint == pointB)
        {
            
            float targetCameraRotationY = 0f;
            cam.transform.position = Vector3.MoveTowards(cam.transform.position, targetPoint.position, moveSpeed * Time.deltaTime);
            Quaternion targetCameraRotation = Quaternion.Euler(0f, targetCameraRotationY, 0f);
            cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, targetCameraRotation, rotationSpeedCamera * Time.deltaTime);
            
            
        }
        // Check if camera is within 1 unit of pointB
            if (Vector3.Distance(cam.transform.position, pointB.position) < 1f || targetPoint == startingPoint)
            {
                targetPoint = startingPoint;
                float targetCameraRotationY = -122f;
                float targetCameraRotationX = 16f;
                Vector3 targetCameraPosition = new Vector3(6.5f, 3.5f, 3.5f);
                cam.transform.position = Vector3.MoveTowards(cam.transform.position, targetCameraPosition, moveSpeed * Time.deltaTime);

                if (Vector3.Distance(cam.transform.position, targetCameraPosition)<3.5f){

                    targetCameraRotationY = -122f;
                    targetCameraRotationX = 16f;
                    Quaternion targetCameraRotation = Quaternion.Euler(targetCameraRotationX, targetCameraRotationY, 0f);
                    cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, targetCameraRotation, rotationSpeedCamera * Time.deltaTime);
                    gameName.gameObject.SetActive(true);
                }
                else
                {
                    targetCameraRotationY = 90f;
                    targetCameraRotationX = 0f;
                    Quaternion targetCameraRotation = Quaternion.Euler(targetCameraRotationX, targetCameraRotationY, 0f);
                    cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, targetCameraRotation, rotationSpeedCamera * Time.deltaTime);
                }

                if(Vector3.Distance(cam.transform.position, targetCameraPosition)<0.01f)
                {
                    //DOOR
                    float rotationSpeed = 5f; 
                        
                    Quaternion targetRotation = Quaternion.Euler(0f, 0f, 0f);
                    door.transform.rotation = Quaternion.Lerp(door.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                    startButton.gameObject.SetActive(true);
                        goToSettingsButton.gameObject.SetActive(true);
                    //gameName.gameObject.SetActive(true);
                    if (Quaternion.Angle(door.transform.rotation, targetRotation) < 0.1f)
                    {
                        cam.GetComponent<CameraMovement>().doorOpened = false;
                        returning = false;
                        targetPoint = pointB;
                    }
                    
                    //END DOOR

                }
            }
        
        
        
    }

    
}


}