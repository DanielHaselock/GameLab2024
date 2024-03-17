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

    public GameEvent OnThrow;

    public GameEvent OnJump;

    public GameEvent OnAttack;

    public GameEvent OnPause;

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
    }

    public void DropInteract(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnDrop.Raise(this, true);
        }
    }

    public void Throw(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnThrow.Raise(this, true);
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnJump.Raise(this, true);
        }
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnAttack.Raise(this, true);
        }
    }

    public void Pause(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnPause.Raise(this, true);
        }
    }
}
