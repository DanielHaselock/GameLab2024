using System.Collections;
using System.Collections.Generic;
using Accessibility;
using GameLoop;
using UnityEngine;
using UnityEngine.UI;
using static Accessibility.AccessibilityHelper;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] GameObject PauseMenuUI;
    [SerializeField] GameObject SettingsMenuUI;
    [SerializeField] Toggle highContrast;

    public void Return()
    {
        PauseMenuUI.SetActive(true);
        SettingsMenuUI.SetActive(false);
    }

    public void SetHighContrast(bool value)
    {
        if (value)
            SetAccessibilityType(AccessibilityType.HighContrast);

        else if (Type == AccessibilityType.HighContrast)
            SetAccessibilityType(AccessibilityType.None);
    }
}
