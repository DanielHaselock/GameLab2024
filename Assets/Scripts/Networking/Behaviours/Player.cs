using System;
using System.Collections;
using Audio;
using Fusion;
using Fusion.Addons.SimpleKCC;
using GameLoop;
using Interactables;
using Networking.Behaviours;
using Networking.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

public class Player : NetworkBehaviour
{
    [SerializeField] private TMPro.TMP_Text nicknameText;
    [SerializeField] private float moveSpeed=6.83f;
    [SerializeField] private float sprintSpeed = 13.66f;
    [SerializeField] private float lookSpeed=720;
    [SerializeField] private float gravity=-9.81f;
    [SerializeField] private float jumpForce = 15;
    [SerializeField] private float stunDur = 0.5f;
    [SerializeField] private float attackDelay=0.25f;
    [SerializeField] private float attackChargeDur = 1f;
    [SerializeField] private GameObject chargedAttackGraphic;
    [SerializeField] private SpatialAudioController spatialAudioController;
    [SerializeField] private GameObject cameraShake;
    
    private NetworkObject _no;
    private Rigidbody _rb;
    private SimpleKCC _controller;
    private NetworkMecanimAnimator _anim;
    private HandlePickup _pickupHandler;
    private DamageComponent _damager;
    private HealthComponent _health;
    private PlayerPickupable _myPickupable;
    private PlayerReviver _reviver;
    
    private Vector3 verticalVelocity;

    private Coroutine _stunRoutine;
    private Tick _initial;
    private float _nextAttackTime = 0;
    
    private bool _teleport { get; set; }
    private Vector3 _teleportPos;
    private Quaternion _teleportRot;
    
    private bool _charging { get; set; }
    [Networked] private float _charge { get; set; }

    [Networked] private bool Stunned { get; set; } = false;
    [Networked] private bool Sprinting { get; set; } = false;

    public bool PlayerDowned => _health.HealthDepleted;
    
    public int PlayerId { get; private set; }

