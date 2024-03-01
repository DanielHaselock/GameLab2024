using Fusion;
using Networking.Behaviours;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HealthComponent : NetworkBehaviour
{

    public float MaxHealth;
    [Networked] public float Health { get; set; }

    private ChangeDetector _change;

    public override void Spawned()
    {
        _change = GetChangeDetector(ChangeDetector.Source.SimulationState);
        if (Runner.IsServer)
            Health = MaxHealth;
    }
    
    public void UpdateHealth(float Value)
    {
        if (!Runner.IsServer)
            return;
        
        Health += Value;
        
        if (Health <= 0)
            Death();
    }
    
    public void SetHealth(float Value)
    {
        if (!Runner.IsServer)
            return;
        
        Health = Value;
    }

    public override void Render()
    {
        base.Render();
        foreach (var change in _change.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(Health):
                    Debug.Log( $"{NetworkManager.Instance.GetPlayerNickNameById(Runner.LocalPlayer.PlayerId)} HEALTH: {Health}");
                    break;
            }
        }
    }

    public void Death()
    {
        if (!Runner.IsServer)
            return;
        
        Debug.Log("DEATH!");
        gameObject.transform.parent.GetComponent<NetworkObject>().Runner.Despawn(gameObject.transform.parent.GetComponent<NetworkObject>());
    }
}
