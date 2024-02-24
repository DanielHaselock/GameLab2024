using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Networking.Data;

public class Enemy : NetworkBehaviour
{
    protected List<GameObject> _seenPlayers = new List<GameObject>();
    protected GameObject _targetPlayer = null;
    [SerializeField] protected float moveSpeed;

    // Start is called before the first frame update
    private IEnumerator Start()
    {
        yield return new WaitWhile(() => Runner == null);
        if (Runner.IsServer)
            yield break;

        //Go crazy        
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
}
