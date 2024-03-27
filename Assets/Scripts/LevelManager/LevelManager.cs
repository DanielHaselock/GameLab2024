using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LevelManager
{
    private enum LevelDifficulty
    {
        Default,
        Easy,
        Hard
    }
    
    public static List<ObjectiveData> Objectives { get; private set; }
    
    private static int currentLevel = 0;
    private static LevelDifficulty difficulty;
    public static void LoadLevel(int level)
    {
        currentLevel = level;
        LoadLevelObjectives();
    }

    private static void LoadLevelObjectives()
    {
        var objectivePath = $"LevelData/Level_{currentLevel.ToString()}/{difficulty.ToString()}";
        var lvlObjectives = Resources.Load<LevelObjectives>(objectivePath);
        if (lvlObjectives == null)
            Objectives = new List<ObjectiveData>();

        Objectives = new List<ObjectiveData>(lvlObjectives.objectives);
    }

    public static void LevelComplete(bool win, TimeSpan timeLeft)
    {
        // if lost or remaining time is about 10 sec
        if (!win || (win && timeLeft.TotalSeconds <= 10))
        {
            difficulty = LevelDifficulty.Easy;
            return;
        }
        
        // more than 2 minutes make it hard
        if (timeLeft.TotalSeconds >= 120)
        {
            difficulty = LevelDifficulty.Hard;
            return;
        }
        
        //keep at default difficulty
        difficulty = LevelDifficulty.Default;
    }
}
