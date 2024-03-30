using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameLoop
{
    public class AlterCollector : NetworkBehaviour
    {
        [SerializeField] private float delayBeforeDespawn=1f;
        [SerializeField] private Collider _collider;
        
        private void OnTriggerEnter(Collider other)
        {
            if (Runner == null)
                return;
            var no = other.GetComponent<NetworkObject>();
            if(no == null)
                return;
            var collectable = no.GetComponent<AlterCollectible>();
            if(collectable == null)
                return;
            StartCoroutine(DestroyAfter(no, delayBeforeDespawn));
            Debug.Log($"Preparing to Delete {no.name}");
        }

        private IEnumerator DestroyAfter(NetworkObject no, float delay)
        {
            yield return new WaitForSeconds(delay);
            Runner.Despawn(no);
        }

        private void OnDrawGizmos()
        {
            if (_collider == null)
                return;

            var col = Color.blue;
            col.a = 0.25f;
            Gizmos.color = col;
            if(_collider is BoxCollider)
                Gizmos.DrawCube(transform.position, (_collider as BoxCollider).size);
            else if (_collider is SphereCollider)
                Gizmos.DrawSphere(transform.position, (_collider as SphereCollider).radius);
            
            #if UNITY_EDITOR
            Handles.Label(transform.position + new Vector3(0,2,0), "Alter Collectible Area");
            #endif
        }
    }
}