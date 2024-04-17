using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Accessibility
{
    public class VisionAssistHighContrast : MonoBehaviour
    {
        public enum AssistEntityType
        {
            Player = 0,
            Enemy,
            ItemOfInterest
        }

        [SerializeField] private AssistEntityType entityType;
        [SerializeField] private VisionAssistHCAsset asset;
        
        private Dictionary<Material, Dictionary<string, Color>> _matDb;
        private Volume _volume;
        
        private IEnumerator Start()
        {
            BuildMaterialVDB();
            yield return new WaitForEndOfFrame();
            _volume = GameObject.Find("PP").GetComponent<Volume>();
            AccessibilityHelper.OnAccessibilitySettingChanged += UpdateHC;
            if(AccessibilityHelper.Type == AccessibilityHelper.AccessibilityType.HighContrast)
                UpdateHC();
        }
        
        private SerializableDictionary<string, Color> GetPalletDict()
        {
            switch (entityType)
            {
                case AssistEntityType.Player:
                    if (this == null)
                        return null;
                    
                    var no = GetComponentInParent<NetworkObject>();
                    if (no == null)
                        return null;
                    return no.HasInputAuthority ? asset.LocalPlayerColors : asset.RemotePlayerColors;
                
                case AssistEntityType.Enemy: return asset.EnemyColors;
                case AssistEntityType.ItemOfInterest: return asset.ItemOfInterestColors;
            }

            return null;
        }
        
        private void BuildMaterialVDB()
        {
            var dictToUse = GetPalletDict();
            _matDb = new Dictionary<Material, Dictionary<string, Color>>();
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                if(renderer is ParticleSystemRenderer)
                    continue;

                var dict = new Dictionary<string, Color>();
                foreach (var key in dictToUse.Keys)
                {
                    if (renderer.material.HasColor(key))
                    {
                        dict.Add(key, renderer.material.GetColor(key));
                    }
                }
                _matDb.Add(renderer.material, dict);
            }
        }
        
        private void UpdateHC()
        {
            
            if (AccessibilityHelper.Type != AccessibilityHelper.AccessibilityType.HighContrast)
            {
                ResetAssist();
            }
            else
            {
                UpdateAssist();
            }
        }

        private void ResetAssist()
        {
            Tonemapping toneMapping;
            _volume.profile.TryGet(out toneMapping);
            toneMapping.mode.value = TonemappingMode.None;
            foreach (var material in _matDb.Keys)
            {
                material.DisableKeyword("_EMISSION");
                foreach (var kv in _matDb[material])
                {
                    material.SetColor(kv.Key, kv.Value);
                }
            }
        }
        
        private void UpdateAssist()
        {
            Tonemapping toneMapping;
            _volume.profile.TryGet(out toneMapping);
            toneMapping.mode.value = TonemappingMode.Neutral;
            var dict = GetPalletDict();
            foreach (var material in _matDb.Keys)
            {
                material.EnableKeyword("_EMISSION");
                foreach (var kv in dict)
                {
                    material.SetColor(kv.Key, kv.Value);
                }
            }
            
        }
    }
}

