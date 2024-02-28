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

    public delegate void InteractDelegate();

    private InteractDelegate interactDelegate;

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
}