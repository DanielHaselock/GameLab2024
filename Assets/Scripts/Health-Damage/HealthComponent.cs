using System;
using Fusion;
using Networking.Behaviours;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public class HealthComponent : NetworkBehaviour
{
    [FormerlySerializedAs("MaxHealth")] [SerializeField]
    private float maxHealth;
    [Networked] public float Health { get; private set; }
    [Networked] public bool CanDeplete { get; private set; } = true;
    [Networked] public bool HealthDepleted { get; private set; }

    public bool IsInitialised  { get; private set; }
    private ChangeDetector _change;

    public Action OnHealthDepleted;
    public Action OnDamaged;

    public float MaxHealth => maxHealth;
    
    public void SetHealthDepleteStatus(bool canDeplete)
    {
        CanDeplete = canDeplete;
    }
    
    public override void Spawned()
    {
        _change = GetChangeDetector(ChangeDetector.Source.SimulationState);
        if (Runner.IsServer)
            Health = maxHealth;

        CanDeplete = true;
        IsInitialised = true;
    }
    
    public void UpdateHealth(float Value)
    {
        if (!Runner.IsServer)
            return;
        if(!IsInitialised)
            return;
        var curr = Health;
        Health += Value;
        
        if(curr < Health)
            OnDamaged.Invoke();
        
        if (Health <= 0)
        {
            CanDeplete = false;
            Death();
        }
    }
    
    public void SetHealth(float Value)
    {
        if (!Runner.IsServer)
            return;
        
        if(!IsInitialised)
            return;
        Health = Value;
        
        CanDeplete = Health > 0;
        HealthDepleted = Health <= 0;
    }

    public override void Render()
    {
        base.Render();
        if(!IsInitialised)
            return;
        
        foreach (var change in _change.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(Health):
                    Debug.Log( $"{transform.parent.name} : HEALTH: {Health.ToString()}");
                    break;
            }
        }
    }

    public void Death()
    {
        if (!Runner.IsServer)
            return;
        
        HealthDepleted = true;
        OnHealthDepleted?.Invoke();
    }
}
