using Fusion;
using Networking.Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameLoop
{
    [CreateAssetMenu(menuName = "GameLabs/LevelData", fileName = "New Level Data")]
    public class LevelData : ScriptableObject
    {
        public LevelObjectives Objectives;
        public ScoringMap ScoringMap;
        public NetworkPrefabRef BossToSpawn;
        public string BossSFXKey = "";
        public string BossMusicKey = "";
        public bool AllowChargedAttacks = false;
        [FormerlySerializedAs("sceneIndx")] public int SceneIndx=2;
        [FormerlySerializedAs("levelTimeInSeconds")] public int LevelTimeInSeconds=180;
    }
}