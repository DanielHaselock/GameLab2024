using System;
using Fusion;
using Fusion.Addons.Physics;
using System.Collections.Generic;
using Fusion.Addons.SimpleKCC;
using Interactables;
using UnityEngine;

public class PickupManager : MonoBehaviour
{
    [SerializeField] private Transform throwPoint;
    [SerializeField] private int MaxPickupSpace = 5;
    [SerializeField] private float throwForce=10;
    private List<GameObject> PickedupObjects;
    private int slotsUsed=0;
    public bool IsFull => PickedupObjects.Count == MaxPickupSpace;
    public float SpaceBetween2Items = 0.5f;

    private SimpleKCC _parentCC;
    private NetworkObject _no;
    
    // Start is called before the first frame update
    void Start()
    {
        PickedupObjects = new List<GameObject>();
        _parentCC = GetComponentInParent<SimpleKCC>();
        _no = GetComponentInParent<NetworkObject>();
    }

    public void addItem(GameObject item, Action OnAdd=null) //Main Obj
    {
        var pickupable = item.GetComponent<PlayerPickupable>();
        if (pickupable == null)
            return;
        if(pickupable.IsPickedUp)
            return;
        if (MaxPickupSpace - PickedupObjects.Count < pickupable.SlotNeeded)
            return;
        
        pickupable.PrepareForParenting(true);
        item.transform.SetParent(transform);

        var posDeltaY = GetPositionDeltaY(item);
        PickedupObjects.Add(item);
        slotsUsed += pickupable.SlotNeeded;
        
        Vector3 pos = throwPoint.localPosition;
        pos.y += posDeltaY;
        pickupable.OnParented(pos, Quaternion.identity);
        OnAdd?.Invoke();
    }

    private float GetPositionDeltaY(GameObject obj)
    {
        float totalY = 0;
        foreach (var item in PickedupObjects)
        {
            var rnd = GetRenderer(item);
            if(rnd == null)
                continue;

            var bounds = rnd.bounds;
            totalY += bounds.size.y;
        }

        var currentRndr = GetRenderer(obj);
        if (currentRndr == null)
            return totalY + SpaceBetween2Items;
        else
        {
            return totalY + currentRndr.bounds.size.y / 2;
        }
    }

    private Renderer GetRenderer(GameObject go)
    {
        Renderer mesh = go.GetComponentInChildren<MeshRenderer>();
        if (mesh == null)
            mesh = go.GetComponentInChildren<SkinnedMeshRenderer>();

        return mesh;
    }
    
    public void removeItem(GameObject item, bool Throw)
    {
        if(item == null)
            return;
        PickedupObjects.Remove(item);
        var pickupable = item.GetComponent<PlayerPickupable>();
        slotsUsed -= pickupable.SlotNeeded;
        
        item.transform.SetParent(null);
        pickupable.PrepareForParenting(false);
        pickupable.Teleport(throwPoint.position, _parentCC.RealVelocity);

        if(Throw)
            pickupable.Throw(transform.parent.forward, throwForce);
    }
    
    public void RemoveLatestItem(bool Throw = false, Action OnRemoved=null)
    {
        if (PickedupObjects.Count != 0)
        {
            OnRemoved?.Invoke();
            removeItem(PickedupObjects[PickedupObjects.Count - 1], Throw);
        }
    }
}
