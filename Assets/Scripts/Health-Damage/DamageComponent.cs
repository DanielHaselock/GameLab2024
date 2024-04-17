using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Audio;
using Fusion;
using GameLoop;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DamageComponent : NetworkBehaviour
{
    [FormerlySerializedAs("AttackDamage")] public float DefaultAttackDamage;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private float attackRadius;
    [SerializeField] private float attackCone = 60;
    [SerializeField] private float knockBackForce = 5;
    [SerializeField] private List<HealthComponent> hittableObjects = new List<HealthComponent>();

    [SerializeField] private bool allowHealOnKill;
    [SerializeField] private float healMin;
    [SerializeField] private float healMax;
    
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
        
        if(!WithinAttackCone(other.transform))
            return;
        
        if(charge)
            other.UpdateHealth(-_chargedamageToDeal, _damagerId, charge);
        else
            other.UpdateHealth(-_damageToDeal, _damagerId, charge);

        TryHeal(other);
        
        // var rb = other.GetComponentInParent<Rigidbody>();
        // if (rb != null)
        // {
        //     var revDir = rb.position - transform.position;
        //     revDir.y = 1;
        //     rb.AddForce(revDir.normalized*knockBackForce, ForceMode.Impulse);
        // }
    }

    private void TryHeal(HealthComponent other)
    {
        if(!allowHealOnKill)
            return;
        if (other.Health > 0)
            return;
        var perc = (selfheal.MaxHealth - selfheal.Health) / selfheal.MaxHealth; // the closer to death, the more you get.
        var offeredHealth = Mathf.Lerp(healMin, healMax, perc);
        StartCoroutine(DelayedHeal((int)offeredHealth));
    }

    IEnumerator DelayedHeal(int health)
    {
        yield return new WaitForSeconds(0.25f);
        selfheal.UpdateHealth(health, -1, false);
        selfheal.ShowHealFX();
    }
    
    private bool WithinAttackCone(Transform target)
    {
        var dir = target.position - transform.position;
        var angle = Vector3.SignedAngle(dir, transform.forward, transform.forward);
        return (angle >= -attackCone / 2) && (angle <= attackCone / 2);
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
        foreach (var hc in GetAllHealthAroundMe())
        {
            if (hc.transform.tag.Contains(tag))
            {
                Attack(hc, charged);
            }
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
        
        #if UNITY_EDITOR
        Color vision = new Color(1, 0, 0, 0.25f);
        vision.a = 0.5f;
        Handles.color = vision;
        var pos = transform.position;
        var forward = transform.forward;
        Handles.DrawSolidArc(pos, Vector3.up, forward, attackCone / 2, attackRadius);
        Handles.DrawSolidArc(pos, Vector3.up, forward, -attackCone / 2, attackRadius);
        #endif
    }
}
