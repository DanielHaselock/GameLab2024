using System;
using System.Collections;
using Audio;
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
    public float Health { get; private set; } //used so that others get access to the net-syched var
    
    [Networked]
    private float HealtNetSynched { get; set; }
    
    [Networked] public bool CanDeplete { get; private set; } = true;
    [Networked] public bool HealthDepleted { get; private set; }

    [SerializeField] private bool allowTempInvulnerability = false;
    [SerializeField] private float tempInvulnerabilityDur = 1f;
    [SerializeField] private ParticleSystem healfx;

    [Header("Audio")]
    [SerializeField] private string hitSFXKey;
    [SerializeField] private string deathSFXKey;
    
    public bool IsInitialised  { get; private set; }
    
    private ChangeDetector _change;

    public Action<int> OnHealthDepleted;
    public Action<int, bool> OnDamaged;

    private HitEffects _hitEffects;
    private float _myLocalHealth;
    
    public float MaxHealth => maxHealth;

    private bool _isInvulnerable = false;

    private Coroutine _healFXRoutine;
    
    private void Start()
    {
        _hitEffects = GetComponentInParent<HitEffects>();
    }

    private void FixedUpdate()
    {
        if(!IsInitialised)
            return;

        Health = HealtNetSynched;
        if (_myLocalHealth > HealtNetSynched)
        {
            if (_hitEffects != null)
            {
                _hitEffects.OnHit();
                AudioManager.Instance.PlaySFX3D(hitSFXKey, transform.position);
            }

            _myLocalHealth = HealtNetSynched;
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
            HealtNetSynched = maxHealth;

        _myLocalHealth = HealtNetSynched;
        CanDeplete = true;
        IsInitialised = true;
    }
    
    public void UpdateHealth(float Value, int damager, bool charged)
    {
        if (!Runner.IsServer)
            return;
        if(!IsInitialised)
            return;
        
        if(_isInvulnerable)
            return;
        
        var curr = HealtNetSynched;
        if (Value < 0)
        {
            Debug.Log("Ow!!"); 
            OnDamaged?.Invoke(damager, charged);
            if (allowTempInvulnerability)
            {
                StartCoroutine(BeInvulnerableRoutine());
            }
        }

        HealtNetSynched += Value;
        HealtNetSynched = Mathf.Clamp(HealtNetSynched, 0, MaxHealth);
        if (HealtNetSynched <= 0)
        {
            CanDeplete = false;
            Death(damager);
        }

        RPC_SpawnEffects(Value);
    }

    IEnumerator BeInvulnerableRoutine()
    {
        _isInvulnerable = true;
        yield return new WaitForSeconds(tempInvulnerabilityDur);
        _isInvulnerable = false;
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
        HealtNetSynched = Value;
        CanDeplete = HealtNetSynched > 0;
        HealthDepleted = HealtNetSynched <= 0;
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
                case nameof(HealtNetSynched):
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
        RPC_PlayDeathSFX();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayDeathSFX()
    {
        AudioManager.Instance.PlaySFX3D(deathSFXKey, transform.position);
    }

    public void ShowHealFX()
    {
        RPC_PlayHealFX();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayHealFX()
    {
        if (_healFXRoutine != null)
            StopCoroutine(_healFXRoutine);

        _healFXRoutine = StartCoroutine(HealFxRoutine());
    }

    IEnumerator HealFxRoutine()
    {
        var emission = healfx.emission;
        float curr = emission.rateOverTime.constant;
        var t = 0f;
        if (curr < 10)
        {
            while (t <= 1)
            {
                t += Time.deltaTime / 0.25f;
                emission.rateOverTime = (int)Mathf.Lerp(curr, 10, t);
                yield return new WaitForEndOfFrame();
            }
        }
        yield return new WaitForSeconds(0.5f);
        t = 0;
        while (t <= 1)
        {
            t += Time.deltaTime / 0.25f;
            emission.rateOverTime = (int)Mathf.Lerp(10, 0, t);
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("Heal");
    }
}
