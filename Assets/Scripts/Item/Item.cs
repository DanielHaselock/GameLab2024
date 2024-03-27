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
}
