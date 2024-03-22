using System.Collections.Generic;
using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Interactables
{
    public class PlayerPickupable : NetworkBehaviour
    {
        [Networked] public bool IsPickedUp { get; private set; }
        [Networked] public bool Thrown { get; private set; }
        
        [SerializeField] private int slotsNeeded;
        [FormerlySerializedAs("_throwMaxHeight")] [SerializeField] private float throwMaxHeight = 1f;
        [FormerlySerializedAs("_throwInterpolation")] [SerializeField] private AnimationCurve throwInterpolation;
        [SerializeField] private float interpolationDur=0.25f;
        
        private NetworkRigidbody3D _nrb;
        private Rigidbody _rb;
        private CharacterController _controller;
        private NetworkCharacterController _networkController;
        private NetworkObject _no;
        private HealthComponent _healthComponent;
        
        private Dictionary<NetworkObject, PlayerRef?> _inputAuthorithyMap;
        
        private float _throwTimeStep = 0;
        private Vector3 _throwStartPos;
        private Vector3 _throwMidPos;
        private Vector3 _throwFinalPos;

        private float _maxSpeedOg;
        private float _maxAccelOg;
        private float _evaluationStep ;
        
        public int SlotNeeded => slotsNeeded;
        public bool AllowInputs => !IsPickedUp && !Thrown;
        
        private void Start()
        {
            _healthComponent = GetComponentInChildren<HealthComponent>();
            _rb = GetComponent<Rigidbody>();
            _nrb = GetComponent<NetworkRigidbody3D>();
            _controller = GetComponent<CharacterController>();
            _networkController = GetComponent<NetworkCharacterController>();
            _no = GetComponent<NetworkObject>();

            if (!_networkController) return;
            
            _maxAccelOg = _networkController.acceleration;
            _maxSpeedOg = _networkController.maxSpeed;
        }
        
        public void PrepareForParenting(bool pickup)
        {
            if (_healthComponent)
            {
                _healthComponent.SetHealthDepleteStatus(!pickup);
            }
            
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

            foreach (var col in GetComponentsInChildren<Collider>())
            {
                col.enabled = !pickup;
            }
            
            IsPickedUp = pickup;
        }

        public override void Spawned()
        {
            base.Spawned();
            if (Runner.IsServer)
            {
                _inputAuthorithyMap = new Dictionary<NetworkObject, PlayerRef?>();
                foreach (var networkObject in GetComponentsInParent<NetworkObject>())
                {
                    if(!networkObject.HasInputAuthority)
                        _inputAuthorithyMap.Add(networkObject, null);
                    else
                        _inputAuthorithyMap.Add(networkObject, networkObject.InputAuthority);
                }
            }
        }

        public void OnParented(Vector3 localPosition, Quaternion localRotation)
        {
            var t = transform;
            t.localPosition = localPosition;
            t.localRotation = localRotation;
            RPC_ReplicateLocalPositionElsewhere(localPosition, localRotation);
            
            //modify the input authority
            // foreach (var kv in _inputAuthorithyMap)
            // {
            //     if(kv.Value == null)
            //         continue;
            //     
            //     if(IsPickedUp)
            //         kv.Key.RemoveInputAuthority();
            //     else
            //         kv.Key.AssignInputAuthority(kv.Value.Value);
            // }
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ReplicateLocalPositionElsewhere(Vector3 pos, Quaternion rot)
        {
            if(Runner.IsServer)
                return;
            var t = transform;
            t.localPosition = pos;
            t.localRotation = rot;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ReplicateControllerParenting(bool pickup)
        {
            if(Runner.IsServer)
                return;
            if (_networkController)
                _networkController.enabled = !pickup;
            
            if (!pickup)
            {
                if (_networkController)
                    _networkController.enabled = true;
                transform.SetParent(null);
                IsPickedUp = false;
                return;
            }

            var list = new List<PlayerRef>(Runner.ActivePlayers);
            list.RemoveAll(a => a.PlayerId.Equals(_no.InputAuthority.PlayerId));
            var otherPlayerName = $"Player_{list[0].PlayerId.ToString()}";
            var slot = GameObject.Find(otherPlayerName).transform.Find("PickupSlot");
            Debug.Log($"slot {slot.parent.name} My: Player_{Runner.LocalPlayer.PlayerId.ToString()}");
            transform.SetParent(slot);
            
            if (_controller)
            {
                _controller.excludeLayers = pickup ? LayerMask.NameToLayer("Default") : 0;
                _controller.radius = pickup ? 0 : 0.5f;
                _controller.enabled = !pickup;
            }
            Debug.Log("Parent Replication complete");
            IsPickedUp = true;
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
            var currPos = transform.position;
            _throwStartPos = currPos;
            _throwFinalPos = currPos + (dir * throwDist);
            _throwMidPos = (_throwStartPos + _throwFinalPos) / 2;
            _throwMidPos.y = throwMaxHeight;
            _throwTimeStep = 0;
            
            Thrown = true;
        }
        
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
                var pos1 = Vector3.Lerp(_throwStartPos, _throwMidPos, _throwTimeStep);
                var pos2 = Vector3.Lerp(_throwMidPos, _throwFinalPos, _throwTimeStep);
                
                var estimated = Vector3.Lerp(pos1, pos2, _throwTimeStep);
                if(_nrb)
                    _rb.MovePosition(estimated);
                else if(_controller)
                    _controller.transform.position = estimated;
                
                var eval = throwInterpolation.Evaluate(_evaluationStep);
                _evaluationStep += Runner.DeltaTime / interpolationDur;
                _throwTimeStep += (Runner.DeltaTime / interpolationDur) * eval;
            }
        }
    }
}