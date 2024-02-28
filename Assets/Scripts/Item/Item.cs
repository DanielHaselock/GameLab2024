using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ItemClass
{
    Tomato,
    Cheeze,
    blabla
}

public class Item : MonoBehaviour
{
    [SerializeField] private ItemClass itemClass;

    [Header("ListOfLocalGameobjects")]
    public GameObject LocalTomatoObj;

    public GameObject LocalCheezeObj;


    [Header("ListOfNetworkedGameObjects")]
    public NetworkObject TomatoObj;

    public NetworkObject CheezeObj;

    public GameObject GetLocalGameobject()
    {
        switch(itemClass)
        {
            case ItemClass.Tomato:
                return LocalTomatoObj;

            case ItemClass.Cheeze:
                return LocalCheezeObj;

            default: 
                return null;
        }
    }


    public NetworkObject GetNetworkGameobject()
    {
        switch (itemClass)
        {
            case ItemClass.Tomato:
                return TomatoObj;

            case ItemClass.Cheeze:
                return CheezeObj;

            default:
                return null;
        }
    }
}
