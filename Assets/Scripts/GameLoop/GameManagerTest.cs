using NaughtyAttributes;
using RuntimeDeveloperConsole;
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


        [ConsoleCommand("Update Game State", "0 - Win, 1 - Lost, 2 - SpawnBoss")]
        public static void UGS(string[] args)
        {
            GameManagerTest test = FindObjectOfType<GameManagerTest>();
            if(test == null)
                return;
            if(args.Length < 1)
                return;
            switch (args[0])
            {
                case "0": test.TestWin();
                    break;
                case "1": test.TestLose();
                    break;
                case "2": test.TestBossSpawn();
                    break;
            }
        }
    }
}