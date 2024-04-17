using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class RoundOverUI : MonoBehaviour
{
    [SerializeField] private GameUI gameUI;
    [SerializeField] private GameObject winUI, loseUI;

    [SerializeField] private TMPro.TMP_Text titleText;
    [SerializeField] private Image timeLeftBar;
    [SerializeField] private TMPro.TMP_Text scoreText;
    [SerializeField] private Image difficultyBar;

    [SerializeField] private Button loseMainMenuBttn;
    [SerializeField] private Button winNextLevelBttn;

    private Action OnMainMenuRequested;
    private Action OnNextLevelClicked;

    public void RegisterLoadToMainMenu(Action action)
    {
        OnMainMenuRequested += action;
    }

    public void DeRegisterLoadToMainMenu(Action action)
    {
        OnMainMenuRequested -= action;
    }

    public void RegisterLoadNextLevel(Action action)
    {
        OnNextLevelClicked += action;
    }

    public void DeRegisterLoadNextLevel(Action action)
    {
        OnNextLevelClicked -= action;
    }

    public void ShowEndScreen(bool win, float timeLeftFillAmount, bool showNext)
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        winNextLevelBttn.gameObject.SetActive(showNext);
        
        winUI.SetActive(win);
        loseUI.SetActive(!win);
        timeLeftBar.fillAmount = timeLeftFillAmount;
        
        if(win)
            scoreText.text = gameUI.GetCurrentPlayerScore();

        if (win)
        {
            titleText.text = "WIN<size=120>!</size>";
            difficultyBar.fillAmount = (float)LevelManager.Difficulty / 2.0f;

            winNextLevelBttn.onClick.AddListener(() =>
            {
                OnNextLevelClicked?.Invoke();
            });
        } else
        {
            titleText.text = "GAME OVER";

            loseMainMenuBttn.onClick.AddListener(() =>
            {
                OnMainMenuRequested?.Invoke();
            });
        }
    }
}
