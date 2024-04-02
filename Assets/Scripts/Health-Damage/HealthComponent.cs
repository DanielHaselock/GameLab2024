using System;
using Effects;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public class HealthComponent : NetworkBehaviour
{
    private const string EFFECT_PATH = "Effects/DamageIndicator";
    
    [FormerlySerializedAs("MaxHealth")] [SerializeField]
    private float maxHealth;
    [Networked] public float Health { get; private set; }
    [Networked] public bool CanDeplete { get; private set; } = true;
    [Networked] public bool HealthDepleted { get; private set; }

    public bool IsInitialised  { get; private set; }
    private ChangeDetector _change;

    public Action<int> OnHealthDepleted;
    public Action<int> OnDamaged;

    private HitEffects _hitEffects;
    private float _myLocalHealth;
    
    public float MaxHealth => maxHealth;

    private void Start()
    {
        _hitEffects = GetComponentInParent<HitEffects>();
    }

    private void FixedUpdate()
    {
        if(!IsInitialised)
            return;
        
        if (_myLocalHealth > Health)
        {
            if(_hitEffects != null)
                _hitEffects.OnHit();
            
            _myLocalHealth = Health;
        }
    }

    public void SetHealthDepleteStatus(bool canDeplete)
    {
        CanDeplete = canDeplete;
    }
    
    public override void Spawned()
    {
        _change = GetChangeDetector(ChangeDetector.Source.SimulationState);
        if (Runner.IsServer)
            Health = maxHealth;

        _myLocalHealth = Health;
        CanDeplete = true;
        IsInitialised = true;
    }
    
    public void UpdateHealth(float Value, int damager)
    {
        if (!Runner.IsServer)
            return;
        if(!IsInitialised)
            return;
        var curr = Health;
        if (Value < 0)
        {
            Debug.Log("Ow!!"); 
            OnDamaged?.Invoke(damager);
        }

        Health += Value;
        if (Health <= 0)
        {
            CanDeplete = false;
            Death(damager);
        }

        RPC_SpawnEffects(Value);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SpawnEffects(float value)
    {
        var go = Resources.Load<GameObject>(EFFECT_PATH);
        var ranPos = Random.insideUnitCircle;
        var position = transform.position + new Vector3(0, 1, 0) + new Vector3(ranPos.x, 0, ranPos.y);
        var spawned = Instantiate(go, position, Quaternion.identity);
        var txt = spawned.GetComponentInChildren<TMPro.TMP_Text>();
        var shadow = txt.transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
        if (value>0)
        {
            txt.color = Color.green;
        }
        txt.text = shadow.text = $"{(value > 0 ? "+" : "")}{value}";
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
                    break;
            }
        }
    }

    public void Death(int damager)
    {
        if (!Runner.IsServer)
            return;
        HealthDepleted = true;
        OnHealthDepleted?.Invoke(damager);
    }
}
