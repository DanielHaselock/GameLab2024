using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.UI;

namespace Interactables
{
    public class PlayerReviver : NetworkBehaviour
    {
        [SerializeField] private NetworkMecanimAnimator anim;
        
        [SerializeField] private GameObject reviveGUI;
        [SerializeField] private Image reviveImg;
        
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private float playerDetectionRadius;
        [SerializeField] private float timeToRevive;

        [SerializeField] private SpatialAudioController spatialAudioController;
        
        [Networked] private bool Reviving { get; set; }
        [Networked] private float ReviveTimeLeft { get; set; }

        private PlayerReviver _targetReviver;
        private HealthComponent _myHealthComp;

        public bool CanBeRevived => _myHealthComp.HealthDepleted;
        
        private void Start()
        {
            _myHealthComp = transform.parent.GetComponentInChildren<HealthComponent>();
        }

        public void TryReviveOther(bool revive, float deltaTime)
        {
            if(!Runner.IsServer)
                return;
            
            if (revive && !_targetReviver)
            {
                _targetReviver = GetOtherPlayerReviver();
                if(_targetReviver == null)
                    return;

                if (!_targetReviver.CanBeRevived)
                {
                    _targetReviver = null;
                    return;
                }
                _targetReviver.OnReviveStarted();
            }

            if (!revive && _targetReviver)
            {
                _targetReviver.OnReviveCancelled();
                _targetReviver = null;
                return;
            }
            
            if(!_targetReviver)
                return;

            if (Vector3.Distance(transform.position, _targetReviver.transform.position) > playerDetectionRadius)
            {
                _targetReviver.OnReviveCancelled();
                _targetReviver = null;
                return;
            }
            
            _targetReviver.OnReviveUpdate(deltaTime);
            
            if (!_targetReviver.CanBeRevived)
                _targetReviver = null;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlaySFX(bool play)
        {
            if(play)
                spatialAudioController.PlayRevive();
            else
            {
                spatialAudioController.StopRevive();
            }
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_UpdateAnim(bool downed)
        {
            anim.Animator.SetBool("Downed", downed);
        }
        
        private void OnReviveStarted()
        {
            ReviveTimeLeft = timeToRevive;
            Reviving = true;

            if (!Runner.IsServer)
                RPC_PlaySFX(true);
        }
        
        private void OnReviveUpdate(float deltaTime)
        {
            if(!Reviving)
                return;
            ReviveTimeLeft -= deltaTime;
            if (ReviveTimeLeft <= 0 && _myHealthComp.HealthDepleted)
            {
                _myHealthComp.SetHealth(_myHealthComp.MaxHealth);
                RPC_UpdateAnim(false);
                OnReviveCancelled();
            }
        }

        private void OnReviveCancelled()
        {
            Reviving = false;
            ReviveTimeLeft = -1;
            if (!Runner.IsServer)
                RPC_PlaySFX(false);
        }
        
        private PlayerReviver GetOtherPlayerReviver()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, playerDetectionRadius, layerMask);
            var nos = new List<NetworkObject>();
            var myNo = transform.parent.GetComponent<NetworkObject>();
            if (colliders.Length <= 0)
                return null;
            
            foreach (var col in colliders)
            {
                if(!col.tag.Equals("Player"))
                    continue;
                var no = col.GetComponent<NetworkObject>();
                if(no == null)
                    continue;
                if(no.Equals(myNo))
                    continue;
                nos.Add(no);
            }

            if (nos.Count <= 0)
                return null;
            
            var reviver = nos[0].GetComponentInChildren<PlayerReviver>();
            return reviver;
        }

        private void OnDrawGizmos()
        {
            var color = Color.cyan;
            color.a = 0.15f;
            Gizmos.color = color;
            Gizmos.DrawSphere(transform.position, playerDetectionRadius);
        }

        private void Update()
        {
            if(Runner == null)
                return;
            
            reviveGUI.SetActive(Reviving);
            reviveImg.fillAmount = (timeToRevive - ReviveTimeLeft) / timeToRevive;
        }
    }
}