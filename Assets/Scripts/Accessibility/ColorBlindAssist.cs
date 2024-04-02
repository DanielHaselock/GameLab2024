using System;
using NaughtyAttributes;
using SOHNE.Accessibility.Colorblindness;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Utils;

namespace Accessibility
{
    public class ColorBlindAssist : SingletonBehaviour<ColorBlindAssist>
    {
        [SerializeField] private ColorblindTypes _testType;
        
        private Colorblindness _colorblindness;
        public ColorblindTypes Type
        {
            get => (ColorblindTypes)_colorblindness.CurrentType;
            set
            {
                _colorblindness.Change((int)value);
            }
        }
        
        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            _colorblindness = GetComponent<Colorblindness>();
        }

        private void OnSceneLoaded(Scene sc, LoadSceneMode mode)
        {
            foreach (var camera in FindObjectsOfType<Camera>())
            {
                camera.GetComponent<UniversalAdditionalCameraData>().renderPostProcessing = true;
            }
            _colorblindness.Change();
        }

        [Button("DEBUG:: TEST")]
        private void Set()
        {
            _colorblindness.Change((int)_testType);
        }
    }
}