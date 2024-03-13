using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

namespace Interactables
{
    public class PlayerPickupable : NetworkBehaviour
    {
        [SerializeField] private int slotsNeeded;
        [SerializeField] private float _ccImpulseMult = 5;
        private NetworkRigidbody3D _nrb;
        private Rigidbody _rb;
        private CharacterController _controller;
        private NetworkCharacterController _networkController;
        private NetworkObject _no;
        public int SlotNeeded => slotsNeeded;
        [Networked] public bool IsPickedUp { get; set; }

        private bool _thrown;
        private float _throwForce;
        private Vector3 _throwDir;

        public bool AllowInputs => !IsPickedUp && !_thrown;
        
        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _nrb = GetComponent<NetworkRigidbody3D>();
            _controller = GetComponent<CharacterController>();
            _networkController = GetComponent<NetworkCharacterController>();
            _no = GetComponent<NetworkObject>();
            
        }
        
        public void PrepareForParenting(bool pickup)
        {
            if (_networkController)
            {
                // special sceanrio, when a pickupable has a network controller attached....
                _networkController.enabled = !pickup;
                _controller.excludeLayers = pickup ? LayerMask.NameToLayer("Default") : 0;
                _controller.radius = pickup ? 0 : 0.5f;
                _controller.enabled = !pickup;
                RPC_ReplicateControllerParenting(pickup);
            }
            
            if (_rb)
            {
                _rb.isKinematic = pickup;
            }

            foreach (var collider in GetComponentsInChildren<Collider>())
            {
                collider.enabled = !pickup;
            }

            IsPickedUp = pickup;
        }

        public void OnParented(Vector3 localPosition, Quaternion localRotation)
        {
            transform.localPosition = localPosition;
            transform.localRotation = localRotation;
            RPC_ReplicateLocalPositionElsewhere(localPosition, localRotation);
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ReplicateLocalPositionElsewhere(Vector3 pos, Quaternion rot)
        {
            if(Runner.IsServer)
                return;
            Debug.Log("OnParent RPC recieved");
            transform.localPosition = pos;
            transform.localRotation = rot;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ReplicateControllerParenting(bool pickup)
        {
            if(Runner.IsServer)
                return;
            Debug.Log("Parenting RPC recieved");
            if (_networkController)
                _networkController.enabled = !pickup;
            
            if (!pickup)
            {
                if (_networkController)
                    _networkController.enabled = true;
                transform.SetParent(null);
                return;
            }

            int id = 0;
            var list = new List<PlayerRef>(Runner.ActivePlayers);
            list.RemoveAll(a => a.PlayerId.Equals(_no.InputAuthority.PlayerId));
            var otherPlayerName = $"Player_{list[0].PlayerId}";
            var slot = GameObject.Find(otherPlayerName).transform.Find("PickupSlot");
            Debug.Log($"slot {slot.parent.name} My: Player_{Runner.LocalPlayer.PlayerId}");
            transform.SetParent(slot);
            
            if (_controller)
            {
                _controller.excludeLayers = pickup ? LayerMask.NameToLayer("Default") : 0;
                _controller.radius = pickup ? 0 : 0.5f;
                _controller.enabled = !pickup;
            }
            Debug.Log("Parent Replication complete");
            IsPickedUp = pickup;
        }
        
        public void Teleport(Vector3 pos, Vector3 velocity)
        {
            if (!_rb || !_nrb)
                return;
            _nrb.Teleport(pos);
            _nrb.ResetRigidbody();
            _rb.velocity = velocity;
        }

        public void Throw(Vector3 dir, float force)
        {
            if (_networkController)
            {
                _thrown = true;
                _throwDir = dir;
                _throwForce = force*_ccImpulseMult;
            }
            else if (_rb)
            {
                _rb.AddForce(dir*force,ForceMode.Impulse);   
            }
        }
        
        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            if(!Runner.IsServer)
                return;
            
            if (_thrown)
            {
                if (_throwForce <= 0.1f)
                {
                    _throwForce = 0;
                    _thrown = false;
                    return;
                }

                _networkController.Teleport(_networkController.transform.position + (_throwDir*_throwForce * Runner.DeltaTime));
                _throwForce = Mathf.Lerp(_throwForce, 0, 5 * Runner.DeltaTime);
            }
        }
    }
}