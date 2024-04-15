using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using GameLoop;
using UnityEngine;
using UnityEngine.AI;

public class EnemyLettuce : Enemy
{
    [SerializeField] private NetworkPrefabRef enemyPickupDummy;

    protected List<GameObject> _seenOnions = new List<GameObject>();
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

        delta += Runner.DeltaTime;
        if (delta > targetTime)
        {
            delta = 0;
            targetTime = Random.Range(1, 10);
            navMeshAgent.destination = GetNextWanderPos();
            //navMeshAgent.destination = new Vector3(transform.position.x + Random.Range(-20f, 20f), transform.position.y, transform.position.z + Random.Range(-10f, 10f));
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
            foreach (GameObject onion in _seenOnions)
            {
                if (onion != null)
                {
                    if (onion.GetComponent<EnemyOnion>().myState == EnemyOnion.OnionState.Passive)
                        onion.GetComponent<EnemyOnion>().Alert(_targetPlayer);
                }
            }
        }
    }
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if (other.tag.Equals("Enemy") && !_seenOnions.Contains(other.gameObject) && (other.name.Contains("Oignon")))
        {
            _seenOnions.Add(other.gameObject);
        }
    }
    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        if (other.tag.Equals("Enemy") && (other.name.Contains("Oignon")))
        {
            _seenOnions.Remove(other.gameObject);
        }
    }

    private async void KillMyself(int damager)
    {
        if (Runner.IsServer)
        {
            await Task.Delay(500);
            GameManager.instance.UpdateScore(damager, "lettuce");
            if (!enemyPickupDummy.Equals(default))
                Runner.Spawn(enemyPickupDummy, transform.position + new Vector3(0, 3, 0), transform.rotation);
            Runner.Despawn(GetComponent<NetworkObject>());
        }
    }
}
