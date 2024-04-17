using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

namespace GameLoop
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField]
        GameObject PauseMenuUI;
        [SerializeField]
        GameObject SettingsMenuUI;
    
        public void Pause()
        {
            PauseMenuUI.SetActive(true);
            SettingsMenuUI.SetActive(false);
        }
        public void Resume()
        {
            GameManager.instance.Pause(false);
            SettingsMenuUI.SetActive(false);
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
            GameManager.instance.OnPauseStatusChanged += HandlePauseStatusChanged;
            PauseMenuUI.SetActive(false);
        }
    
    
        void OnDestroy()
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
}

