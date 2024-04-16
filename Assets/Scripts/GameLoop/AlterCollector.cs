using System.Collections;
using Audio;
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
        [SerializeField] private Transform effectPos;
        
        [SerializeField] private GameObject collectionEffectFx;
        [SerializeField] private GameObject rejectionEffectFx;
        
        //-------------------------------
        [Header("Visual References")] 
        [SerializeField] private GameObject alterCamClose;
        [SerializeField] private GameObject alterCamFar;
        
        [SerializeField] private GameObject alterGraphic;
        [SerializeField] private GameObject spawnFx;
        [SerializeField] private Transform spawnFxTrf;
        
        public GameObject AlterCamClose => alterCamClose;
        public GameObject AlterCamFar => alterCamFar;
        public GameObject AlterGraphic => alterGraphic;
        public GameObject SpawnEffect => spawnFx;
        public Transform SpawnFXTRF => spawnFxTrf;
        
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
            if(no == null)
                yield break;
            
            var raiseObj = no.GetComponent<RaiseObjective>();
            if (raiseObj != null)
            {
                if(LevelManager.ContainsObjective(raiseObj.Key))
                    RPC_SpawnCollectEffect();
                else
                {
                    RPC_SpawnRejectEffect();
                }
            }
            
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

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_SpawnCollectEffect()
        {
            if(collectionEffectFx != null)
                Instantiate(collectionEffectFx, effectPos.position, Quaternion.identity);
            
            AudioManager.Instance.PlaySFX3D(AudioConstants.ItemCollect, transform.position);
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_SpawnRejectEffect()
        {
            if(rejectionEffectFx != null)
                Instantiate(rejectionEffectFx, effectPos.position, Quaternion.identity);
            
            AudioManager.Instance.PlaySFX3D(AudioConstants.ItemReject, transform.position);
        }
    }
}