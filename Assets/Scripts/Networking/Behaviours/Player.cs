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
            PickItem();
        }
        else if (data.Drop)
        {
            DropItem();
        }
        else if(data.Throw)
        {
            ThrowItem(data);
        }
    }
    
    public void PickItem()
    {
       if(Runner.IsServer)
           itemHandler.InputPickItem();
       else
        RPC_PickItemOnServer();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_PickItemOnServer()
    {
        itemHandler.InputPickItem();
    }
    
    private void DropItem()
    {
        itemHandler.InputDropItem();
    }


    private void ThrowItem(PlayerInputData data)
    {
        if(Runner.IsServer)
            itemHandler.InputThrowItem();
        else
            RPC_ThrowItem();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_ThrowItem()
    {
        itemHandler.InputThrowItem();
    }
    
    private void HandleJump(PlayerInputData data)
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