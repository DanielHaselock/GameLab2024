using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ItemClass
{
    Tomato,
    Feta,
    Bread,
    Hotdog,
    Herring,
    Onion,
    Pepper,
    Lettuce,
    Mustard,
    blabla
}

public class Item : MonoBehaviour
{
    [SerializeField] private ItemClass itemClass;
}
