using System;
using System.Collections;
using Cinemachine;
using Fusion;
using Interactables;
using Networking.Data;
using UnityEngine;
using UnityEngine.Events;

public class Player : NetworkBehaviour
{
    private NetworkCharacterController _controller;
    private NetworkMecanimAnimator _anim;
    private HandlePickup _pickupHandler;
    private DamageComponent _damager;
    private PlayerPickupable _myPickupable;
    public bool HasInputAuthority { get; private set; }
    
    private void Awake()
    {
        _controller = GetComponent<NetworkCharacterController>();
        _anim = GetComponent<NetworkMecanimAnimator>();
        _pickupHandler = GetComponentInChildren<HandlePickup>();
        _myPickupable = GetComponent<PlayerPickupable>();
    }
    
    public override void Spawned()
    {
        var no = GetComponent<NetworkObject>();
        this.name = "Player_" + no.InputAuthority.PlayerId;
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
        
        if(!_myPickupable.AllowInputs)
            return;
        
        if (GetInput(out PlayerInputData data))
        {
            HandleInteract(data);
            HandleJump(data);
            HandleAttack(data);
            _controller.Move(3 * data.MoveDirection.normalized * Runner.DeltaTime);
        }
    }

    public override void Render()
    {
        base.Render();
        if (IsProxy )
            return;

        if (!Runner.IsForward)
            return;
        
        _anim.Animator.SetFloat("Move", _controller.Velocity.normalized.magnitude);
    }


    public void HandleInteract(PlayerInputData data)
    {
        if (!HasInputAuthority)
            return;
        
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
           _pickupHandler.InputPick();
       else
        RPC_PickItemOnServer();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_PickItemOnServer()
    {
        _pickupHandler.InputPick();
    }
    
    private void DropItem()
    {
        if (Runner.IsServer)
            _pickupHandler.InputDrop();
        else
            RPC_DropItemOnServer();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_DropItemOnServer()
    {
        Debug.Log("RPC");
        _pickupHandler.InputDrop();
    }
    

    private void ThrowItem(PlayerInputData data)
    {
        if(Runner.IsServer)
            _pickupHandler.InputThrow();
        else
            RPC_ThrowItem();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_ThrowItem()
    {
        Debug.Log("RPC");
        _pickupHandler.InputThrow();
    }
    
    private void HandleJump(PlayerInputData data)
    {
        if (data.Jump)
        { 
            _controller.Jump();
            _anim.SetTrigger("Jump", true);
        }
    }

    public void HandleAttack(PlayerInputData data)
    {
        if (data.Attack && HasInputAuthority)
        {
            if (_damager == null)
                _damager = GetComponentInChildren<DamageComponent>();
            _damager.InitiateAttack();
            _anim.SetTrigger("Attack", true);
        }
    }
}