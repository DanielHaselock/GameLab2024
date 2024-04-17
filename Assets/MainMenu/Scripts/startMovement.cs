using UnityEngine;
using UnityEngine.UIElements;

public class startMovement : MonoBehaviour
{
    public Transform pointB;
    public Transform pointC;
    public bool started;
    public float moveSpeed = 0.5f;
    public GameObject door;
    private Transform targetPoint;
    public bool doorOpened;
    public Transform startingPoint;
    public GameObject returnObj; 
    returnMovement returnMove;
    public GameObject returnButton;
    public GameObject startGameButton;
    public GameObject nameinput;
    public GameObject cam;
    public GameObject fade;

    void Start()
    {
        started = false;
        targetPoint = pointB;
        doorOpened = false;
        startingPoint = cam.transform;
        returnMove = returnObj.GetComponent<returnMovement>();
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

            //CAMERA MOVEMENT
            if(doorOpened && targetPoint == pointB)
            {
                float targetCameraRotationY = -180f;
                cam.transform.position = Vector3.MoveTowards(cam.transform.position, targetPoint.position, moveSpeed * Time.deltaTime);
                Quaternion targetCameraRotation = Quaternion.Euler(0f, targetCameraRotationY, 0f);
                cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, targetCameraRotation, rotationSpeedCamera * Time.deltaTime);
            }
            
            // Check if camera is within 1 unit of pointB
            if (Vector3.Distance(cam.transform.position, pointB.position) < 1f || targetPoint == pointC && !returnMove.returning == true)
            {
                rotationSpeedCamera = 4f; 
                targetPoint = pointC;
                float targetCameraRotationY = -180f;
                Quaternion targetCameraRotation = Quaternion.Euler(0f, targetCameraRotationY, 0f);
                cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, targetCameraRotation, rotationSpeedCamera * Time.deltaTime);
                cam.transform.position = Vector3.MoveTowards(cam.transform.position, targetPoint.position, moveSpeed * Time.deltaTime);

                if (Vector3.Distance(cam.transform.position, pointC.position) < 0.1f )
                {
                    started=false;
                    targetPoint = pointB;
                    returnButton.gameObject.SetActive(true);
                    startGameButton.SetActive(true);
                    nameinput.SetActive(true);
                    fade.SetActive(false);
                    rotationSpeedCamera =1f;
                }
            }  
        }
    }
}