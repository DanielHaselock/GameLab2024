using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class CustomGameEvent : UnityEvent<Component, object> { }
public class GameEventListenener : MonoBehaviour
{
    public GameEvent gameEvent;

    public CustomGameEvent Response;

    private void OnEnable()
    {
        gameEvent.RegisterListener(this);
    }

    private void OnDisable()
    {
        gameEvent.UnRegisterListener(this);
    }

    public void OnEventRaised(Component Sender, object Data)
    {
        Response.Invoke(Sender, Data);
    }
}
