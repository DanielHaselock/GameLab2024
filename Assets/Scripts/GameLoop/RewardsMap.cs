using System.Collections.Generic;
using UnityEngine;

namespace GameLoop
{
    [CreateAssetMenu(menuName = "GameLabs/RewardMap", fileName = "New Rewards Map")]
    public class RewardsMap : ScriptableObject
    {
        public List<RewardData> Rewards;
    }
}