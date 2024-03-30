using System;
using System.Collections.Generic;
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
    
    private void Start()
    {
        var weapon = transform.parent.GetComponentInChildren<Weapon>();
        if (weapon != null)
            _damageToDeal = weapon.Damage;
        else
            _damageToDeal = DefaultAttackDamage;
        selfheal = transform.parent.GetComponentInChildren<HealthComponent>();
        var player = GetComponentInParent<Player>();
        if (player != null)
            _damagerId = player.PlayerId;
    }

    public void Attack(HealthComponent other)
    {
        //block depletion
        if(!other.CanDeplete)
            return;
        
        Debug.Log($"Dealing Damage {_damageToDeal}");
        other.UpdateHealth(-_damageToDeal, _damagerId);
    }
    public void InitiateAttack()
    {
        if(Runner.IsServer)
            DoAttack();
        else
            RPC_InitiateAttackOnServer();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_InitiateAttackOnServer()
    {
        DoAttack();
    }
    
    private void DoAttack()
    {
        foreach (var hc in GetAllHealthAroundMe())
        {
            Attack(hc);
        }
    }

    public void InitiateAttack(string tag)
    {
        if (Runner.IsServer)
            DoAttack(tag);
        else
            RPC_InitiateAttackOnServer(tag);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_InitiateAttackOnServer(string tag)
    {
        DoAttack(tag);
    }

    private void DoAttack(string tag)
    {
        AudioManager.Instance.PlaySFX(SFXConstants.Attack);
        foreach (var hc in GetAllHealthAroundMe())
        {
            if (hc.transform.tag.Contains(tag))
                Attack(hc);
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

    private void OnDrawGizmos()
    {
        var color = Color.yellow;
        color.a = 0.25f;
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position, attackRadius);
    }
}
