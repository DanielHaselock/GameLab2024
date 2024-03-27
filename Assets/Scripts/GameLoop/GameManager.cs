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

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    public GameState CurrentGameState { get; private set; }

    [Networked] public bool IsPaused { get; private set; }
    [Networked] public bool IsPausable { get; private set; } //checks if the player is allowed to pause (For example, cannot pause from main menu)
    public static event Action<GameState> OnGameStateChanged;
    public event Action<bool> OnPauseStatusChanged;

    private ChangeDetector _change;
    [SerializeField]
    private Dictionary<string,ObjectiveData> objectivesMap = new Dictionary<string, ObjectiveData>();
    [Networked] public bool bossDefeated { get; private set; }
    [SerializeField] private int levelSceneIndex;
    [SerializeField] private SerializableDictionary<string, NetworkPrefabRef> enemyMap;

    public override async void Spawned()
    {
        if(!Runner.IsServer)
            return;
        
        _change = GetChangeDetector(ChangeDetector.Source.SimulationState);
        await Task.Delay(2000);
        UpdateGameState(GameState.ActiveLevel);
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

    // Update is called once per frame
    void Update()
    {
        
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
                RunLevel();
                break;
            case GameState.BetweenLevels:
                //replace with win state only? 
                break;
            case GameState.GameOver:
                break;
            case GameState.Win:

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
        NetworkManager.Instance.SpawnPlayers(new List<Vector3> { new Vector3(0,2,0), new Vector3(0,4,0)}, new List<Quaternion> { Quaternion.identity,Quaternion.identity});
        Runner.Spawn(enemyMap["test"], Vector3.zero,Quaternion.identity);
        
        //change objectives
        ObjectiveData testObjective = ScriptableObject.CreateInstance<ObjectiveData>();
        testObjective.Initialize("OBLITERATE 1 enemy", "onion", 1, 0, ObjectiveData.OperationType.Sub);
        objectivesMap.Add("onion", testObjective);
        bossDefeated = false;
    }
    private void RunLevel()
    {   //while loop? Like while objectives not met and boss not defeated?
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
        if (objective.operationType == ObjectiveData.OperationType.Add)
        {
            objective.value += 1;
        }
        else
        {
            objective.value -= 1;
        }
        if (objective.value == objective.targetValue)
        {
            objectivesMap.Remove(key);
        }
      
    }

    //so main logic is as such, when an enemy dies, it will update the objective. if it matches, then the objective is updated. once objective is met, it is removed from list. Once list is empty, all objectives have been met and we can spawn the boss. Once boss is defeated, triggers gamestate change and level ends.
}
