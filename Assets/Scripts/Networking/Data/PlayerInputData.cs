using Fusion;
using UnityEngine;

namespace Networking.Data
{
    public struct PlayerInputData : INetworkInput
    {
        public Vector3 MoveDirection;
        public bool Revive;
        public bool Jump;
        public bool Attack;
        public bool ChargeAttack;
        public bool Throw;
       [Networked] public bool Interact { get => default; set { } }
        public bool Drop;


        public void RefreshInputs()
        {
           // MoveDirection = Vector3.zero; Movement is handled on it's own
            Jump = false;
            Attack = false;
            ChargeAttack = false;
            Interact = false;
            Drop = false;
            Throw = false;
        }
    }
}