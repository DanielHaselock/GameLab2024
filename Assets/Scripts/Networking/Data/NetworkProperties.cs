using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Networking.Data
{
    [CreateAssetMenu(fileName = "NetworkProperties", menuName = "GameLabs/Networking/Create Network Properties SO")]
    public class NetworkProperties : ScriptableObject
    {
        public int MaxPlayers=2;
        public List<NetworkPrefabRef> PlayerPrefabs;
        public List<SpawnData> NetworkObjectsToSpawnOnGameStart;

    }
}