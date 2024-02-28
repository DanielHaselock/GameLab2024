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
        public bool Drop;


        public void RefreshInputs()
        {
           // MoveDirection = Vector3.zero; Movement is handled on it's own
            Jump = false;
            Attack = false;
            Interact = false;
            Drop = false;
        }
    }
}