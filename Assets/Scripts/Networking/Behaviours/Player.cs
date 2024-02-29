using System;
using Cinemachine;
using Fusion;
using Networking.Data;
using UnityEngine;
using UnityEngine.Events;

public class Player : NetworkBehaviour
{
    private NetworkCharacterController _controller;

    private HandleItem itemHandler;
    private DamageComponent _damager;
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
            HandleInteract(data);
            HandleJump(data);
            HandleAttack(data);
            _controller.Move(3 * data.MoveDirection.normalized * Runner.DeltaTime);
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
            ThrowItem(data);
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


    public void ThrowItem(PlayerInputData data)
    {
        if(data.Throw && HasInputAuthority)
            RPC_ThrowItem();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_ThrowItem()
    {
        RPC_Mul_ThrowItem();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_Mul_ThrowItem()
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

    public void HandleAttack(PlayerInputData data)
    {
        if (data.Attack && HasInputAuthority)
        {
            if (_damager == null)
                _damager = GetComponentInChildren<DamageComponent>();
            _damager.InitiateAttack();
        }
    }
}