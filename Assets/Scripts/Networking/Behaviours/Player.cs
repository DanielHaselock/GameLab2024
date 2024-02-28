using System;
using Fusion;
using Networking.Data;
using UnityEngine;
using UnityEngine.Events;

public class Player : NetworkBehaviour
{
    private NetworkCharacterController _controller;

    private HandleItem itemHandler;
    public bool HasInputAuthority { get; private set; }

    private void Awake()
    {
        _controller = GetComponent<NetworkCharacterController>();
        itemHandler = transform.Find("ItemSlot").GetComponent<HandleItem>();
    }


    public override void Spawned()
    {
        var no = GetComponent<NetworkObject>();
        if (no.InputAuthority == Runner.LocalPlayer)
        {
            HasInputAuthority = true;
            GetComponent<SetCamera>().SetCameraParams(gameObject.transform.GetChild(1).gameObject);
            GetComponent<PlayerInputController>().OnSpawned();
        }
    }
    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        if (GetInput(out PlayerInputData data))
        {
            data.MoveDirection.Normalize();
            _controller.Move(3 * data.MoveDirection * Runner.DeltaTime);
            HandleInteract(data);
        }
    }

    public void HandleInteract(PlayerInputData data)
    {
        if (data.Interact)
        {
            itemHandler.InputPickItem();
        }
        else if(data.Drop)
        {
            itemHandler.InputDropItem();
        }
    }
}