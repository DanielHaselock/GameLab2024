using System.Collections;
using System.Collections.Generic;
using Accessibility;
using GameLoop;
using SOHNE.Accessibility.Colorblindness;
using UnityEngine;
using UnityEngine.UI;
using static Accessibility.AccessibilityHelper;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] GameObject PauseMenuUI;
    [SerializeField] GameObject SettingsMenuUI;
    [SerializeField] Toggle HighContrastToggle;
    [SerializeField] Toggle ColorBlindToggle;
    [SerializeField] GameObject FiltersList;
    [SerializeField] TMPro.TMP_Text CurrentFilterText;

    private void Start()
    {
        HighContrastToggle.isOn = Type == AccessibilityType.HighContrast;
        ColorBlindToggle.isOn = Type == AccessibilityType.ColorBlindAssist;
        UpdateColorFilter((int)ColorBlindAssist.Instance.Type);
    }

    public void Return()
    {
        PauseMenuUI.SetActive(true);
        SettingsMenuUI.SetActive(false);
    }

    public void ToggleHighContrast(bool value)
    {
        if (value)
            SetAccessibilityType(AccessibilityType.HighContrast);

        else if (Type == AccessibilityType.HighContrast)
            SetAccessibilityType(AccessibilityType.None);
    }

    public void ToggleColorblindAssist(bool value)
    {
        if (value)
            SetAccessibilityType(AccessibilityType.ColorBlindAssist);

        else if (Type == AccessibilityType.ColorBlindAssist)
            SetAccessibilityType(AccessibilityType.None);

        FiltersList.SetActive(value);
    }

    public void IncrementColorFilter()
    {
        int currentFilter = (int)ColorBlindAssist.Instance.Type;
        currentFilter++;
        if (currentFilter > 8)
            currentFilter = 0;
        UpdateColorFilter(currentFilter);
    }
    
    public void DecrementColorFilter()
    {
        int currentFilter = (int)ColorBlindAssist.Instance.Type;
        currentFilter--;
        if (currentFilter < 0)
            currentFilter = 8;
        UpdateColorFilter(currentFilter);
    }

    private void UpdateColorFilter(int value)
    {
        ColorblindTypes newType = (ColorblindTypes)value;
        SetColorBlindFilterType(newType);
        CurrentFilterText.text = newType.ToString();
    }
}
