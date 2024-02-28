using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "GameEvent")]
public class GameEvent : ScriptableObject
{
    public List<GameEventListenener> listeners = new List<GameEventListenener>();

    public void Raise(Component Sender, object Data)
    {
        foreach (var listener in listeners)
        {
            listener.OnEventRaised(Sender, Data);
        }
    }

    public void RegisterListener(GameEventListenener newlistener)
    {
        if(!listeners.Contains(newlistener))
            listeners.Add(newlistener);
    }

    public void UnRegisterListener(GameEventListenener newlistener)
    {
        if (listeners.Contains(newlistener))
            listeners.Remove(newlistener);
    }
}
