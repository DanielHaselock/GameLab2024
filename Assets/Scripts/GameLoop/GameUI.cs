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
        Debug.Log("Hide Called");
        foreach (var objectiveText in _allObjectivesTexts)
        {
            Destroy(objectiveText.transform.parent.gameObject);
        }

        _scoreTexts.Clear();
        
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
        objectivesParent.gameObject.SetActive(true);
        foreach (var data in map)
        {
            Debug.Log($"DATA {data.Key}");
            var go = Instantiate(objectiveBody, objectivesParent.transform);
            go.gameObject.SetActive(true);
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
        if (_scoreTexts == null)
            _scoreTexts = new Dictionary<int, TMP_Text>();
 
        var playerId = NetworkManager.Instance.GetLocalPlayer().InputAuthority.PlayerId;

        _scoreTexts.Clear();
        for (int i = 1; i < scoreTextParent.childCount; i++)
        {
            Destroy(scoreTextParent.GetChild(i).gameObject);
        }
        
        foreach (var kv in scoreMap)
        {
            TMP_Text text = null;
            if (kv.Key == playerId)
            {
                var go = Instantiate(scoreText, scoreTextParent);
                go.SetActive(true);
                text = go.GetComponentInChildren<TMP_Text>();
                text.text = $"{kv.Value}";
            }
            _scoreTexts.Add(kv.Key, text);
        }

        StartCoroutine(RebuildThisStupidLayout());
        
        var image = scoreTextParent.GetComponent<Image>();
        image.sprite = LevelManager.ScoreUISprite;
        image.preserveAspect = true;
    }

    IEnumerator RebuildThisStupidLayout()
    {
        //I swear if this doesn't solve it X_X
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate(scoreTextParent.GetComponent<RectTransform>());
    }
    
    public void UpdateScore(int id, int score)
    {
        if(_scoreTexts == null || _scoreTexts[id] == null)
            return;
        _scoreTexts[id].text =  $"{score}";
    }

    public void ShowLostGameUI(bool show)
    {
        if(show)
            HideObjectives();
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        SetBossHealth(false, 0);
        roundOverUI.gameObject.SetActive(show);
        roundOverUI.ShowEndScreen(false, timerBar.fillAmount, false);
    }

    public void ShowWinGameUI(bool show, bool showNextButton, RewardData upgradeGiven=null)
    {
        if(show)
            HideObjectives();
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        SetBossHealth(false, 0);
        roundOverUI.gameObject.SetActive(show);
        upgradesImage.sprite = upgradeGiven == null ? null : upgradeGiven.Ico;
        roundOverUI.ShowEndScreen(show, timerBar.fillAmount,showNextButton);
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
        Debug.Log(_scoreTexts);
        var txt = _scoreTexts.GetValueOrDefault(NetworkManager.Instance.GetLocalPlayer().InputAuthority.PlayerId);
        return txt == null? "" : txt.text;
    }

    public void SetBossHealth(bool show, float val)
    {
        bossHealthbar.SetActive(show);
        bossHPFill.fillAmount = val;
    }

    public void ShakeBossHealthBar()
    {
        bossHPShaker.Shake();
    }
}
