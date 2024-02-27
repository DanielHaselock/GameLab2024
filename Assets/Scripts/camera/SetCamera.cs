using Cinemachine;
using Networking.Behaviours;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCamera : MonoBehaviour
{
    [SerializeField] private GameObject _camera;
    private CinemachineVirtualCamera _virtualcamera;

    public void SetCameraParams(GameObject obj)
    {
        _camera = GameObject.Find("Virtual Camera");
        _virtualcamera = _camera.GetComponent<CinemachineVirtualCamera>();
        _virtualcamera.Follow = obj.transform;
    }
}
