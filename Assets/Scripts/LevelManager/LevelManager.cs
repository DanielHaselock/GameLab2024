using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using GameLoop;
using Networking.Data;
using UnityEngine;

public static class LevelManager
{
    private enum LevelDifficulty
    {
        Default,
        Easy,
        Hard
    }
    
    private static int currentLevel = 0;
    private static LevelDifficulty difficulty;
    private static LevelData _data;
    private static string LevelDataPathDefault => $"LevelDatas/Level_{currentLevel.ToString()}/{difficulty.ToString()}/LevelData";
    
    public static string LevelDataPath => $"LevelDatas/Level_{currentLevel.ToString()}/{difficulty.ToString()}/LevelData";
    public static Dictionary<string, int> ScoreMap { get; private set; }
    public static List<ObjectiveData> Objectives { get; private set; }
    public static NetworkPrefabRef BossToSpawn => _data == null ? default : _data.BossToSpawn;

    public static int LevelSceneIndx => _data == null ? 1 : _data.SceneIndx;
    public static int LevelTime => _data == null ? 180 : _data.LevelTimeInSeconds;
    
    public static void LoadLevel(int level)
    {
        currentLevel = level;
        Debug.Log($"Loading Level Data {level} @ {LevelDataPath}");
        LoadLevelObjectives();
    }

    private static void LoadLevelObjectives()
    {
        var objectivePath = LevelDataPath;
        var lvlData = Resources.Load<LevelData>(objectivePath);
        if (lvlData == null)
        {
            Objectives = new List<ObjectiveData>();
            ScoreMap = new Dictionary<string, int>();
        }
        ScoreMap = new Dictionary<string, int>();
        foreach (var kv in lvlData.ScoringMap.ScoreRefTable)
        {
            ScoreMap.Add(kv.Key, kv.Value);
        }
        Objectives = new List<ObjectiveData>(lvlData.Objectives.objectives);
    }

    public static void LoadLevelObjectivesFrom(string path)
    {
        var lvlData = Resources.Load<LevelData>(path);
        if (lvlData == null)
        {
            Objectives = new List<ObjectiveData>();
            ScoreMap = new Dictionary<string, int>();
        }
        ScoreMap = new Dictionary<string, int>();
        foreach (var kv in lvlData.ScoringMap.ScoreRefTable)
        {
            ScoreMap.Add(kv.Key, kv.Value);
        }
        Objectives = new List<ObjectiveData>(lvlData.Objectives.objectives);
    }
    
    public static void LevelComplete(bool win, TimeSpan timeLeft)
    {
        _data = null;
        ScoreMap.Clear();
        Objectives.Clear();
        
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
