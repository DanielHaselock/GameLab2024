using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLoop;
using Fusion;

public class BeginTimer : NetworkBehaviour
{
    bool can = true;
    private void OnTriggerEnter(Collider other)
    {
        if (!Runner.IsServer)
            return;
        if (other.tag.Equals("Player") && can)
        {
            can = false;
            GameManager.instance._timer.StartTimer(TimeSpan.FromSeconds(LevelManager.LevelTime));
        }
    }


}
