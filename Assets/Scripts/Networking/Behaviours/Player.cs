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
            RPC_HandleAttack(data);

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
        else if(data.Throw)
        {
            RPC_ThrowItem();
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

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_ThrowItem()
    {
        itemHandler.InputThrowItem();
    }



    public void HandleJump(PlayerInputData data)
    {
        if (data.Jump)
        { 
            _controller.Jump();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)] //This is the only method that works up to now
    public void RPC_HandleAttack(PlayerInputData data)
    {
        RPC_Mul_HandleAttack(data);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_Mul_HandleAttack(PlayerInputData data)
    {
        if (data.Attack)
        {
            GetComponentInChildren<DamageComponent>().InputAttack();
        }
    }
}