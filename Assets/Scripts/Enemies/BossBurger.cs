using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using GameLoop;
using UnityEngine;
using UnityEngine.AI;

public class BossBurger : Enemy
{
    [SerializeField] private GameObject summonTomato;
    [SerializeField] private GameObject summonBellPepper;
    [SerializeField] private GameObject breakableWallJail;

    float delta = 0;
    int targetTime = 2;

    private Vector3 lastPosition;
    private float velocity;
    protected bool idle = true;
    protected bool prevIdle = true;

    float delay = 0;

    protected override void Start()
    {
        base.Start();
        canAttack = true;
        //Go crazy        
        lastPosition = transform.position;
        animator.CrossFade("Rise", .25f);
        healthComponent.OnDamaged += OnAttacked;
        healthComponent.OnHealthDepleted += KillMyself;

        //navMeshAgent.updatePosition = false;
        //navMeshAgent.updateRotation = false;
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

        delay += Time.deltaTime;
        if (delay < 3)
            return;

        //UpdateMoveAndRotation(Runner.DeltaTime);
        velocity = Vector3.Distance(transform.position, lastPosition) / Runner.DeltaTime;
        lastPosition = transform.position;

        if (_seenPlayers.Count <= 0)
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
        else
        {
            if (_targetPlayer != null)
            {
                navMeshAgent.destination = _targetPlayer.transform.position;
                if (_targetPlayer && canAttack && !stunned)
                {
                    
                    //Melee Attacks
                    if (Vector3.Distance(transform.position, _targetPlayer.transform.position) <= attackRange)
                    {
                        if (Random.Range(1,100) > 50)
                            StartCoroutine(WaitAndAttack());
                        else
                            StartCoroutine(WaitAndAttackLong());
                    }
                    else if (Random.Range(1, 100) > 80)
                    {
                        if (Random.Range(1, 100) > 60)
                            StartCoroutine(SummoningRoar());
                        else
                            StartCoroutine(SummonWalls());
                    }
                }
            }
            if (_seenPlayers.Count > 1)
                ChangeTargeting();
        }

        if (!stunned)
        {
            if (velocity > 0.2f)
                idle = false;
            else
                idle = true;
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
            if (idle != prevIdle && !attacking && !currentState.IsName("Rise"))
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
                break;
            case 1:
                _targetPlayer = _seenPlayers[0];
                break;
            default:
                if (_seenPlayers[1] == null && _seenPlayers[0] == null)
                {
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
        yield return new WaitForSeconds(2f);
        canAttack = true;
    }
    IEnumerator WaitAndAttackLong()
    {
        canAttack = false;
        attacking = true;
        navMeshAgent.speed = 0;
        animator.CrossFade("AttackLong", .1f);
        //attack windup
        yield return new WaitForSeconds(.8f);
        if (stunned)
        {
            attacking = false;
            canAttack = true;
            yield break;
        }
        damageComponent.DefaultAttackDamage = 3;
        damageComponent.InitiateAttack("Player");
        damageComponent.DefaultAttackDamage = 2;
        //attack recovery
        yield return new WaitForSeconds(.25f);
        animator.CrossFade("Idle", .5f);
        attacking = false;
        navMeshAgent.speed = speed;
        //attack delay
        yield return new WaitForSeconds(4f);
        canAttack = true;
    }
    IEnumerator SummoningRoar()
    {
        canAttack = false;
        attacking = true;
        navMeshAgent.speed = 0;
        animator.CrossFade("Roar", .1f);
        //attack windup
        yield return new WaitForSeconds(.8f);
        if (stunned)
        {
            attacking = false;
            canAttack = true;
            yield break;
        }
        for (int i = 0; i < Random.Range(2, 5); i++)
        {
            GameObject enemy;
            Vector3 summonPosition = new Vector3(Random.Range(transform.position.x - (i + 2), transform.position.x + (i + 2)), transform.position.y, Random.Range(transform.position.z - (i + 2), transform.position.z + (i + 2)));
            if (Random.Range(1, 100) > 50)
            {
                var no = Runner.Spawn(summonBellPepper, summonPosition, Quaternion.identity);
                enemy = no.gameObject;
                enemy.GetComponent<EnemyPepper>().myState = EnemyPepper.PepperState.Aggressive;
            }
            else
            {
                var no = Runner.Spawn(summonTomato, summonPosition, Quaternion.identity);
                enemy = no.gameObject;
            }

        }
        //attack recovery
        yield return new WaitForSeconds(.17f);
        animator.CrossFade("Idle", .5f);
        attacking = false;
        navMeshAgent.speed = speed;
        //attack delay
        yield return new WaitForSeconds(4f);
        canAttack = true;
    }
    IEnumerator SummonWalls()
    {
        canAttack = false;
        attacking = true;
        navMeshAgent.speed = 0;
        animator.CrossFade("Jump", .1f);
        //attack windup
        yield return new WaitForSeconds(.8f);
        if (stunned)
        {
            attacking = false;
            canAttack = true;
            yield break;
        }
        List<GameObject> cages = new List<GameObject>();
        foreach (GameObject player in _seenPlayers)
        {
            Runner.Spawn(breakableWallJail, new Vector3(player.transform.position.x, -1.5f, player.transform.position.z));
        }
        //attack recovery
        yield return new WaitForSeconds(.17f);
        animator.CrossFade("Idle", .5f);
        attacking = false;
        navMeshAgent.speed = speed;
        //attack delay
        yield return new WaitForSeconds(4f);
        canAttack = true;
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
            animator.CrossFade("Die", .1f);
            await Task.Delay(1500);          
            Runner.Despawn(GetComponent<NetworkObject>());
        }
    }
}
