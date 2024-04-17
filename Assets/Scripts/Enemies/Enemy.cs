using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Fusion;
using Fusion.Addons.Physics;
using UnityEngine.AI;
using Networking.Behaviours;

[RequireComponent(typeof(NetworkRigidbody3D))]
public class Enemy : NetworkBehaviour
{
    protected List<GameObject> _seenPlayers = new List<GameObject>();
    protected GameObject _targetPlayer = null;
    protected NavMeshAgent navMeshAgent;
    protected Rigidbody rb;
    
    protected float Health
    {
        get
        {
            if (healthComponent == null)
                return 0;
            if (!healthComponent.IsInitialised)
                return 0;

            return healthComponent.Health;
        }
    }

    [SerializeField] protected float speed=3;
    [SerializeField] protected float angularSpeed=120;
    [SerializeField] protected float attackRange=3;
    [SerializeField] protected float maxNextWanderPosDist=5;
    [SerializeField] protected GameObject healthBar;
    
    protected bool dead = false;
    protected bool stunned = false;
    protected bool canAttack = false;
    protected bool attacking = false;

    protected Animator animator;
    protected HealthComponent healthComponent;
    protected DamageComponent damageComponent;
    
    // Start is called before the first frame update
    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        if (healthBar == null || healthComponent == null || healthComponent.Health == healthComponent.MaxHealth)
            return;

        ShowHealthBar();
    }

    private void ShowHealthBar()
    {
        var player = NetworkManager.Instance.GetLocalPlayer();
        var distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        bool showHealthBar = distanceToPlayer <= 10.0f;
        healthBar.SetActive(showHealthBar);

        if (Runner.IsServer)
            RPC_ShowHealthBarOnClients();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowHealthBarOnClients()
    {
        if (Runner.IsServer)
            return;

        ShowHealthBar();
    }

    protected Vector3 GetNextWanderPos()
    {
        var delta = Random.insideUnitCircle * maxNextWanderPosDist;
        return new Vector3(transform.position.x + delta.x, transform.position.y, transform.position.z + delta.y);
    }
    
    protected virtual void Start()
    {
        animator = GetComponentInChildren<Animator>();
        healthComponent = GetComponentInChildren<HealthComponent>();
        damageComponent = GetComponentInChildren<DamageComponent>();
        //Go crazy        
        speed = GetComponent<NavMeshAgent>().speed;
        rb = GetComponent<Rigidbody>();
        
        //set the navmesh properties
        navMeshAgent.stoppingDistance = attackRange - 0.5f;
        navMeshAgent.speed = speed;
        navMeshAgent.angularSpeed = angularSpeed;

        navMeshAgent.Warp(transform.position);

        //we want to manually update our agent position
        /*navMeshAgent.updatePosition = false;
        navMeshAgent.updateRotation = false;*/
    }

    protected void UpdateMoveAndRotation(float deltaTime)
    {
        var nextPosition = navMeshAgent.nextPosition;
        nextPosition.y = transform.position.y; //eliminate y position diff
        var dir = (nextPosition - transform.position).normalized;
        rb.MovePosition(Vector3.Lerp(transform.position, navMeshAgent.nextPosition, 0.1f));
        if (Mathf.Approximately(dir.magnitude, 0))
            return;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        rb.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, navMeshAgent.angularSpeed * deltaTime);
    }
    
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Player") && !_seenPlayers.Contains(other.gameObject))
        {
            _seenPlayers.Add(other.gameObject);
            ChangeTargeting();
        }
    }
    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Player"))
        {
            _seenPlayers.Remove(other.gameObject);
            ChangeTargeting();
        }
    }
    public virtual void ChangeTargeting() { }

    public virtual void OnAttack() {
        if (Health > 0)
            StartCoroutine(Stun());
        else
        {
            animator.StopPlayback();
            dead = true;
            navMeshAgent.speed = 0;
            rb.AddForce((transform.position - _targetPlayer.transform.position).normalized * 1f, ForceMode.Impulse);
        }
    }
    IEnumerator Stun()
    {
        if (!name.Contains("Boss"))
        {
            Debug.Log("STUN!!");
            canAttack = false;
            navMeshAgent.speed = 0;

            animator.CrossFade("Hit", .01f);
            stunned = true;
            yield return new WaitForSecondsRealtime(.5f);
            animator.CrossFade("Idle", .25f);
            yield return new WaitForSecondsRealtime(1.5f);
            stunned = false;

            navMeshAgent.speed = speed;
            canAttack = true;
        }
        else if (!attacking)
        {
            Debug.Log("STUN!!");
            navMeshAgent.speed = 0;

            animator.CrossFade("Hit", .01f);
            stunned = true;
            yield return new WaitForSecondsRealtime(.5f);
            animator.CrossFade("Idle", .25f);
            yield return new WaitForSecondsRealtime(1.5f);
            stunned = false;

            navMeshAgent.speed = speed;
        }
    }

    public void SynchedCrossFade(string clipName, float fade)
    {
        if (Runner.IsServer)
            RPC_PlayAnim(clipName, fade);
        Debug.Log("rpc anim");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_PlayAnim(string name, float fade)
    {
        animator.CrossFade(name, fade);
    }
}
