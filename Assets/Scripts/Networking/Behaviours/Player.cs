using System.Collections;
using Audio;
using Fusion;
using Fusion.Addons.SimpleKCC;
using Interactables;
using Networking.Behaviours;
using Networking.Data;
using UnityEngine;
using UnityEngine.Serialization;

public class Player : NetworkBehaviour
{
    new ref NetworkCCData Data => ref ReinterpretState<NetworkCCData>();

    [SerializeField] private TMPro.TMP_Text nicknameText;
    [SerializeField] private float moveSpeed=6.83f;
    [SerializeField] private float lookSpeed=720;
    [SerializeField] private float gravity=-9.81f;
    [SerializeField] private float jumpForce = 15;
    [SerializeField] private float stunDur = 0.5f;
    
    private NetworkObject _no;
    private CharacterController _stdController;
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
    
    [Networked] private bool Stunned { get; set; } = false;
    
    private void Awake()
    {
        _controller = GetComponent<SimpleKCC>();
        _reviver = GetComponentInChildren<PlayerReviver>();
        _health = GetComponentInChildren<HealthComponent>();
        _no = GetComponent<NetworkObject>();
        _anim = GetComponent<NetworkMecanimAnimator>();
        _pickupHandler = GetComponentInChildren<HandlePickup>();
        _myPickupable = GetComponent<PlayerPickupable>();
        _health.OnHealthDepleted += OnHealthDepleted;
        _health.OnDamaged += AnimateHit;
    }

    private void AnimateHit()
    {
        if(_stunRoutine != null)
            return;
        
        _anim.SetTrigger("Hit");
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
        this.name = "Player_" + no.InputAuthority.PlayerId;
        if (no.InputAuthority == Runner.LocalPlayer)
        {
            GetComponent<SetCamera>().SetCameraParams(gameObject.transform.GetChild(1).gameObject);
            GetComponent<PlayerInputController>().OnSpawned();
        }

        _controller.SetGravity(gravity);
        nicknameText.text = NetworkManager.Instance.GetPlayerNickNameById(no.InputAuthority.PlayerId);
    }
    
    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        if(_health.HealthDepleted)
            return;
        
        if(!_myPickupable.AllowInputs)
            return;
        
        if(Stunned)
            return;
        
        if (GetInput(out PlayerInputData data))
        {
            HandleInteract(data);
            HandleAttack(data);
            HandleRevive(data, Runner.DeltaTime);
            Move(data.MoveDirection, data.Jump);
        }
        
    }
    
    private void Move(Vector3 moveDir, bool jump)
    {
        float jumpImpulse = default;
        if (jump && _controller.IsGrounded)
        {
            jumpImpulse = jumpForce;
            _anim.SetTrigger("Jump", true);
            AudioManager.Instance.PlaySFX(SFXConstants.Jump);
        }
        
        
        if(!Mathf.Approximately(moveDir.magnitude, 0))
        {
            var newRot = Quaternion.LookRotation(moveDir.normalized, Vector3.up);
            var rot = Quaternion.RotateTowards(transform.rotation, newRot, lookSpeed*Runner.DeltaTime);
            _controller.SetLookRotation(rot);
        }
        
        _controller.Move(moveDir * moveSpeed, jumpImpulse);
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

    private void HandleAttack(PlayerInputData data)
    {
        if(!data.Attack)
            return;
        
        _anim.SetTrigger("Attack", true);
        AudioManager.Instance.PlaySFX(SFXConstants.Attack);
        
        if (Runner.IsServer)
        {
            if (_damager == null)
                _damager = GetComponentInChildren<DamageComponent>();
            _damager.InitiateAttack();
        }
    }

    private void HandleRevive(PlayerInputData data, float deltaTime)
    {
        _reviver.TryReviveOther(data.Revive, deltaTime);   
    }
    
    private void OnHealthDepleted()
    {
        AudioManager.Instance.PlaySFX(SFXConstants.Help, syncNetwork:true);
    }
}