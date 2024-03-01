using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyOnion : Enemy
{
    protected bool passive = true;
    protected List<GameObject> _seenOnions = new List<GameObject>();
    float delta = 0;
    int targetTime = 7;
    private void Start()
    {
        
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        if (passive)
        {
            delta += Runner.DeltaTime;
            if (delta > targetTime)
            {
                delta = 0;
                targetTime = Random.Range(1, 10);
                navMeshAgent.destination = new Vector3(transform.position.x + Random.Range(-10f, 10f), transform.position.y, transform.position.z + Random.Range(-10f, 10f));
            }
        }
        else //aggressive
        {
            if (_targetPlayer != null)
            {
                navMeshAgent.destination = _targetPlayer.transform.position;
            }
            if (_seenPlayers.Count > 1)
                ChangeTargeting();
        }
    }
    public override void ChangeTargeting()
    {
        base.ChangeTargeting();
        switch (_seenPlayers.Count)
        {
            case 0:
                _targetPlayer = null;
                StartCoroutine(waitPassive());
                break;
            case 1:
                _targetPlayer = _seenPlayers[0];
                break;
            default:
                if (Vector3.Distance(transform.position, _seenPlayers[0].transform.position) < Vector3.Distance(transform.position, _seenPlayers[1].transform.position))
                    _targetPlayer = _seenPlayers[1];
                else
                    _targetPlayer = _seenPlayers[0];
                break;
        }
    }
    IEnumerator waitPassive()
    {
        bool happy = true;
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSecondsRealtime(.5f);
            if (_seenPlayers.Count > 0)
                happy = false;
        }
        passive = happy;
        GetComponent<SphereCollider>().radius = 35;
    }
    public override void OnAttack(int damage)
    {
        base.OnAttack(damage);
        if (passive)
        {
            passive = false;
            navMeshAgent.destination = _targetPlayer.transform.position;
            GetComponent<SphereCollider>().radius = 15;
        }
        foreach (GameObject onion in _seenOnions)
        {
            if (onion.GetComponent<EnemyOnion>().passive)
                onion.GetComponent<EnemyOnion>().alert(_targetPlayer);
        }
    }
    public void alert(GameObject player)
    {
        passive = false;
        //Can't add player to seen players, as that needs to happen on its own time.
        navMeshAgent.destination = player.transform.position;
        GetComponent<SphereCollider>().radius = 15;
    }
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if (other.tag.Equals("Enemy") && !_seenOnions.Contains(other.gameObject) && other.name.Contains("Onion"))
        {
            _seenOnions.Add(other.gameObject);
        }
    }
    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        if (other.tag.Equals("Enemy") && other.name.Contains("Onion"))
        {
            _seenOnions.Remove(other.gameObject);
        }
    }
}