    private bool _canCharge;
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _controller = GetComponent<SimpleKCC>();
        _reviver = GetComponentInChildren<PlayerReviver>();
        _damager = GetComponentInChildren<DamageComponent>();
        _health = GetComponentInChildren<HealthComponent>();
        _no = GetComponent<NetworkObject>();
        _anim = GetComponent<NetworkMecanimAnimator>();
        _pickupHandler = GetComponentInChildren<HandlePickup>();
        _myPickupable = GetComponent<PlayerPickupable>();
        _health.OnHealthDepleted += OnHealthDepleted;
        _health.OnDamaged += AnimateHit;
    }

    private void Start()
    {
        _canCharge = LevelManager.AllowChargedAttack;
    }

    private void AnimateHit(int damager, bool charged)
    {
        if(_stunRoutine != null)
            return;
        
        _anim.SetTrigger("Hit", true);
        _stunRoutine = StartCoroutine(Stun());
    }
    
    IEnumerator Stun()
    {
        Stunned = true;
        yield return new WaitForSeconds(stunDur);
        Stunned = false;
        _stunRoutine = null;
    }
    
    public override void Spawned()
    {
        var no = GetComponent<NetworkObject>();
        PlayerId = no.InputAuthority.PlayerId;
        this.name = "Player_" + no.InputAuthority.PlayerId;
        if (no.InputAuthority == Runner.LocalPlayer)
        {
            var cameraTarget = GetComponentInChildren<CameraTarget>();
            GetComponent<SetCamera>().SetCameraParams(cameraTarget.gameObject);
            GetComponent<PlayerInputController>().OnSpawned();
        }
        _controller.SetGravity(gravity);
        nicknameText.text = NetworkManager.Instance.GetPlayerNickNameById(no.InputAuthority.PlayerId);
    }
    
    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        if (GetInput(out PlayerInputData data))
        {
            Sprinting = data.Sprint;

            if (_myPickupable.IsPickedUp)
                return;

            TryTeleporting();
            
            var moveVec = _health.HealthDepleted || GameManager.instance.BossSpawning
                ? Vector3.zero
                : data.MoveDirection;
            var jump = !_health.HealthDepleted && !GameManager.instance.BossSpawning && data.Jump;
            
            Move(moveVec, jump);
            if(_health.HealthDepleted)
                return;
        
            if(!_myPickupable.AllowInputs)
                return;
        
            if(Stunned)
                return;
            
            if(GameManager.instance.BossSpawning)
                return;
            
            HandleInteract(data);
            HandleChargedAttack(data);
            HandleRevive(data, Runner.DeltaTime);
        }
        
    }

    private void Update()
    {
        if (chargedAttackGraphic.activeSelf && _charge < 1)
        {
            chargedAttackGraphic.SetActive(false);
        }
        else if (!chargedAttackGraphic.activeSelf && _charge >= 1)
        {
            chargedAttackGraphic.SetActive(true);
            if(_no.HasInputAuthority)
                AudioManager.Instance.PlaySFX(AudioConstants.PlayerAttackCharge);
        }
    }

    private void Move(Vector3 moveDir, bool jump)
    {
        float jumpImpulse = default;
        if (jump && _controller.IsGrounded)
        {
            jumpImpulse = jumpForce;
            _anim.SetTrigger("Jump", true);
            AudioManager.Instance.PlaySFX(AudioConstants.Jump);
        }
        
        
        if(!Mathf.Approximately(moveDir.magnitude, 0))
        {
            var newRot = Quaternion.LookRotation(moveDir.normalized, Vector3.up);
            var rot = Quaternion.RotateTowards(transform.rotation, newRot, lookSpeed*Runner.DeltaTime);
            _controller.SetLookRotation(rot);
        }

        if (moveDir != Vector3.zero && _controller.IsGrounded)
        {
            spatialAudioController.PlayFootStep();
        }
        else
        {
            spatialAudioController.StopFootStep();
        }
        
        _controller.Move(moveDir * (Sprinting? sprintSpeed : moveSpeed), jumpImpulse);
        _anim.Animator.SetFloat("Move", moveDir.normalized.magnitude);
    }

    public void HandleInteract(PlayerInputData data)
    {
        if (!HasInputAuthority)
            return;
        
        if(_no.InputAuthority != Runner.LocalPlayer)
            return;
        
        if(_myPickupable.IsPickedUp)
            return;
        
        if (data.Interact)
        {
            PickItem();
        }
        else if (data.Drop)
        {
            DropItem();
        }
        else if(data.Throw)
        {
            ThrowItem(data);
        }
    }
    
    public void PickItem()
    {
       if(Runner.IsServer)
           _pickupHandler.InputPick();
       else
        RPC_PickItemOnServer();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_PickItemOnServer()
    {
        _pickupHandler.InputPick();
    }
    
    private void DropItem()
    {
        if (Runner.IsServer)
            _pickupHandler.InputDrop();
        else
            RPC_DropItemOnServer();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_DropItemOnServer()
    {
        Debug.Log("RPC");
        _pickupHandler.InputDrop();
    }
    

    private void ThrowItem(PlayerInputData data)
    {
        if(Runner.IsServer)
            _pickupHandler.InputThrow();
        else
            RPC_ThrowItem();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_ThrowItem()
    {
        Debug.Log("RPC");
        _pickupHandler.InputThrow();
    }

    private bool _chargeAnimTriggered;
    private bool _chargeClicked = false;
    private void HandleChargedAttack(PlayerInputData data)
    {
        if(Time.time < _nextAttackTime)
            return;
        
        if (data.ChargeAttack)
        {
            if (!_canCharge)
            {
                if (!_chargeClicked)
                {
                    _chargeClicked = true;
                    RegularAttack();
                }

                return;
            }
            
            if (_charging && _charge > 0.15f && !_chargeAnimTriggered)
            {
                _chargeAnimTriggered = true;
                _anim.Animator.SetBool("ChargeAttack", true);
            }

            data.Attack = false;
            _charging = true;
            _charge += Runner.DeltaTime / attackChargeDur;
            _charge = Mathf.Clamp01(_charge);
        }

        if (!data.ChargeAttack)
        {
            _chargeClicked = false;
            if(!_charging)
                return;
            
            if(_charge > 0.15f){
                _anim.Animator.SetBool("ChargeAttack", false);
            }
            else
            {
                        _anim.SetTrigger("Attack", true);
                _anim.Animator.SetTrigger("Attack");
            }
            
            _charging = false;
            _chargeAnimTriggered = false;
            if (Runner.IsServer)
            {
                bool charged = _charge >= 1;
                StartCoroutine(InitiateAttack(0.15f, charged));
                if (charged)
                {
                    Debug.Log($"Charged Attack {_charge}");
                }
                else
                    Debug.Log($"Attack {_charge}");
               
            }
            _nextAttackTime = Time.time + attackDelay;
            _charge = 0;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    private void RPC_LocalCameraShake()
    {
        Instantiate(cameraShake);
    }
    
    private void RegularAttack()
    {
        if(Time.time < _nextAttackTime)
            return;
        
        _charging = false;
        _chargeAnimTriggered = false;
        _charge = 0;
        
        _anim.SetTrigger("Attack", true);
        _anim.Animator.SetTrigger("Attack");
        
        if (Runner.IsServer)
        {
            StartCoroutine(InitiateAttack(0.15f, false));
            Debug.Log($"Attack {_charge}");
        }
        _nextAttackTime = Time.time + attackDelay;
    }
    
    IEnumerator InitiateAttack(float attackDelay, bool charged)
    {
        yield return new WaitForSeconds(attackDelay);
        if(charged)
            RPC_LocalCameraShake();
        RPC_WeaponSwingSFX();
        _damager.InitiateAttack(charged);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    private void RPC_WeaponSwingSFX()
    {
        AudioManager.Instance.PlaySFX3D(AudioConstants.PlayerAttack, transform.position);
    }
    
    private void HandleRevive(PlayerInputData data, float deltaTime)
    {
        _reviver.TryReviveOther(data.Revive, deltaTime);   
    }
    
    private void OnHealthDepleted(int damager)
    {
        AudioManager.Instance.PlaySFX(AudioConstants.Help, syncNetwork:true);
        if (Runner.IsServer)
            RPC_SetDowned(true);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SetDowned(bool downed)
    {
        _anim.Animator.SetBool("Downed", downed);
    }
    
    public void SetWeapon(int indx)
    {
        if(!Runner.IsServer)
            return;
        
        var weaponLoc = transform.FindDeepChild("WeaponLoc");
        for (int i = 0; i < weaponLoc.childCount; i++)
        {
            if (i == indx)
                weaponLoc.GetChild(i).gameObject.SetActive(true);
            else
                weaponLoc.GetChild(i).gameObject.SetActive(false);
        }

        RPC_ReflectWeaponUpgradeOnClient(indx);
        _damager.UpdateWeapon();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ReflectWeaponUpgradeOnClient(int indx)
    {
        if(Runner.IsServer)
            return;
        
        var weaponLoc = transform.FindDeepChild("WeaponLoc");
        for (int i = 0; i < weaponLoc.childCount; i++)
        {
            if (i == indx)
                weaponLoc.GetChild(i).gameObject.SetActive(true);
            else
                weaponLoc.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void MarkForTeleport(Vector3 pos, Quaternion rot)
    {
        if(!Runner.IsServer)
            return;
        _teleportPos = pos;
        _teleportRot = rot;
        _teleport = true;
    }

    private void TryTeleporting()
    {
        if(!_teleport)
            return;
        
        _controller.SetPosition(_teleportPos);
        _controller.SetLookRotation(_teleportRot);
        _teleport = false;
    }
}