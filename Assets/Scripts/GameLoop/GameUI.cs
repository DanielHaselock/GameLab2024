using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Audio;
using Fusion;
using GameLoop;
using Networking.Behaviours;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.Video;
using static UnityEditor.Experimental.GraphView.GraphView;

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

    [SerializeField] private RoundOverUI roundOverUI;
    [SerializeField] private Button loseMainMenuBttn;
    [SerializeField] private Button winNextLevelBttn;

    [SerializeField] private VideoPlayer cutscenePlayer;
    [SerializeField] private GameObject cutSceneObj;
    
    private List<TMP_Text> _allObjectivesTexts;
    private Dictionary<int, TMP_Text> _scoreTexts;
    private Dictionary<int, string> _nicknameMap;

    private Action OnMainMenuRequested;
    private Action OnNextLevelClicked;

    public Action OnCutsceneCompleted;

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
            Destroy(objectiveText.gameObject.transform.parent.gameObject);
        }
        _allObjectivesTexts.Clear();
        
        foreach (var data in map)
        {
            var go = Instantiate(objectiveBody, objectivesParent.transform);
            go.gameObject.SetActive(true);
            var str = $"{data.Value.Current.ToString()}/{data.Value.Target.ToString()}";
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
        var players = FindObjectsOfType<Player>().ToList();
        string playerNickname = null;

        foreach (var player in players)
        {
            if (player.HasInputAuthority)
            {
                playerNickname = NetworkManager.Instance.GetPlayerNickNameById(player.PlayerId);
                break;
            }
        }

        foreach (var kv in scoreMap)
        {
            TMP_Text text = null;
            if (nameMap[kv.Key].Equals(playerNickname))
            {
                var go = Instantiate(scoreText, scoreTextParent);
                go.SetActive(true);
                text = go.GetComponentInChildren<TMP_Text>();
                text.text = $"{kv.Value}";
            }
            _scoreTexts.Add(kv.Key, text);
        }
    }

    public void UpdateScore(int id, int score)
    {
        if(_scoreTexts == null || _scoreTexts[id] == null)
            return;
        _scoreTexts[id].text =  $"{score}";
    }

    public void ShowLostGameUI(bool show)
    {
        roundOverUI.gameObject.SetActive(show);
        roundOverUI.ShowEndScreen(false, timerBar.fillAmount);
    }

    public void ShowWinGameUI(bool show, bool showNextButton)
    {
        winNextLevelBttn.gameObject.SetActive(showNextButton);
        roundOverUI.gameObject.SetActive(show);
        roundOverUI.ShowEndScreen(true, timerBar.fillAmount);
    }

    public void PlayCutscene()
    {
        cutSceneObj.SetActive(true);
        AudioManager.Instance.MuteBGAndAmbiance(true);
        cutscenePlayer.Play();
        StartCoroutine(TrackCutscenePlayback());
    }

    public void HideCutscenePlayer()
    {
        cutSceneObj.SetActive(false);
    }
    
    IEnumerator TrackCutscenePlayback()
    {
        yield return new WaitForSeconds((int)cutscenePlayer.length + 1);
        AudioManager.Instance.MuteBGAndAmbiance(false);
        OnCutsceneCompleted?.Invoke();
    }

    public string GetCurrentPlayerScore()
    {
        var players = FindObjectsOfType<Player>().ToList();
        string playerNickname = null;
        foreach (var player in players)
        {
            if (player.HasInputAuthority)
            {
                playerNickname = NetworkManager.Instance.GetPlayerNickNameById(player.PlayerId);
                break;
            }
        }

        if (playerNickname == null)
            return null;

        int i = -1;
        foreach (var name in _nicknameMap)
        {
            if (name.Value == playerNickname)
            {
                i = name.Key;
                break;
            }
        }

        if (i == -1)
            return null;

        return _scoreTexts.GetValueOrDefault(i).text;
    }
}
