using Fusion;
using Fusion.Addons.Physics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Inventory : MonoBehaviour
{
    public List<GameObject> Items;

    [SerializeField] private int MaxItemNumb = 5;

    [HideInInspector] public bool IsFull => Items.Count == MaxItemNumb;

    public float SpaceBetween2Items = 0.5f;

    public float ThrowForce;

    // Start is called before the first frame update
    void Start()
    {
        Items = new List<GameObject>();
    }

    public void addItem(GameObject item) //Main Obj
    {
        Item itemClass = item.GetComponent<Item>();
        GameObject LocalObj = itemClass.GetLocalGameobject();
        NetworkObject NetObj = itemClass.GetNetworkGameobject();
        LocalObj.SetActive(true);
        LocalObj.transform.SetParent(transform);
        NetObj.GetComponent<NetworkRigidbody3D>().Teleport(new Vector3(1000, 0, 1000));
        NetObj.GetComponent<Rigidbody>().isKinematic = true;

        Items.Add(item);
        Vector3 pos = Vector3.zero;
        pos.y += SpaceBetween2Items * Items.Count;
        LocalObj.gameObject.transform.localPosition = pos;
    }

    public void removeItem(GameObject item, bool Throw) 
    {
        Item itemClass = item.GetComponent<Item>();
        GameObject LocalObj = itemClass.GetLocalGameobject();
        NetworkObject NetObj = itemClass.GetNetworkGameobject();
        LocalObj.transform.SetParent(item.transform);
        LocalObj.SetActive(false);
        NetObj.GetComponent<NetworkRigidbody3D>().Teleport(transform.position);
        NetObj.GetComponent<Rigidbody>().isKinematic = false;
        if(Throw)
        {
            NetObj.GetComponent<Rigidbody>().AddForce(transform.forward * ThrowForce);
        }


        Items.Remove(item);
    }

    public void RemoveLatestItem(bool Throw = false)
    {
        if(Items.Count != 0)
            removeItem(Items[Items.Count - 1], Throw);
    }



}
