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
    public GameObject creditsButton;
    public GameObject door;
    public GameObject startButton;
    private Transform targetPoint;
    private Transform startingPoint;
    public GameObject goToSettingsButton;

    public  Quaternion targetCameraRotation;
    public bool closeDoor;

    void Start()
    {
        returning = false;
        targetPoint = pointB;
        closeDoor=false;
    }

    void Update()
    {
        startButton.GetComponent<startClicked>().closeDoor = closeDoor;
        if (closeDoor)
        {
            float rotationSpeed = 5f;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, 0f);
            if (Quaternion.Angle(door.transform.rotation, targetRotation) < 0.1f)
            {
                cam.GetComponent<CameraMovement>().doorOpened = false;
                targetPoint = pointB;
                closeDoor=false;
            }
            door.transform.rotation = Quaternion.Lerp(door.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        startingPoint = cam.GetComponent<CameraMovement>().startingPoint;
        float rotationSpeedCamera = 5f;

        if (returning)
        {
            gameName.gameObject.SetActive(false);

            //CAMERA MOVEMENT
            if (targetPoint == pointB)
            {
                float targetCameraRotationY = 0f;
                cam.transform.position = Vector3.MoveTowards(cam.transform.position, targetPoint.position, moveSpeed * Time.deltaTime);
                Quaternion targetCameraRotation = Quaternion.Euler(0f, targetCameraRotationY, 0f);
                cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, targetCameraRotation, rotationSpeedCamera * Time.deltaTime);
            }

            if (Vector3.Distance(cam.transform.position, pointB.position) < 1f || targetPoint == startingPoint)
            {
                targetPoint = startingPoint;
                float targetCameraRotationY;
                float targetCameraRotationX;
                Vector3 targetCameraPosition = new Vector3(6.5f, 3.5f, 3.5f);
                cam.transform.position = Vector3.MoveTowards(cam.transform.position, targetCameraPosition, moveSpeed * Time.deltaTime);

                if (Vector3.Distance(cam.transform.position, targetCameraPosition) < 3.5f)
                {
                    targetCameraRotationY = -122f;
                    targetCameraRotationX = 16f;
                    targetCameraRotation = Quaternion.Euler(targetCameraRotationX, targetCameraRotationY, 0f);
                    cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, targetCameraRotation, rotationSpeedCamera * Time.deltaTime);
                    gameName.gameObject.SetActive(true);
                }
                else
                {
                    targetCameraRotationY = 90f;
                    targetCameraRotationX = 0f;
                    targetCameraRotation = Quaternion.Euler(targetCameraRotationX, targetCameraRotationY, 0f);
                    cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, targetCameraRotation, rotationSpeedCamera * Time.deltaTime);
                }

                if (Vector3.Distance(cam.transform.position, targetCameraPosition) ==0 && Quaternion.Angle(cam.transform.rotation, targetCameraRotation)<0.2f)
                {
                    returning=false;
                    closeDoor=true;
                    startButton.gameObject.SetActive(true);
                    goToSettingsButton.gameObject.SetActive(true);
                    creditsButton.gameObject.SetActive(true);
                }
            }
        }

    }
}
