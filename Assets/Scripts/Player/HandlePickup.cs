using System;
using Fusion;
using System.Collections.Generic;
using Interactables;
using UnityEngine;

public class HandlePickup : NetworkBehaviour
{
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
        ItemsAvailable = GetPickableInProximity();
        if (ItemsAvailable.Count == 0)
            return;

        int i = 0;
        for (; i < ItemsAvailable.Count; ++i)
        {
            if (ItemsAvailable[i].transform.parent == null)
                break;

            if (i == ItemsAvailable.Count - 1)
                return;
        }

        if (!_pickupManager.IsFull && ItemsAvailable[i])
        {
            _pickupManager.addItem(ItemsAvailable[i]);
            ItemsAvailable.RemoveAt(i);
        }
    }
   
    public void InputDrop()
    {
        _pickupManager.RemoveLatestItem();
    }

    public void InputThrow()
    {
        _pickupManager.RemoveLatestItem(true);
    }

    private void OnDrawGizmos()
    {
        var col = Color.red;
        col.a = 0.15f;
        Gizmos.color = col;
        Gizmos.DrawSphere(transform.position, _detectionRadius);
    }
}
