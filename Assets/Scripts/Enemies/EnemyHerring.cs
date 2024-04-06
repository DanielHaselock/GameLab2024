using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using GameLoop;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHerring : Enemy
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

    protected override void Start()
    {
        base.Start();
        canAttack = true;
        //Go crazy        
        animator.CrossFade("Idle", .25f);
        healthComponent.OnDamaged += OnAttacked;
        healthComponent.OnHealthDepleted += KillMyself;
    }

    private void OnDestroy()
    {
        healthComponent.OnDamaged -= OnAttacked;
    }

    private void OnAttacked(int damager)
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
                Vector3 direction = (_targetPlayer.transform.position - transform.position).normalized;
                navMeshAgent.destination = _targetPlayer.transform.position - direction * 25f;
            }
            if (_seenPlayers.Count > 1)
                ChangeTargeting();
        }

        if (!stunned)
        {
            animator.CrossFade("Idle", .25f);
        }
    }
    public override void ChangeTargeting()
    {
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
    
    public override void OnAttack()
    {
        base.OnAttack();
        if (!healthComponent.HealthDepleted)
        {
            if (myState == PepperState.Passive)
            {
                myState = PepperState.Aggressive;
                if (_targetPlayer != null)
                {
                    Vector3 direction = (_targetPlayer.transform.position - transform.position).normalized;
                    navMeshAgent.destination = _targetPlayer.transform.position - direction * 25f;
                }
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
            GameManager.instance.UpdateScore(damager, "herring");
            if (!enemyPickupDummy.Equals(default))
                Runner.Spawn(enemyPickupDummy, transform.position + new Vector3(0, 3, 0), transform.rotation);
            Runner.Despawn(GetComponent<NetworkObject>());
        }
    }
}
