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
    [Networked] public bool IsPaused { get; set; }
    public static event Action<GameState> OnGameStateChanged;
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
       // UpdateGameState(GameState.MainMenu);
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
                SceneManager.LoadScene("Network Test 1");
                break;
            case GameState.ActiveRound:
                break;
            case GameState.Settings:
                break;
            case GameState.BetweenRounds:
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
        ActiveRound,
        BossRound,
        Settings,
        BetweenRounds,
        GameOver,
        Win
    }
}
