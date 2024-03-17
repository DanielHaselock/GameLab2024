using Fusion;
using Fusion.Addons.Physics;
using System.Collections.Generic;
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

    private NetworkCharacterController _parentCC;
    private NetworkObject _no;
    
    // Start is called before the first frame update
    void Start()
    {
        PickedupObjects = new List<GameObject>();
        _parentCC = GetComponentInParent<NetworkCharacterController>();
        _no = GetComponentInParent<NetworkObject>();
    }

    public void addItem(GameObject item) //Main Obj
    {
        var pickupable = item.GetComponent<PlayerPickupable>();
        if (pickupable == null)
            return;
        
        if (MaxPickupSpace - PickedupObjects.Count < pickupable.SlotNeeded)
            return;

        pickupable.PrepareForParenting(true);
        item.transform.SetParent(transform);

        PickedupObjects.Add(item);
        slotsUsed += pickupable.SlotNeeded;
        
        Vector3 pos = throwPoint.localPosition;
        pos.y += SpaceBetween2Items * PickedupObjects.Count;
        pickupable.OnParented(pos, Quaternion.identity);
    }

    public void removeItem(GameObject item, bool Throw)
    {
        PickedupObjects.Remove(item);
        var pickupable = item.GetComponent<PlayerPickupable>();
        slotsUsed -= pickupable.SlotNeeded;
        
        item.transform.SetParent(null);
        pickupable.PrepareForParenting(false);
        pickupable.Teleport(throwPoint.position, _parentCC.Velocity);

        if(Throw)
            pickupable.Throw(transform.parent.forward, throwForce);
    }
    
    public void RemoveLatestItem(bool Throw = false)
    {
        if(PickedupObjects.Count != 0)
            removeItem(PickedupObjects[PickedupObjects.Count - 1], Throw);
    }
}
