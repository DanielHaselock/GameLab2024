using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HandleItem : NetworkBehaviour
{
    private List<GameObject> ItemsAvailable;

    [SerializeField] private string tag;

    Inventory inventory;

    void Start()
    {
        ItemsAvailable = new List<GameObject>();
        inventory = GetComponent<Inventory>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == tag)
        {
            ItemsAvailable.Add(other.gameObject);
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == tag)
        {
            ItemsAvailable.Remove(other.gameObject);
        }
        
    }
    public void InputPickItem()
    {
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

        if (!inventory.IsFull && ItemsAvailable[i])
        {
            inventory.addItem(ItemsAvailable[i]);
        }
    }
   
    public void InputDropItem()
    {
        inventory.RemoveLatestItem();
    }

    public void InputThrowItem()
    {
        inventory.RemoveLatestItem(true);
    }
}
