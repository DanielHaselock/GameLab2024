using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class DamageComponent : NetworkBehaviour
{
    public float AttackDamage;

    [SerializeField] private float attackRadius;
    [SerializeField] private List<HealthComponent> hittableObjects = new List<HealthComponent>();

    HealthComponent selfheal;


    private void Start()
    {
        selfheal = transform.parent.GetComponentInChildren<HealthComponent>();
    }

    public void Attack(HealthComponent other)
    {
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
    
    private List<HealthComponent> GetAllHealthAroundMe()
    {
        var list = new List<HealthComponent>();
        var colliders = Physics.OverlapSphere(transform.position, attackRadius, LayerMask.NameToLayer("Health"));
        foreach (var collider in colliders)
        {
            var health = collider.GetComponent<HealthComponent>();
            if(health.Equals(selfheal))
                continue;
            
            list.Add(health);
        }

        return list;
    }
}
