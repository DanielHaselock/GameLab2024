using System;
using Fusion;
using System.Collections.Generic;
using Audio;
using Interactables;
using UnityEngine;

public class HandlePickup : NetworkBehaviour
{
    [SerializeField] private NetworkMecanimAnimator _anim;
    [SerializeField] private float _detectionRadius = 2;
    private List<GameObject> ItemsAvailable;
    PickupManager _pickupManager;
    private PlayerPickupable _mine;

    void Start()
    {
        _mine = GetComponentInParent<PlayerPickupable>();
        ItemsAvailable = new List<GameObject>();
        _pickupManager = GetComponent<PickupManager>();
    }

    private List<GameObject> GetPickableInProximity()
    {
        var list = new List<GameObject>();
        var allCOll = Physics.OverlapSphere(transform.position, _detectionRadius);
        foreach (var col in allCOll)
        {
            var pickable = col.gameObject.GetComponent<PlayerPickupable>();
            if (!pickable)
                continue;
            if(pickable == _mine)
                continue;
            
            if(pickable.IsPickedUp)
                continue;
            
            list.Add(pickable.gameObject);
        }

        return list;
    }

    public void InputPick()
    {
        Debug.Log("Pickup");
        if(_mine.IsPickedUp)
            return;
        
        ItemsAvailable = GetPickableInProximity();
        Debug.Log(ItemsAvailable.Count);
        if (ItemsAvailable.Count == 0)
            return;

        int i = 0;
        for (; i < ItemsAvailable.Count; ++i)
        {
            if (ItemsAvailable[i].transform.parent == null)
                break;

            if (i == ItemsAvailable.Count - 1)
                break;
        }

        Debug.Log(ItemsAvailable[i]);
        if (!_pickupManager.IsFull && ItemsAvailable[i])
        {
            _pickupManager.addItem(ItemsAvailable[i], () =>
            {
                RPC_UpdateAnim(true);
                RPC_PlaySFX(SFXConstants.Pickup);
            });
            ItemsAvailable.RemoveAt(i);
        }
    }
   
    public void InputDrop()
    {
        _pickupManager.RemoveLatestItem(false,() =>
        {
            RPC_UpdateAnim(false);
            RPC_PlaySFX(SFXConstants.Drop);
        });
    }

    public void InputThrow()
    {
        _pickupManager.RemoveLatestItem(true, () =>
        {
            RPC_UpdateAnim(false);
            RPC_PlaySFX(SFXConstants.Throw);
        });
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpdateAnim(bool pickedup)
    {
        _anim.Animator.SetBool("HasPickedItem", pickedup);
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    private void RPC_PlaySFX(string key)
    {
        AudioManager.Instance.PlaySFX(key);
    }
    
    private void OnDrawGizmos()
    {
        var col = Color.red;
        col.a = 0.15f;
        Gizmos.color = col;
        Gizmos.DrawSphere(transform.position, _detectionRadius);
    }
}
