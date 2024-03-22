using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Networking.Data;
using UnityEngine.AI;

public class Enemy : NetworkBehaviour
{
    protected List<GameObject> _seenPlayers = new List<GameObject>();
    protected GameObject _targetPlayer = null;

    protected NavMeshAgent navMeshAgent;

    protected float health;

    [SerializeField] protected Animator animator;

    protected bool dead = false;
    protected bool stunned = false;
    protected bool canAttack = false;
    protected bool attacking = false;

    [SerializeField] protected HealthComponent healthComponent;
    [SerializeField] protected DamageComponent damageComponent;

    protected float speed;

    [SerializeField] protected GameObject deathDrops;
    // Start is called before the first frame update
    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }
    protected virtual void Start()
    {
        //Go crazy        
        health = healthComponent.MaxHealth;
        speed = GetComponent<NavMeshAgent>().speed;
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
        health = healthComponent.Health;
        if (health > 0)
            StartCoroutine(stun());
        else
        {
            animator.StopPlayback();
            // TODO spawn the collectible corpse here
            GameObject drops = Instantiate(deathDrops);
            drops.transform.position = transform.position;
            dead = true;
            navMeshAgent.speed = 0;
            GetComponent<Rigidbody>().AddForce((_targetPlayer.transform.position - transform.position).normalized * 1f, ForceMode.Impulse);
            Destroy(gameObject);
        }
    }
    IEnumerator stun()
    {
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
}
