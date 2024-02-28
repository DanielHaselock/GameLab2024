using Networking.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerNetworkedActions : MonoBehaviour
{

    private PlayerInputData InputData = new PlayerInputData();
    public void BroadcastMove(Component Sender, object data)
    {
        InputData.MoveDirection = (Vector3)data;
    }

    public void BroadcastInteract(Component Sender, object data)
    {
        InputData.Interact = (bool)data;
    }

    public void BroadcastDrop(Component Sender, object data)
    {
        InputData.Drop = (bool)data;
    }


    public void RefreshInputData()
    {
        InputData.RefreshInputs();
    }
    public PlayerInputData GetInputData() 
    {
        return InputData; 
    }
}