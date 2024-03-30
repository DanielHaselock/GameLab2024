using Fusion;
using Networking.Data;
using UnityEngine;

namespace GameLoop
{
    [CreateAssetMenu(menuName = "GameLabs/LevelData", fileName = "New Level Data")]
    public class LevelData : ScriptableObject
    {
        public LevelObjectives Objectives;
        public ScoringMap ScoringMap;
        public NetworkPrefabRef BossToSpawn;
    }
}