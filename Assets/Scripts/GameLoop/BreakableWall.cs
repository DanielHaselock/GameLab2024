using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Networking.Behaviours;
using UnityEngine;

public class BreakableWall : NetworkBehaviour
{
    [SerializeField] private GameObject wall;
    [SerializeField] private GameObject brokenWall;
    [SerializeField] private GameObject cameraShake;
    
    private HealthComponent _health;
    
    private void Start()
    {
        _health = GetComponentInChildren<HealthComponent>();
        _health.OnDamaged += OnDamaged;
    }

    private void OnDamaged(int damager, bool charged)
    {
        if(!Runner.IsServer)
            return;
        if(!charged)
            return;
        RPC_BreakWall();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_BreakWall()
    {
        StartCoroutine(BreakWalls());
        var localPlayer = NetworkManager.Instance.GetLocalPlayer().gameObject;
        var dist = Vector3.Distance(localPlayer.transform.position, transform.position);
        if (dist <= 5)
        {
            SpawnCameraShake();
        }
    }

    IEnumerator BreakWalls()
    {
        wall.SetActive(false);
        brokenWall.SetActive(true);
        yield return new WaitForSeconds(3);
        Runner.Despawn(GetComponent<NetworkObject>());
        ApplyForce();
    }

    private void ApplyForce()
    {
        foreach (var rb in GetComponentsInChildren<Rigidbody>())
        {
            rb.AddExplosionForce(2, transform.position, 2);
        }
    }
    
    private void SpawnCameraShake()
    {
        Instantiate(cameraShake, transform.position, transform.rotation);
    }
}
