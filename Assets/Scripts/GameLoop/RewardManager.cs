using System.Collections.Generic;
using UnityEngine;

namespace GameLoop
{
    public class RewardManager
    {
        public Dictionary<int, int> UpgradesMap { get; private set; }

        private RewardsMap _rewardsMap;
        
        public RewardManager()
        {
            UpgradesMap = new Dictionary<int, int>();
        }
        
        public int GetRewardIndex(int playerId)
        {
            if(!UpgradesMap.ContainsKey(playerId))
                UpgradesMap.Add(playerId, 0);
            return UpgradesMap[playerId];
        }
        
        public void Calculate(Dictionary<int, int> scores, RewardsMap rewardsMap)
        {
            Debug.Log("Calculate Goal");
        }
    }
}