using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    [SerializeField] private InputActionAsset playerInput;

    [Header("GameEvents")]
    public GameEvent OnMoved;

    public GameEvent OnInteract;

    public GameEvent OnDrop;

    private void Start()
    {
    }
    public void OnSpawned()
    {
        playerInput.Enable();
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 Direction = context.ReadValue<Vector2>();
            Vector3 Movement = new Vector3(Direction.x, 0, Direction.y);
            OnMoved.Raise(this, Movement);
        }
        else
        {
            OnMoved.Raise(this, Vector3.zero);
        }
    }


    public void Interact(InputAction.CallbackContext context)
    {
        if (context.started)
        { 
            OnInteract.Raise(this, true);
        }
        //else
        //{
        //    OnInteract.Raise(this, false);
        //}
    }

    public void DropInteract(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnDrop.Raise(this, true);
        }
        //else
        //{
        //    OnDrop.Raise(this, false);
        //}
    }
}
