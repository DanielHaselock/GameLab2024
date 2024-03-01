using Fusion;
using Fusion.Addons.Physics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Inventory : NetworkBehaviour
{
    [SerializeField] private Transform throwPoint;
    [SerializeField] private int MaxItemNumb = 5;
    [SerializeField] private float throwForce=10;
    private List<GameObject> Items;
    public bool IsFull => Items.Count == MaxItemNumb;
    public float SpaceBetween2Items = 0.5f;

    private List<GameObject> itemsToThrow;
    private List<GameObject> itemsToDrop;

    // Start is called before the first frame update
    void Start()
    {
        Items = new List<GameObject>();
    }

    public void addItem(GameObject item) //Main Obj
    {
        Item itemClass = item.GetComponent<Item>();
        item.transform.SetParent(transform);
        item.GetComponent<Rigidbody>().isKinematic = true;

        Items.Add(item);
        Vector3 pos = Vector3.zero;
        pos.y += SpaceBetween2Items * Items.Count;
        item.gameObject.transform.localPosition = pos;
    }

    public void removeItem(GameObject item, bool Throw)
    {
        Debug.Log($"Kya re {Throw}");
        if (itemsToThrow == null)
            itemsToThrow = new List<GameObject>();
        
        if (itemsToDrop == null)
            itemsToDrop = new List<GameObject>();
        
        if(Throw)
            itemsToThrow.Add(item);
        else
            itemsToDrop.Add(item);
        
        Items.Remove(item);
    }

    public void RemoveLatestItem(bool Throw = false)
    {
        if(Items.Count != 0)
            removeItem(Items[Items.Count - 1], Throw);
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        HandleThrow();
        HandleDrop();
    }

    private void HandleThrow()
    {
        if (itemsToThrow == null || itemsToThrow.Count <= 0)
            return;
        var clones = new List<GameObject>(itemsToThrow);
        itemsToThrow.Clear();
        foreach (var item in clones)
        {
            Item itemClass = item.GetComponent<Item>();
            var nrb = item.GetComponent<NetworkRigidbody3D>();
            var rb = item.GetComponent<Rigidbody>();
            item.transform.SetParent(null);
            nrb.Teleport(throwPoint.position);
            nrb.RBIsKinematic = false;
            nrb.ResetRigidbody();
            rb.AddForce(transform.parent.forward * throwForce, ForceMode.Impulse);
        }
    }
    private void HandleDrop()
    {
        if (itemsToDrop == null || itemsToDrop.Count <= 0)
            return;
        var clones = new List<GameObject>(itemsToDrop);
        itemsToDrop.Clear();
        foreach (var item in clones)
        {
            Item itemClass = item.GetComponent<Item>();
            var nrb = item.GetComponent<NetworkRigidbody3D>();
            item.transform.SetParent(null);
            nrb.Teleport(throwPoint.position);
            nrb.RBIsKinematic = false;
        }
    }
}
