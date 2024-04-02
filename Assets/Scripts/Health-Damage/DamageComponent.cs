using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Audio;
using Fusion;
using GameLoop;
using UnityEngine;
using UnityEngine.Serialization;

public class DamageComponent : NetworkBehaviour
{
    [FormerlySerializedAs("AttackDamage")] public float DefaultAttackDamage;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private float attackRadius;
    [SerializeField] private List<HealthComponent> hittableObjects = new List<HealthComponent>();
    
    HealthComponent selfheal;
    private int _damagerId = -1;
    private float _damageToDeal;
    private float _chargedamageToDeal;
    
    private void Start()
    {
        var weapon = transform.parent.GetComponentInChildren<Weapon>();
        if (weapon != null)
        {
            _damageToDeal = weapon.Damage;
            _chargedamageToDeal = weapon.ChargeDamage;
        }
        else
        {
            _chargedamageToDeal = DefaultAttackDamage;
            _damageToDeal = DefaultAttackDamage;
        }
            
        selfheal = transform.parent.GetComponentInChildren<HealthComponent>();
        var player = GetComponentInParent<Player>();
        if (player != null)
            _damagerId = player.PlayerId;
    }

    public void Attack(HealthComponent other, bool charge)
    {
        //block depletion
        if(!other.CanDeplete)
            return;
        
        Debug.Log($"Dealing Damage {_damageToDeal}");

        if(charge)
            other.UpdateHealth(-_chargedamageToDeal, _damagerId);
        else
            other.UpdateHealth(-_damageToDeal, _damagerId);
    }
    public void InitiateAttack(bool charged)
    {
        if(Runner.IsServer)
            DoAttack(charged);
        else
            RPC_InitiateAttackOnServer(charged);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_InitiateAttackOnServer(bool charged)
    {
        DoAttack(charged);
    }
    
    private void DoAttack(bool charged)
    {
        foreach (var hc in GetAllHealthAroundMe())
        {
            Attack(hc, charged);
        }
    }

    public void InitiateAttack(string tag, bool charged = false)
    {
        if (Runner.IsServer)
            DoAttack(tag, charged);
        else
            RPC_InitiateAttackOnServer(tag, charged);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_InitiateAttackOnServer(string tag, bool charged)
    {
        DoAttack(tag, charged);
    }

    private void DoAttack(string tag, bool charged)
    {
        AudioManager.Instance.PlaySFX(SFXConstants.Attack);
        foreach (var hc in GetAllHealthAroundMe())
        {
            if (hc.transform.tag.Contains(tag))
                Attack(hc, charged);
        }
    }

    private List<HealthComponent> GetAllHealthAroundMe()
    {
        var list = new List<HealthComponent>();
        var colliders = Physics.OverlapSphere(transform.position, attackRadius, _layerMask);
        foreach (var collider in colliders)
        {
            var health = collider.GetComponent<HealthComponent>();
            if(health.Equals(selfheal))
                continue;
            
            list.Add(health);
        }

        return list;
    }

    public void UpdateWeapon()
    {
        var weapon = transform.parent.GetComponentInChildren<Weapon>();
        if (weapon != null)
            _damageToDeal = weapon.Damage;
        else
            _damageToDeal = DefaultAttackDamage;
    }
    
    private void OnDrawGizmos()
    {
        var color = Color.yellow;
        color.a = 0.25f;
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position, attackRadius);
    }
}
