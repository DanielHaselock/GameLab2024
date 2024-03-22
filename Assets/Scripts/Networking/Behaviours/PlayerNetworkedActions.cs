using System;
using Fusion;
using Networking.Data;
using System.Collections;
using System.Collections.Generic;
using Interactables;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerNetworkedActions : MonoBehaviour
{
    private Camera _camera;
    private PlayerInputData InputData = new PlayerInputData();
    private Vector3 moveInput;
    private void Start()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if(!_camera)
            return;
        
        //keep updating the move
        var input = moveInput;
        Vector3 Forward = _camera.transform.forward;
        Vector3 Right = _camera.transform.right;
        Vector3 forwardRelative = input.z * Forward;
        Vector3 rightRelative = input.x * Right;
        Vector3 MoveDir = forwardRelative + rightRelative;
        MoveDir.y = 0;
        InputData.MoveDirection = MoveDir;
    }

    public void BroadcastMove(Component Sender, object data)
    {
       

        moveInput = (Vector3)data;
    }
    public void BroadcastInteract(Component Sender, object data)
    {
        InputData.Interact = (bool)data;
    }
    public void BroadcastDrop(Component Sender, object data)
    {
        InputData.Drop = (bool)data;
    }

    public void BroadcastJump(Component Sender, object data)
    {
        InputData.Jump = (bool)data;
    }

    public void BroadcastThrow(Component Sender, object data)
    {
        InputData.Throw = (bool)data;
    }

    public void BroadcastAttack(Component Sender, object data)
    {
        InputData.Attack = (bool)data;
    }

    public void BroadcastPause(Component Sender, object data)
    {
        GameManager.instance.Pause(!GameManager.instance.IsPaused);
    }

    public void RefreshInputData()
    {
        InputData.RefreshInputs();
    }
    public PlayerInputData GetInputData() 
    {
        return InputData; 
    }
}