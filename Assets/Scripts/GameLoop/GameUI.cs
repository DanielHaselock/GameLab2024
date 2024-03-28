using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using GameLoop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [SerializeField] private Transform _objectivesParent;
    [SerializeField] private GameObject _objectiveBody;
    
    private List<TMPro.TMP_Text> _allObjectivesTexts;
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
            var go = Instantiate(_objectiveBody, _objectivesParent.transform);
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
}
