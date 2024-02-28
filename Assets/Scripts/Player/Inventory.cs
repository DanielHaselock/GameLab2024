using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<GameObject> Items;

    [SerializeField] private int MaxItemNumb = 5;

    [HideInInspector] public bool IsFull => Items.Count == MaxItemNumb;

    public float SpaceBetween2Items = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        Items = new List<GameObject>();
    }

    public void addItem(GameObject item)
    {
        Items.Add(item);
        item.gameObject.transform.SetParent(transform, true);
        item.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        Vector3 pos = Vector3.zero;
        pos.y += SpaceBetween2Items * Items.Count; 
        item.gameObject.transform.localPosition = pos;
    }

    public void removeItem(GameObject item) 
    {
        Items.Remove(item);
        item.gameObject.transform.SetParent(null, true);
        item.gameObject.GetComponent<Rigidbody>().isKinematic = false;
    }

    public void RemoveLatestItem()
    {
        if(Items.Count != 0)
            removeItem(Items[Items.Count - 1]);
    }
}
