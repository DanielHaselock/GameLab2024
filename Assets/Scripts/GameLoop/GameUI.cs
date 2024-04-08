using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using GameLoop;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [FormerlySerializedAs("_objectivesParent")] [SerializeField] private Transform objectivesParent;
    [FormerlySerializedAs("_objectiveBody")] [SerializeField] private GameObject objectiveBody;
    
    [SerializeField] private GameObject timerObject;
    [SerializeField] private TMPro.TMP_Text timerText;
    [SerializeField] private Image timerBar;

    [SerializeField] private GameObject scoreObject;
    [SerializeField] private Transform scoreTextParent;
    [SerializeField] private GameObject scoreText;

    [SerializeField] private GameObject loseUI;
    [SerializeField] private Button loseMainMenuBttn;
    
    [SerializeField] private GameObject winUI;
    [SerializeField] private Button winMainMenuBttn;
    [SerializeField] private Button winNextLevelBttn;
    
    private List<TMP_Text> _allObjectivesTexts;
    private Dictionary<int, TMP_Text> _scoreTexts;
    private Dictionary<int, string> _nicknameMap;

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

    private void Start()
    {
        loseMainMenuBttn.onClick.AddListener(() =>
        {
            OnMainMenuRequested?.Invoke();
        });
        
        winMainMenuBttn.onClick.AddListener(() =>
        {
            OnMainMenuRequested?.Invoke();
        });
        
        winNextLevelBttn.onClick.AddListener(() =>
        {
            OnNextLevelClicked?.Invoke();
        });
    }

    public void UpdateLevelObjectives(Dictionary<string, Objective> map)
    {
        if (_allObjectivesTexts == null)
        {
            _allObjectivesTexts = new List<TMP_Text>();
        }

        foreach (var objectiveText in _allObjectivesTexts)
        {
            Destroy(objectiveText.gameObject);
        }
        _allObjectivesTexts.Clear();
        
        foreach (var data in map)
        {
            var go = Instantiate(objectiveBody, objectivesParent.transform);
            go.gameObject.SetActive(true);
            var str = $"{data.Value.ObjectiveString} ({data.Value.Current.ToString()}/{data.Value.Target.ToString()})";
            var txt = go.GetComponentInChildren<TMP_Text>();
            _allObjectivesTexts.Add(txt);
            txt.text = str;
            if (data.Value.IsCompleted)
            {
                txt.text = $"<s>{txt.text}</s>";
            }
        }
    }

    public void ShowGameTimer(bool show)
    {
        timerObject.SetActive(show);
    }
    
    public void UpdateTimerText(TimeSpan timeSpan)
    {
        var min = timeSpan.Minutes;
        var sec = timeSpan.Seconds;
        timerText.text = $"{min:00}:{sec:00}";
        timerBar.fillAmount = (float)timeSpan.TotalSeconds / LevelManager.LevelTime;
    }

    public void InitialiseScores(Dictionary<int,int> scoreMap, Dictionary<int, string> nameMap)
    {
        _nicknameMap = nameMap;
        _scoreTexts = new Dictionary<int, TMP_Text>();
        foreach (var kv in scoreMap)
        {
            var go = Instantiate(scoreText, scoreTextParent);
            go.SetActive(true);
            var text = go.GetComponentInChildren<TMP_Text>();
            text.text = $"{nameMap[kv.Key]}: {kv.Value}";
            _scoreTexts.Add(kv.Key, text);
        }
    }

    public void UpdateScore(int id, int score)
    {
        if(_scoreTexts == null)
            return;
        _scoreTexts[id].text =  $"{_nicknameMap[id]}: {score}";
    }

    public void ShowLostGameUI(bool show)
    {
        loseUI.SetActive(show);
    }

    public void ShowWinGameUI(bool show, bool showNextButton)
    {
        winNextLevelBttn.gameObject.SetActive(showNextButton);
        winUI.SetActive(show);
    }
}
