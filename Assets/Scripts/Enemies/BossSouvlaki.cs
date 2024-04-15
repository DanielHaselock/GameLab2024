using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using GameLoop;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class BossSouvlaki : Enemy
{
    [SerializeField] private GameObject summonOnion;
    [SerializeField] private GameObject summonBellPepper;
    [SerializeField] private CapsuleCollider trigger;

    float delta = 0;
    int targetTime = 2;

    private Vector3 lastPosition;
    private float velocity;
    protected bool idle = true;
    protected bool prevIdle = true;

    protected bool rolling = false;

    float yPos = 0;

    protected override void Start()
    {
        base.Start();
        trigger.radius = 35;
        yPos = transform.position.y;
        canAttack = true;
        //Go crazy        
        lastPosition = transform.position;
        animator.CrossFade("Rise", .25f);
        healthComponent.OnDamaged += OnAttacked;
        healthComponent.OnHealthDepleted += KillMyself;

        navMeshAgent.updatePosition = false;
        navMeshAgent.updateRotation = false;
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
        transform.position = new Vector3(transform.position.x, yPos, transform.position.z);

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
                // Calculate the direction vector from the object to the target player
                Vector3 directionToTarget = _targetPlayer.transform.position - transform.position;

                // Rotate the direction vector by 90 degrees to the left
                Vector3 rotatedDirection = Quaternion.Euler(0, 105, 0) * directionToTarget;

                // Calculate the desired rotation based on the rotated direction
                Quaternion desiredRotation = Quaternion.LookRotation(rotatedDirection, Vector3.up);

                // Smoothly rotate towards the final rotation
                transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, 90 * Time.deltaTime);
                //navMeshAgent.destination = _targetPlayer.transform.position;
                if (_targetPlayer && canAttack && !stunned)
                {

                    //Melee Attacks
                    if (Vector3.Distance(transform.position, _targetPlayer.transform.position) <= attackRange && Random.Range(1, 100) > 50)
                    {
                        StartCoroutine(SpinAttack());
                    }
                    else if (Random.Range(1, 100) > 30)
                    {
                        if (Random.Range(1, 100) > 75)
                            StartCoroutine(SummoningRoar());
                        else
                        {
                            
                            StartCoroutine(RollAttack());
                        }
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
                    //animator.CrossFade("Run", .25f);
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
            yield return new WaitForSeconds(.17f);
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
                var no = Runner.Spawn(summonOnion, summonPosition, Quaternion.identity);
                enemy = no.gameObject;
                enemy.GetComponent<EnemyOnion>().myState = EnemyOnion.OnionState.Aggressive;
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
    IEnumerator SpinAttack()
    {
        //deactivate health collider so cant be hit
        canAttack = false;
        attacking = true;
        navMeshAgent.speed = 0;
        animator.CrossFade("Spin", .1f);
        //attack windup
        yield return new WaitForSeconds(.7f);
        rolling = true;
        trigger.radius = 2;
        //activate spin collider or something
        yield return new WaitForSeconds(2.08f);
        rolling = false;
        trigger.radius = 35;
        //deactivate colliders n stuff


        //attack recovery
        yield return new WaitForSeconds(.8f);
        //reactivate health collider so cant be hit
        animator.CrossFade("Idle", .5f);
        attacking = false;
        navMeshAgent.speed = speed;
        //attack delay
        yield return new WaitForSeconds(1.5f);
        canAttack = true;
    }

    IEnumerator RollAttack()
    {
        //deactivate health collider so cant be hit
        canAttack = false;
        attacking = true;
        navMeshAgent.speed = 0;
        animator.CrossFade("Roll", .1f);
        //attack windup
        yield return new WaitForSeconds(.7f);
        
        rolling = true;
        trigger.radius = 2;
        float startTime = Time.time;
        float duration = 1.8333f;
        while (Time.time - startTime < duration)
        {
            GetComponent<Rigidbody>().AddForce(transform.right * -.5f, ForceMode.Impulse);

            yield return null;
        }

        rolling = false;
        trigger.radius = 35;
        //deactivate colliders n stuff


        //attack recovery
        yield return new WaitForSeconds(.75f);
        //reactivate health collider so cant be hit
        animator.CrossFade("Idle", .5f);
        attacking = false;
        navMeshAgent.speed = speed;
        //attack delay
        yield return new WaitForSeconds(1f);
        canAttack = true;
    }
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if (rolling && other.tag.Equals("Player"))
        {
            
            damageComponent.InitiateAttack("Player");
        }
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
            GameManager.instance.UpdateScore(0, "boss");
            Runner.Despawn(GetComponent<NetworkObject>());
        }
    }
}
