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
            if (GetInput(out NetworkInputData data))
            {
                _controller.Move(data.direction.normalized*3*Runner.DeltaTime);
            }
        }
    }
}