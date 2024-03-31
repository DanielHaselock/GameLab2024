using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaiseObjective : MonoBehaviour
{
    [Serializable]
    public enum InvokeType { OnSpawn, OnDestroy, Manual};
    [SerializeField] private InvokeType invokeType;
    [SerializeField] private string key;

    private void Start()
    {
        if (invokeType == InvokeType.OnSpawn)
        {
            GameManager.instance.RaiseObjective(key);
        }
    }
    private void OnDestroy()
    {
        if (invokeType == InvokeType.OnDestroy)
        {
            GameManager.instance.RaiseObjective(key);
        }
    }
    private void Raise()
    {
            GameManager.instance.RaiseObjective(key);
     
    }


}
