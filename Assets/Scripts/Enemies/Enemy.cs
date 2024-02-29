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

    public int maxHealth;
    protected int health;
    // Start is called before the first frame update
    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }
    private IEnumerator Start()
    {
        yield return new WaitWhile(() => Runner == null);
        if (Runner.IsServer)
            yield break;

        //Go crazy        
        health = maxHealth;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Player") && !_seenPlayers.Contains(other.gameObject))
        {
            _seenPlayers.Add(other.gameObject);
            ChangeTargeting();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Player"))
        {
            _seenPlayers.Remove(other.gameObject);
            ChangeTargeting();
        }
    }
    public virtual void ChangeTargeting() { }

    public virtual void OnAttack(int damage) {
        health -= damage;
    }
}
