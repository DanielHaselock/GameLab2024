using UnityEngine;
using TMPro;
public class leaveCredits : MonoBehaviour
{
    public bool leavingCreditsBool;
    public float moveSpeed = 5f;
    public GameObject cam;
    public GameObject gameName;
    public GameObject settingsButton;
    public GameObject visitCreditsButton;
    public GameObject startButton;
    private Transform targetPoint;
    private Transform startingPoint;
    public GameObject startingPosition;
    public GameObject fade;
    void Start()
    {
        leavingCreditsBool = false; 
    }

    void Update()
        {
        startingPoint = startingPosition.GetComponent<startMovement>().startingPoint;
        if (leavingCreditsBool)
        {
            //CAMERA MOVEMENT
            float rotationSpeedCamera = 5f;
            Vector3 targetCameraPosition = new Vector3(6.5f, 3.5f, 3.5f);
            cam.transform.position = Vector3.MoveTowards(cam.transform.position, targetCameraPosition, moveSpeed * Time.deltaTime);
                
            // Check if camera is within 1 unit of pointB
            if (Vector3.Distance(cam.transform.position, startingPoint.position) < 1f || targetPoint == startingPoint)
            {
                targetPoint = startingPoint;
                float targetCameraRotationY = -122f;
                float targetCameraRotationX = 16f;
                Quaternion targetCameraRotation = Quaternion.Euler(targetCameraRotationX, targetCameraRotationY, 0f);
                cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, targetCameraRotation, rotationSpeedCamera * Time.deltaTime);

                if(Vector3.Distance(cam.transform.position, targetCameraPosition)<0.01f)
                {
                    startButton.gameObject.SetActive(true);
                    settingsButton.gameObject.SetActive(true);
                    visitCreditsButton.gameObject.SetActive(true);
                    gameName.gameObject.SetActive(true);
                    leavingCreditsBool=false;
                    fade.SetActive(false);
                }
            }
        }
    }
}