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

    public void ShowEndScreen(bool win, float timeLeftFillAmount)
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        winUI.SetActive(win);
        loseUI.SetActive(!win);
        timeLeftBar.fillAmount = timeLeftFillAmount;
        scoreText.text = gameUI.GetCurrentPlayerScore();

        if (win)
        {
            titleText.text = "WIN<size=120>!</size>";

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
