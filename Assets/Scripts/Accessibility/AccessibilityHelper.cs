using System;
using RuntimeDeveloperConsole;
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

            if(ColorBlindAssist.Instance == null)
                return;

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

        [ConsoleCommand("update high contrast accessibility", "0 - off, 1 - on")]
        public static void Acc_HC(String[] args)
        {
            if(args.Length < 1)
                return;
            switch (args[0])
            {
                case "0": SetAccessibilityType(AccessibilityType.None);
                            break;
                case "1": SetAccessibilityType(AccessibilityType.HighContrast);
                    break;
            }
        }
        
        [ConsoleCommand("update color filter", "0 - off, 1 - protanopia\n" +
                                               "\t,2 - protanomaly, 3 - deuteranopia, 4 - deuteranomaly\n" +
                                               "\t,5 - tritanopia, 6 - tritanomaly, 7 - achromatopsia\n" +
                                               "\t,8 - Achromatomaly")]
        public static void Acc_Cf(String[] args)
        {
            if(args.Length < 1)
                return;
            switch (args[0])
            {
                case "0": SetColorBlindFilterType(ColorblindTypes.Normal);
                    break;
                case "1": SetColorBlindFilterType(ColorblindTypes.Protanopia);
                    break;
                case "2": SetColorBlindFilterType(ColorblindTypes.Protanomaly);
                    break;
                case "3": SetColorBlindFilterType(ColorblindTypes.Deuteranopia);
                    break;
                case "4": SetColorBlindFilterType(ColorblindTypes.Deuteranomaly);
                    break;
                case "5": SetColorBlindFilterType(ColorblindTypes.Tritanopia);
                    break;
                case "6": SetColorBlindFilterType(ColorblindTypes.Tritanomaly);
                    break;
                case "7": SetColorBlindFilterType(ColorblindTypes.Achromatopsia);
                    break;
                case "8": SetColorBlindFilterType(ColorblindTypes.Achromatomaly);
                    break;
            }
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