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

    public void ShowEndScreen(bool win, float timeLeftFillAmount)
    {
        winUI.SetActive(win);
        loseUI.SetActive(!win);
        timeLeftBar.fillAmount = timeLeftFillAmount;
        scoreText.text = gameUI.GetCurrentPlayerScore();

        if (win)
        {
            titleText.text = "WIN<size=120>!</size>";
        } else
        {
            titleText.text = "GAME OVER";
        }
    }
}
