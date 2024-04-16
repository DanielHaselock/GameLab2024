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
    [SerializeField] private Image upgradesImage;

    [SerializeField] private VideoPlayer cutscenePlayer;
    [SerializeField] private GameObject cutSceneObj;

    [SerializeField] private GameObject bossHealthbar;
    [SerializeField] private Image bossHPFill;
    [SerializeField] private UIElementShaker bossHPShaker;
    
    private List<TMP_Text> _allObjectivesTexts;
    private Dictionary<int, TMP_Text> _scoreTexts;
    private Dictionary<int, string> _nicknameMap;
    private int _thisPlayerId;

    public Action OnCutsceneCompleted;

    public void RegisterLoadToMainMenu(Action action)
    {
        roundOverUI.RegisterLoadToMainMenu(action);
    }

    public void DeRegisterLoadToMainMenu(Action action)
    {
        roundOverUI.DeRegisterLoadToMainMenu(action);
    }

    public void RegisterLoadNextLevel(Action action)
    {
        roundOverUI.RegisterLoadNextLevel(action);
    }

    public void DeRegisterLoadNextLevel(Action action)
    {
        roundOverUI.DeRegisterLoadNextLevel(action);
    }

    public void HideObjectives()
    {
        _allObjectivesTexts.Clear();
        objectivesParent.gameObject.SetActive(false);
    }
    
    public void UpdateLevelObjectives(Dictionary<string, Objective> map)
    {
        if (_allObjectivesTexts == null)
        {
            _allObjectivesTexts = new List<TMP_Text>();
        }
        
        foreach (var objectiveText in _allObjectivesTexts)
        {
            Destroy(objectiveText.transform.parent.gameObject);
        }
        _allObjectivesTexts.Clear();
        
        foreach (var data in map)
        {
            var go = Instantiate(objectiveBody, objectivesParent.transform);
            var txt = go.GetComponentInChildren<TMP_Text>();
            var img = go.transform.Find("Image").GetComponent<Image>();

            var str = $"{data.Value.Current}/{data.Value.Target}";
            _allObjectivesTexts.Add(txt);
            txt.text = str;

            img.sprite = data.Value.Data.objectiveUISprite;

            if (data.Value.IsCompleted)
            {
                txt.text = $"<s>{txt.text}</s>";
                img.color = new Color(0.5f, 0.5f, 0.5f);
            }

            go.gameObject.SetActive(true);
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
        _thisPlayerId = -1;

        foreach (var player in players)
        {
            if (player.HasInputAuthority)
            {
                _thisPlayerId = player.PlayerId;
                break;
            }
        }

        foreach (var kv in scoreMap)
        {
            TMP_Text text = null;
            if (kv.Key == _thisPlayerId)
            {
                var go = Instantiate(scoreText, scoreTextParent);
                go.SetActive(true);
                text = go.GetComponentInChildren<TMP_Text>();
                text.text = $"{kv.Value}";
            }
            _scoreTexts.Add(kv.Key, text);
        }

        var image = scoreTextParent.GetComponent<Image>();
        image.sprite = LevelManager.ScoreUISprite;
        image.SetNativeSize();
    }

    public void UpdateScore(int id, int score)
    {
        if(_scoreTexts == null || _scoreTexts[id] == null)
            return;
        _scoreTexts[id].text =  $"{score}";
    }

    public void ShowLostGameUI(bool show)
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SetBossHealth(false, 0);
        roundOverUI.gameObject.SetActive(show);
        roundOverUI.ShowEndScreen(false, timerBar.fillAmount);
    }

    public void ShowWinGameUI(bool show, bool showNextButton, RewardData upgradeGiven=null)
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SetBossHealth(false, 0);
        roundOverUI.gameObject.SetActive(show);
        upgradesImage.sprite = upgradeGiven == null? null : upgradeGiven.Ico;
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
        return _scoreTexts.GetValueOrDefault(_thisPlayerId).text;
    }

    public void SetBossHealth(bool show, float val)
    {
        bossHealthbar.SetActive(show);
        bossHPFill.fillAmount = val;
        objectivesParent.gameObject.SetActive(false);
    }

    public void ShakeBossHealthBar()
    {
        bossHPShaker.Shake();
    }
}
