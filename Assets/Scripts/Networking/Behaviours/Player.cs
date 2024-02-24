using System;
using Fusion;
using Networking.Data;

namespace Networking.Behaviours
{
    public class Player : NetworkBehaviour
    {
        private NetworkCharacterController _controller;

        private void Awake()
        {
            _controller = GetComponent<NetworkCharacterController>();
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            if (GetInput(out PlayerInputData data))
            {
                _controller.Move(data.MoveDirection.normalized*3*Runner.DeltaTime);
            }
        }
    }
}