using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

namespace Interactables
{
    public class PlayerPickupable : NetworkBehaviour
    {
        [Networked] public bool IsPickedUp { get; private set; }
        [Networked] public bool Thrown { get; private set; }
        
        [SerializeField] private int slotsNeeded;
        [SerializeField] private float _throwMaxHeight = 1f;
        [SerializeField] private AnimationCurve _throwInterpolation;
        [SerializeField] private float interpolationDur=0.25f;
        
        private NetworkRigidbody3D _nrb;
        private Rigidbody _rb;
        private CharacterController _controller;
        private NetworkCharacterController _networkController;
        private NetworkObject _no;

       
        private float _throwTimeStep = 0;
        private Vector3 _throwStartPos;
        private Vector3 _throwMidPos;
        private Vector3 _throwFinalPos;

        private float _maxSpeedOg;
        private float _maxAccelOg;
        
        public int SlotNeeded => slotsNeeded;
        public bool AllowInputs => !IsPickedUp && !Thrown;
        
        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _nrb = GetComponent<NetworkRigidbody3D>();
            _controller = GetComponent<CharacterController>();
            _networkController = GetComponent<NetworkCharacterController>();
            _no = GetComponent<NetworkObject>();

            if (_networkController)
            {
                _maxAccelOg = _networkController.acceleration;
                _maxSpeedOg = _networkController.maxSpeed;
            }
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

        public void Throw(Vector3 dir, float throwDist)
        {
            if (_nrb)
                _nrb.RBIsKinematic = true;
           
            _evaluationStep = 0;
            _throwStartPos = transform.position;
            _throwFinalPos = transform.position + (dir * throwDist);
            _throwMidPos = (_throwStartPos + _throwFinalPos) / 2;
            _throwMidPos.y = _throwMaxHeight;
            _throwTimeStep = 0;
            
            Thrown = true;
        }

        private float _evaluationStep = 0;
        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            if(!Runner.IsServer)
                return;
            if (Thrown)
            {
                if (_throwTimeStep > 1)
                {
                    Thrown = false;
                    if (_rb)
                        _rb.isKinematic = false;
                    return;
                }

                var currPos = transform.position;
                var pos1 = Vector3.Lerp(_throwStartPos, _throwMidPos, _throwTimeStep);
                var pos2 = Vector3.Lerp(_throwMidPos, _throwFinalPos, _throwTimeStep);
                
                var estimated = Vector3.Lerp(pos1, pos2, _throwTimeStep);
                var dir = (estimated - currPos).normalized;
                
                if(_nrb)
                    _rb.MovePosition(estimated);
                else if(_controller)
                    _controller.transform.position = estimated;
                
                var eval = _throwInterpolation.Evaluate(_evaluationStep);
                _evaluationStep += Runner.DeltaTime / interpolationDur;
                _throwTimeStep += (Runner.DeltaTime / interpolationDur) * eval;
            }
        }
    }
}