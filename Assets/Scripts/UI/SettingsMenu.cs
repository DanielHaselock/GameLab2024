using System.Collections;
using System.Collections.Generic;
using GameLoop;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField]
    GameObject PauseMenuUI;
    [SerializeField]
    GameObject SettingsMenuUI;

    public void Return()
    {
        PauseMenuUI.SetActive(true);
        SettingsMenuUI.SetActive(false);
    }
    public void MainMenu()
    {
        GameManager.instance.UpdateGameState(GameManager.GameState.MainMenu);
    }
}
