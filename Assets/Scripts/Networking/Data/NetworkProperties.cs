using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking.Data
{
    [CreateAssetMenu(fileName = "NetworkProperties", menuName = "GameLabs/Networking/Create Network Properties SO")]
    public class NetworkProperties : ScriptableObject
    {
        public int GameSceneIndex;
        public NetworkPrefabRef PlayerPrefab;
        public int MaxPlayers=2;
    }
}