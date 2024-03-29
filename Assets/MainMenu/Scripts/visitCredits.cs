using UnityEngine;

public class visitCredits : MonoBehaviour
{
    public Transform creditPosition;
    public bool creditsClicked;
    public float moveSpeed = 5f;
    public GameObject cam;
    private Transform targetPoint;
    public GameObject creditsLeaveButton;
    void Start()
    {
        creditsClicked = false;
        targetPoint = creditPosition;
    }

    void Update()
    {
        float rotationSpeedCamera = 3f;
        if (creditsClicked)
        {
            //CAMERA MOVEMENT
            if(targetPoint == creditPosition)
            {
                cam.transform.position = Vector3.MoveTowards(cam.transform.position, targetPoint.position, moveSpeed * Time.deltaTime);
                Quaternion targetCameraRotation = Quaternion.Euler(0f, -180f, 0f);
                cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, targetCameraRotation, rotationSpeedCamera * Time.deltaTime);
            }
            // Check if camera is within 1 unit of pointB
            if (Vector3.Distance(cam.transform.position, creditPosition.position) < 0.1f )
            {
                creditsClicked = false;
                creditsLeaveButton.gameObject.SetActive(true);
            }  
        }  
    }
}