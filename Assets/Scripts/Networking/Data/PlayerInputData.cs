using Fusion;
using UnityEngine;

namespace Networking.Data
{
    public struct PlayerInputData : INetworkInput
    {
        public Vector3 MoveDirection;
        public bool Jump;
        public bool Attack;
        public bool Interact;
        public void Poll()
        {
            //Todo: Update with Input logic later
            MoveDirection = Vector3.zero;
            MoveDirection += Vector3.forward * Input.GetAxis("Vertical");
            MoveDirection += Vector3.right * Input.GetAxis("Horizontal");
        }
    }
}