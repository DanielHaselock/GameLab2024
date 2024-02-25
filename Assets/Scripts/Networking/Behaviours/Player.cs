using System;
using Cinemachine;
using Fusion;
using Networking.Data;
using Networking.Utils;
using UnityEngine;


namespace Networking.Behaviours
{
    public class Player : NetworkBehaviour
    {
        private NetworkCharacterController _controller;
        public bool HasInputAuthority { get; private set; }

        private void Awake()
        {
            _controller = GetComponent<NetworkCharacterController>();
        }


        public override void Spawned()
        {
            var no = GetComponent<NetworkObject>();
            if (no.InputAuthority == Runner.LocalPlayer)
            {
                HasInputAuthority = true;
                Debug.Log("Working");
                GetComponent<SetCamera>().SetCameraParams(gameObject);
            }
        }
        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();

            if (GetInput(out PlayerInputData data))
            {
                data.MoveDirection.Normalize();
                _controller.Move(3 * data.MoveDirection * Runner.DeltaTime);
            }
        }
    }
}