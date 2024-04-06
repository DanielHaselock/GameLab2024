using System;
using SOHNE.Accessibility.Colorblindness;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Accessibility
{
    public static class AccessibilityHelper
    {
        private const string KEY = "VisionAssist";
        
        public enum AccessibilityType
        {
            None=0,
            ColorBlindAssist,
            HighContrast
        }
        
        public static Action OnAccessibilitySettingChanged;
        private static AccessibilityType _type;

        public static AccessibilityType Type => _type;

        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            SetAccessibilityType((AccessibilityType)PlayerPrefs.GetInt(KEY, 0));
        }
        
        public static void SetAccessibilityType(AccessibilityType type)
        {
            _type = type;
            OnAccessibilitySettingChanged?.Invoke();
            PlayerPrefs.SetInt(KEY, (int)_type);
            if (type != AccessibilityType.ColorBlindAssist)
                ColorBlindAssist.Instance.Type = ColorblindTypes.Normal;
        }
        
        public static void SetColorBlindFilterType(ColorblindTypes type)
        {
            _type = AccessibilityType.ColorBlindAssist;
            OnAccessibilitySettingChanged?.Invoke();
            PlayerPrefs.SetInt(KEY, (int)_type);
            ColorBlindAssist.Instance.Type = type;
        }
        
#if UNITY_EDITOR
        [MenuItem("GameLabs/Accessibility/Vision High Contrast")]
        public static void TestHC()
        {
            SetAccessibilityType(AccessibilityType.HighContrast);
        }
        
        [MenuItem("GameLabs/Accessibility/Vision Reset")]
        public static void Reset()
        {
            SetAccessibilityType(AccessibilityType.None);
        }
        
        [MenuItem("GameLabs/Accessibility/Color Blind Filter")]
        public static void TestColorBlindFilter()
        {
            SetColorBlindFilterType(ColorblindTypes.Achromatopsia);
        }
#endif
    }
}