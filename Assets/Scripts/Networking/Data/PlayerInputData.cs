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


        public void RefreshInputs()
        {
            MoveDirection = Vector3.zero;
            Jump = false;
            Attack = false;
            Interact = false;
        }
    }
}