using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputActionAsset playerInput;

    [Header("GameEvents")]
    public GameEvent OnMoved;

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
    }
}
