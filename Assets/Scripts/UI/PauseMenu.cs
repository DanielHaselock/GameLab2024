using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using static GameManager;

//interactable depends on who set pause,
public class PauseMenu : MonoBehaviour
{
    [SerializeField]
    GameObject PauseMenuUI;
    [SerializeField]
    GameObject SettingsMenuUI;

    public void Pause()
    {
        PauseMenuUI.SetActive(true);
    }
    public void Resume()
    {
        GameManager.instance.IsPaused = false;
        PauseMenuUI.SetActive(false);
    }
    public void Settings()
    {
        SettingsMenuUI.SetActive(true);
        PauseMenuUI.SetActive(false);
    }
    public void MainMenu()
    {
        GameManager.instance.UpdateGameState(GameManager.GameState.MainMenu);
    }
    // Start is called before the first frame update
    void Awake()
    {
        PauseMenuUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        GameManager.instance.OnPauseStatusChanged += HandlePauseStatusChanged;
    }

    void OnDisable()
    {
        GameManager.instance.OnPauseStatusChanged -= HandlePauseStatusChanged;
    }

    void HandlePauseStatusChanged(bool isPaused)
    {
        if (isPaused)
        {
            Pause();
        }
        else
        {
            Resume();
        }
    }

}
