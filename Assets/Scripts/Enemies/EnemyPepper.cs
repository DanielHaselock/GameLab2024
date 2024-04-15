using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using GameLoop;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPepper : Enemy
{
    [System.Serializable]
    public enum PepperState
    {
        Passive,
        Aggressive,
    }

    [SerializeField] public PepperState myState;
    [SerializeField] private NetworkPrefabRef enemyPickupDummy;

    float delta = 0;
    int targetTime = 2;

    private Vector3 lastPosition;
    private float velocity;
    protected bool idle = true;
    protected bool prevIdle = true;

    protected override void Start()
    {
        base.Start();
        canAttack = true;
        //Go crazy        
        lastPosition = transform.position;
        animator.CrossFade("Idle", .25f);
        healthComponent.OnDamaged += OnAttacked;
        healthComponent.OnHealthDepleted += KillMyself;
    }

    private void OnDestroy()
    {
        healthComponent.OnDamaged -= OnAttacked;
    }

    private void OnAttacked(int damager, bool charged)
    {
        OnAttack();
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        if (!Runner.IsServer)
            return;

        if (dead)
            return;

        //UpdateMoveAndRotation(Runner.DeltaTime);
        velocity = Vector3.Distance(transform.position, lastPosition) / Runner.DeltaTime;
        lastPosition = transform.position;

        if (myState == PepperState.Passive)
        {
            delta += Runner.DeltaTime;
            if (delta > targetTime)
            {
                delta = 0;
                targetTime = Random.Range(1, 10);
                navMeshAgent.destination = GetNextWanderPos();
                //navMeshAgent.destination = new Vector3(transform.position.x + Random.Range(-20f, 20f), transform.position.y, transform.position.z + Random.Range(-10f, 10f));
            }
        }

        if (myState == PepperState.Aggressive) //aggressive
        {
            if (_targetPlayer != null)
            {
                navMeshAgent.destination = _targetPlayer.transform.position;
            }
            if (_seenPlayers.Count > 1)
                ChangeTargeting();
            if (_targetPlayer && canAttack && !stunned && Vector3.Distance(transform.position, _targetPlayer.transform.position) <= attackRange)
            {
                StartCoroutine(WaitAndAttack());
            }
        }

        if (!stunned)
        {
            if (velocity > 0.2f)
                idle = false;
            else
                idle = true;
            if (idle != prevIdle && !attacking)
            {
                if (idle)
                {
                    animator.CrossFade("Idle", .25f);
                }
                else
                {
                    animator.CrossFade("Run", .25f);
                }
            }
            prevIdle = idle;
        }
    }
    public override void ChangeTargeting()
    {
        if (attacking)
            return;

        base.ChangeTargeting();
        switch (_seenPlayers.Count)
        {
            case 0:
                _targetPlayer = null;
                StartCoroutine(WaitPassive());
                break;
            case 1:
                _targetPlayer = _seenPlayers[0];
                break;
            default:
                if (_seenPlayers[1] == null && _seenPlayers[0] == null){
                    _targetPlayer = null;
                    break;
                }
                
                if (Vector3.Distance(transform.position, _seenPlayers[0].transform.position) < Vector3.Distance(transform.position, _seenPlayers[1].transform.position))
                    _targetPlayer = _seenPlayers[1];
                else
                    _targetPlayer = _seenPlayers[0];
                break;
        }
    }
    IEnumerator WaitPassive()
    {
        bool happy = true;
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSecondsRealtime(.5f);
            if (_seenPlayers.Count > 0)
                happy = false;
        }
        myState = happy ? PepperState.Passive : PepperState.Aggressive;
        GetComponent<SphereCollider>().radius = 35;
    }

    IEnumerator WaitAndAttack()
    {
        canAttack = false;
        attacking = true;
        navMeshAgent.speed = 0;
        animator.CrossFade("Attack", .1f);
        //attack windup
        yield return new WaitForSeconds(.35f);
        if (stunned)
        {
            attacking = false;
            canAttack = true;
            yield break;
        }
        damageComponent.InitiateAttack("Player");
        //attack recovery
        yield return new WaitForSeconds(.17f);
        animator.CrossFade("Idle", .5f);
        attacking = false;
        navMeshAgent.speed = speed;
        //attack delay
        yield return new WaitForSeconds(3f);
        canAttack = true;
    }
    public override void OnAttack()
    {
        base.OnAttack();
        if (!healthComponent.HealthDepleted)
        {
            if (myState == PepperState.Passive)
            {
                myState = PepperState.Aggressive;
                if (_targetPlayer != null)
                    navMeshAgent.destination = _targetPlayer.transform.position;
                GetComponent<SphereCollider>().radius = 15;
            }
        }
    }
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
    }
    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
    }

    private async void KillMyself(int damager)
    {
        if (Runner.IsServer)
        {
            await Task.Delay(500);
            GameManager.instance.UpdateScore(damager, "bellpepper");
            if (!enemyPickupDummy.Equals(default))
                Runner.Spawn(enemyPickupDummy, transform.position + new Vector3(0, 3, 0), transform.rotation);
            Runner.Despawn(GetComponent<NetworkObject>());
        }
    }
}
