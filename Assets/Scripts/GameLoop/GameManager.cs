using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Networking.Data;
using UnityEngine.Events;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    public GameState CurrentGameState { get; private set; }

    [Networked] public bool IsPaused { get; private set; }
    [Networked] public bool IsPausable { get; private set; } //checks if the player is allowed to pause (For example, cannot pause from main menu)
    public static event Action<GameState> OnGameStateChanged;
    public event Action<bool> OnPauseStatusChanged;

    private ChangeDetector _change;

    public override void Spawned()
    {
        _change = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }
        
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
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
                RunLevel();
                break;
            case GameState.BetweenLevels:
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

    private void RunLevel()
    {   //load level
        //spawn enemies
        IsPausable = true;
        //place holder for now
    }
}
