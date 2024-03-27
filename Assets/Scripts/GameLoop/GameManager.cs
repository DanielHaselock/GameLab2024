using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Networking.Data;
using UnityEngine.Events;
using Networking.Behaviours;
using System.Threading.Tasks;
using GameLoop;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    public GameState CurrentGameState { get; private set; }

    [Networked] public bool IsPaused { get; private set; }
    
    //checks if the player is allowed to pause (For example, cannot pause from main menu)
    [Networked] public bool IsPausable { get; private set; } 
    public static event Action<GameState> OnGameStateChanged;
    public event Action<bool> OnPauseStatusChanged;

    private ChangeDetector _change;
    [SerializeField]
    private Dictionary<string,Objective> objectivesMap = new Dictionary<string, Objective>();
    [Networked] public bool bossDefeated { get; private set; }
    [SerializeField] private int levelSceneIndex;
    [SerializeField] private SerializableDictionary<string, NetworkPrefabRef> enemyMap;

    private int currentLevel = 1;
    private TimeSpan timeLeft;

    private GameUI _gameUI;
    
    public override async void Spawned()
    {
        if(!Runner.IsServer)
            return;
        
        _change = GetChangeDetector(ChangeDetector.Source.SimulationState);
        NetworkManager.Instance.OnTimerTick += TimerTick;
        await Task.Delay(2000);
        UpdateGameState(GameState.ActiveLevel);
    }

    private void TimerTick(TimeSpan timeLeft)
    {
        this.timeLeft = timeLeft;
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    // Creates the Pause UI in scene
    void Start()
    {
        IsPausable = true;//for testing
        Instantiate(Resources.Load("GameUI"));
        Instantiate(Resources.Load("EventSystem"));
    }
    
    public void UpdateGameState(GameState newState)
    {
        CurrentGameState = newState;
        switch (newState)
        {
            case GameState.MainMenu:
                //place holder for now
                SceneManager.LoadScene("Network Test 1");
                break;
            case GameState.ActiveLevel:
                StartLevel();
                break;
            case GameState.BetweenLevels:
                //replace with win state only? 
                break;
            case GameState.GameOver:
                LevelManager.LevelComplete(false,timeLeft);
                break;
            case GameState.Win:
                LevelManager.LevelComplete(true, timeLeft);
                break;
            default:
                break;
        }
        OnGameStateChanged?.Invoke(newState);
    }

    //best practice = keeping these in seperate script
    public enum GameState
    {
        MainMenu,
        ActiveLevel,
        BetweenLevels,
        GameOver,
        Win
    }
    public override void Render()
    {
        base.Render();
        if(_change == null)
            return;
        
        foreach (var change in _change.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(IsPaused):
                    OnPauseStatusChanged?.Invoke(IsPaused);
                    break;
            }
        }
    }

    private void DoPause(bool pause)
    {   if(!IsPausable)
        {
            return;
        }

        IsPaused = pause;
    }
    public void Pause(bool pause)
    {
        Debug.Log($"Pause {pause}");
       if(Runner.IsServer)
        DoPause(pause);
       else
        RPC_Pause(pause);
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_Pause(bool pause)
    {
        DoPause(pause);
    }

    private async void StartLevel()
    {
        if(Runner==null)
            return;
        if (!Runner.IsServer)
        {
            Debug.Log("Cannot call StartLevel on client");
            return;
        }
        
        IsPausable = true;
        bool result= await NetworkManager.Instance.LoadSceneNetworked(levelSceneIndex, false);
        if (!result)
        {
            Debug.LogError("Failed to load level");
            return;
        }

        var defPos = new List<Vector3> { new Vector3(0, 2, 0), new Vector3(0, 4, 0) };
        var defRot = new List<Quaternion> { Quaternion.identity, Quaternion.identity };
        var spawnPositions = new List<Vector3>();
        var spawnRotations = new List<Quaternion>();

        foreach (var spawnpoint in FindObjectsOfType<PlayerSpawnPoint>())
        {
            spawnPositions.Add(spawnpoint.SpawnPosition);
            spawnRotations.Add(spawnpoint.SpawnRotation);
        }

        if (spawnPositions.Count < 0 || spawnRotations.Count < 0)
        {
            spawnPositions = defPos;
            spawnRotations = defRot;
        }
        
        NetworkManager.Instance.SpawnPlayers(spawnPositions, spawnRotations);
        Runner.Spawn(enemyMap["test"], Vector3.zero,Quaternion.identity);
        
        //change objectives
        LevelManager.LoadLevel(currentLevel);
        objectivesMap = new Dictionary<string, Objective>();
        foreach (var objectiveData in LevelManager.Objectives)
        {
            objectivesMap.Add(objectiveData.key, new Objective(objectiveData));
        }
        bossDefeated = false;
    }
    
    public void SpawnBoss() { }
    
    public void RaiseObjective(string key)
    {
        if(Runner==null)
            return;
        if (!Runner.IsServer) { return; }
        if (!objectivesMap.ContainsKey(key))
        {
            return;
        }
        Debug.Log("Objective Raised " + key);
        var objective = objectivesMap[key];
        objective.UpdateObjective();
        if (objective.IsCompleted)
        {
            objectivesMap.Remove(key);
        }

        TryUpdateGameState();
    }

    private void TryUpdateGameState()
    {
        
        if(objectivesMap.Count > 0 && !bossDefeated)
        {
           
        }
        else if (objectivesMap.Count == 0 && !bossDefeated)
        {
            SpawnBoss();
        }
      
        else
        {
            UpdateGameState(GameState.Win);
        }
    }

    //so main logic is as such, when an enemy dies,
    //it will update the objective. if it matches,
    //then the objective is updated. once objective is met,
    //it is removed from list. Once list is empty,
    //all objectives have been met and we can spawn the boss.
    //Once boss is defeated, triggers gamestate change and level ends.
}
