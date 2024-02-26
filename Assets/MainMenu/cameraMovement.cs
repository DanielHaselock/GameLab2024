using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CameraMovement : MonoBehaviour
{
    public Transform pointB;
    public Transform pointC;
    public bool started;
    public float moveSpeed = 0.5f;
    public TMP_Text gameName;
    public GameObject door;

    private Transform targetPoint;
    private bool doorOpened;

    void Start()
    {
        started = false;
        targetPoint = pointB;
        doorOpened = false;
    }

    void Update()
{
    if (started)
    {
        //DOOR
        float targetRotationY = 100f;
        float rotationSpeed = 5f; 
        float rotationSpeedCamera = 1f;

        Quaternion targetRotation = Quaternion.Euler(0f, targetRotationY, 0f);
        door.transform.rotation = Quaternion.Lerp(door.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (Quaternion.Angle(door.transform.rotation, targetRotation) < 1f)
        {
            doorOpened = true;
        }
        //END DOOR

        gameName.gameObject.SetActive(false);

        //CAMERA MOVEMENT
        if(doorOpened && targetPoint == pointB)
        {
            float targetCameraRotationY = -180f;
            transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.deltaTime);
            Quaternion targetCameraRotation = Quaternion.Euler(0f, targetCameraRotationY, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetCameraRotation, rotationSpeedCamera * Time.deltaTime);
            
        }
        // Check if camera is within 1 unit of pointB
            if (Vector3.Distance(transform.position, pointB.position) < 1f || targetPoint == pointC)
            {

                targetPoint = pointC;
                float targetCameraRotationY = -170f;
                float targetCameraRotationX = -5f;
                transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.deltaTime);
                Quaternion targetCameraRotation = Quaternion.Euler(targetCameraRotationX, targetCameraRotationY, 0f);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetCameraRotation, rotationSpeedCamera * Time.deltaTime);
            }
        
        
        
    }

    
}


}