using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class DamageComponent : NetworkBehaviour
{
    public float AttackDamage;

    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private float attackRadius;
    [SerializeField] private List<HealthComponent> hittableObjects = new List<HealthComponent>();

    HealthComponent selfheal;


    private void Start()
    {
        selfheal = transform.parent.GetComponentInChildren<HealthComponent>();
    }

    public void Attack(HealthComponent other)
    {
        //block depletion
        if(!other.CanDeplete)
            return;
        
        other.UpdateHealth(-AttackDamage);
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
