using Fusion;
using UnityEngine;

namespace Networking.Data
{
    [System.Serializable]
    public class SpawnData
    {
        public NetworkPrefabRef ObjectToSpawn;
        public Vector3 Position;
        public Vector3 Rotation;
    }
}