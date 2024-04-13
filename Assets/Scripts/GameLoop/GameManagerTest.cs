using NaughtyAttributes;
using UnityEngine;

namespace GameLoop
{
    public class GameManagerTest : MonoBehaviour
    {
        [Button("Test Win")]
        private void TestWin()
        {
            GameManager.instance.UpdateGameState(GameManager.GameState.Win);
        }
        
        [Button("Test Lose")]
        private void TestLose()
        {
            GameManager.instance.UpdateGameState(GameManager.GameState.Lost);
        }
        
        [Button("Test Boss Spawn")]
        private void TestBossSpawn()
        {
            GameManager.instance.UpdateGameState(GameManager.GameState.SpawnBoss);
        }
    }
}