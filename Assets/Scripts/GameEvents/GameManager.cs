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

    //stores the variable so the event does not get called when no actual value change exists
    private bool isPaused;
    [Networked] public bool IsPaused
    {
        get { return isPaused; }
        set
        {
            if (isPaused != value)
            {
                isPaused = value;
                OnPauseStatusChanged?.Invoke(isPaused);
            }
        }
    }
    [Networked] public bool IsPausable { get; set; } //checks if the player is allowed to pause (For example, cannot pause from main menu)
    public static event Action<GameState> OnGameStateChanged;
    public event Action<bool> OnPauseStatusChanged;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Game Manager started");
        IsPausable = true;//for testing
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

    public enum GameState
    {
        MainMenu,
        ActiveLevel,
        BetweenLevels,
        GameOver,
        Win
    }
}
