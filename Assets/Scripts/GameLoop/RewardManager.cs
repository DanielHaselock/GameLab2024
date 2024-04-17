using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace GameLoop
{
    public static class RewardManager
    {
        public static Dictionary<int, RewardData> UpgradesMap { get; private set; } = new Dictionary<int, RewardData>();
        
        public static RewardData GetRewardDataForPlayer(int playerId)
        {
            if (!UpgradesMap.ContainsKey(playerId))
                return null;

            return UpgradesMap[playerId];
        }
        
        public static int GetRewardIndex(int playerId)
        {
            if (!UpgradesMap.ContainsKey(playerId))
                return 0;
            return UpgradesMap[playerId].WeaponIndex;
        }
        
        public static void Calculate(Dictionary<int, int> scores, RewardsMap rewardsMap)
        {
            var decSortRewards = rewardsMap.Rewards.OrderByDescending((a) => a.MinScoreNeeded);
            var def = rewardsMap.Rewards.OrderBy((a) => a.MinScoreNeeded).ToArray()[0];
            foreach (var kv in scores)
            {
                if (!UpgradesMap.ContainsKey(kv.Key))
                    UpgradesMap.Add(kv.Key, def);
                Debug.Log("SCORE SHIT " + kv.Key + ": " + kv.Value);
                foreach (var reward in decSortRewards)
                {
                    if (kv.Value >= reward.MinScoreNeeded)
                    {
                        Debug.Log("SCORE SHIT " + reward.MinScoreNeeded);
                        UpgradesMap[kv.Key] = reward; 
                        break;
                    }
                }
            }
        }

        public static void Cleanup()
        {
            UpgradesMap.Clear();
        }
    }
}