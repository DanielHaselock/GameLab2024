using Cinemachine;
using Networking.Behaviours;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCamera : MonoBehaviour
{
    [SerializeField] private GameObject _camera;
    private CinemachineVirtualCamera _virtualcamera;
    void Start()
    {
    }

    public void SetCameraParams(GameObject obj)
    {
        _camera = GameObject.Find("Virtual Camera");
        _virtualcamera = _camera.GetComponent<CinemachineVirtualCamera>();
        _virtualcamera.LookAt = obj.transform;
        _virtualcamera.Follow = obj.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
