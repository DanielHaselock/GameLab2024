using Fusion;
using Networking.Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameLoop
{
    [CreateAssetMenu(menuName = "GameLabs/LevelData", fileName = "New Level Data")]
    public class LevelData : ScriptableObject
    {
        public string BGM;
        public string Ambiance;
        public LevelObjectives Objectives;
        public ScoringMap ScoringMap;
        public NetworkPrefabRef BossToSpawn;
        public string BossSFXKey = "";
        public string BossMusicKey = "";
        public bool AllowChargedAttacks = false;
        public Sprite ScoreUISprite;
        public RewardsMap Rewards;
        [FormerlySerializedAs("sceneIndx")] public int SceneIndx=2;
        [FormerlySerializedAs("levelTimeInSeconds")] public int LevelTimeInSeconds=180;
    }
}