using Fusion;
using Fusion.Addons.Physics;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
public class Inventory : MonoBehaviour
{
    [SerializeField] private Transform throwPoint;
    [SerializeField] private int MaxItemNumb = 5;
    [SerializeField] private float throwForce=10;
    private List<GameObject> Items;
    public bool IsFull => Items.Count == MaxItemNumb;
    public float SpaceBetween2Items = 0.5f;

    private NetworkCharacterController _parentCC;
    
    // Start is called before the first frame update
    void Start()
    {
        Items = new List<GameObject>();
        _parentCC = GetComponentInParent<NetworkCharacterController>();
    }

    public void addItem(GameObject item) //Main Obj
    {
        Item itemClass = item.GetComponent<Item>();
        item.transform.SetParent(transform);
        item.GetComponent<Rigidbody>().isKinematic = true;
        item.GetComponent<Collider>().enabled = false;

        Items.Add(item);
        Vector3 pos = throwPoint.localPosition;
        pos.y += SpaceBetween2Items * Items.Count;
        item.transform.localPosition = pos;
        item.transform.localRotation = Quaternion.identity;
    }

    public void removeItem(GameObject item, bool Throw)
    {
        Items.Remove(item);
        var nrb = item.GetComponent<NetworkRigidbody3D>();
        var rb = item.GetComponent<Rigidbody>();
        item.transform.SetParent(null);
        nrb.Teleport(throwPoint.position);
        item.GetComponent<Collider>().enabled = true;
        rb.isKinematic = false;
        nrb.ResetRigidbody();
        rb.velocity = _parentCC.Velocity;
        
        if(Throw)
            rb.AddForce(transform.parent.forward * throwForce, ForceMode.Impulse);
    }
    
    public void RemoveLatestItem(bool Throw = false)
    {
        if(Items.Count != 0)
            removeItem(Items[Items.Count - 1], Throw);
    }
}
