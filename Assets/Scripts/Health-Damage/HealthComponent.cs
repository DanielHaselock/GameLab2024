using Fusion;
using Networking.Behaviours;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IHealth
{ 
    public void RemoveHealth(float Value);
    public void AddHealth(float Value);

    public void SetHealth(float Value);

    public void Death();
}


public class HealthComponent : MonoBehaviour, IHealth
{

    public float MaxHealth;
    public float Health = 10;


    private void Start()
    {
        Health = MaxHealth;
    }
    public void RemoveHealth(float Value)
    {
        Health -= Value;
        Debug.Log(gameObject.transform.parent.gameObject.name + " is Taking Damage");
        Debug.Log("Current Health : " + Health);

        if (Health <= 0)
            Death();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void AddHealth(float Value) 
    {
        Health += Value;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void SetHealth(float Value)
    {
        Health = Value;
    }


    public void Death()
    {
        Debug.Log("DEATH!");
        gameObject.transform.parent.GetComponent<NetworkObject>().Runner.Despawn(gameObject.transform.parent.GetComponent<NetworkObject>());
    }




}
