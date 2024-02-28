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
            HandleInteract(data);
            HandleJump(data);

            _controller.Move(3 * (data.MoveDirection) * Runner.DeltaTime);
            
        }
    }


    public void HandleInteract(PlayerInputData data)
    {
        if (data.Interact)
        {
            RPC_PickItem();
        }
        else if (data.Drop)
        {
            RPC_DropItem();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_PickItem()
    {
        itemHandler.InputPickItem();
    }
    
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_DropItem()
    {
        itemHandler.InputDropItem();
    }



    public void HandleJump(PlayerInputData data)
    {
        if (data.Jump)
        { 
            _controller.Jump();
        }
    }
}