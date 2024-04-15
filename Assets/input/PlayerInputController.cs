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

    public GameEvent OnChargeAttack;

    public GameEvent OnStartChargeAttack;

    public GameEvent OnRevive;

    public GameEvent OnPause;

    public GameEvent OnSprint;

    private bool CanChargeAttack = false;

    private void Start()
    {
    }

    private void Update()
    {
        bool pressed = playerInput["ChargeAttack"].IsPressed();
        OnChargeAttack.Raise(this, pressed);
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

    public void Attack(InputAction.CallbackContext context) //Regular --> requires Tap interaction in IA
    {
        if(context.performed)
        {
            OnAttack.Raise(this, true);
        }
    }

    public void ChargeAttack(InputAction.CallbackContext context) //Charged --> requires Hold interaction in IA
    {
       
    }

    public void Pause(InputAction.CallbackContext context)
    {
       if(context.started)
        {
            OnPause.Raise(this, true);
        }
    }
    

    public void Revive(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnRevive.Raise(this, true);
        }
        else if (context.canceled)
        {
            OnRevive.Raise(this, false);
        }
    }


    public void Sprint(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnSprint.Raise(this, true);
        }
        else if (context.canceled)
        {
            OnSprint.Raise(this, false);
        }
    }
}
