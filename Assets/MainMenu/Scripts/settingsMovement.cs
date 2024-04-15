using UnityEngine;

public class settingsMovement : MonoBehaviour
{
    public Transform settingsPosition;
    public bool settingsClicked;
    public float moveSpeed = 5f;
    public GameObject cam;
    private Transform targetPoint;
    public GameObject settingsLeaveButton;
    public GameObject fade;
    void Start()
    {
        settingsClicked = false;
        targetPoint = settingsPosition;
    }

    void Update()
    {
        float rotationSpeedCamera = 5f;
        if (settingsClicked)
        {
            //CAMERA MOVEMENT
            if(targetPoint == settingsPosition)
            {
                cam.transform.position = Vector3.MoveTowards(cam.transform.position, targetPoint.position, moveSpeed * Time.deltaTime);
                Quaternion targetCameraRotation = Quaternion.Euler(29f, -111f, 0.6f);
                cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, targetCameraRotation, rotationSpeedCamera * Time.deltaTime);
            }
            // Check if camera is within 1 unit of pointB
            if (Vector3.Distance(cam.transform.position, settingsPosition.position) < 1f )
            {
                settingsClicked = false;
                settingsLeaveButton.gameObject.SetActive(true);
                fade.SetActive(false);
            }
        }
    }
}