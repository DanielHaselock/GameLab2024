using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExitGames.Client.Photon.StructWrapping;

public class creditMovement : MonoBehaviour
{
    public Transform creditPosition;
    //public Transform pointC;
    public bool creditsClicked;
    public float moveSpeed = 5f;
    public GameObject cam;
    private Transform targetPoint;
    private Transform startingPoint;

    void Start()
    {
        creditsClicked = false;
        targetPoint = creditPosition;
    }

    void Update()
{
    startingPoint = cam.GetComponent<CameraMovement>().startingPoint;
    float rotationSpeedCamera = 5f;
    if (creditsClicked)
    {

        //CAMERA MOVEMENT
        if(targetPoint == creditPosition)
        {
            
            float targetCameraRotationY = 0f;
            cam.transform.position = Vector3.MoveTowards(cam.transform.position, targetPoint.position, moveSpeed * Time.deltaTime);
            Quaternion targetCameraRotation = Quaternion.Euler(29f, -111f, 0.6f);
            cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, targetCameraRotation, rotationSpeedCamera * Time.deltaTime);
            
            
        }
        // Check if camera is within 1 unit of pointB
            if (Vector3.Distance(cam.transform.position, creditPosition.position) < 1f )
            {
                creditsClicked = false;
            }
        
        
        
    }

    
}


}