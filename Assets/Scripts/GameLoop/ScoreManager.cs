using System.Collections.Generic;

namespace GameLoop
{
    public static class ScoreManager
    {
        public static Dictionary<int, int> Score { get; private set; }

        public static void Initialise(List<int> ids)
        {
            Score = new Dictionary<int, int>();
            foreach (var id in ids)
            {
                Score.Add(id,0);
            }
        }
        
        public static void UpdateScore(int id, int score)
        {
            if (Score.ContainsKey(id))
            {
                Score[id] += score;
            }
        }

        public static void Clear()
        {
            if(Score!=null)
                Score.Clear();
        }
    }
}