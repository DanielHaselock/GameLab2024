using Fusion;
using UnityEngine;

namespace Networking.Data
{
    [CreateAssetMenu(menuName = "GameLabs/ScoreMap", fileName = "New Score Map")]
    public class ScoringMap : ScriptableObject
    {
        public SerializableDictionary<string, int> ScoreRefTable;
    }
}