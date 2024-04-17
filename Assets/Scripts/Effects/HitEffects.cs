using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using CartoonFX;
using Fusion;
using UnityEngine;

namespace Effects
{
    public class HitEffects : MonoBehaviour
    {
        [SerializeField]
        private Material hitMaterialEffect;
        
        [SerializeField]
        private List<GameObject> itemsToDisableDuringHit;

        [SerializeField] 
        private float materialHitEfectDelay = 0.5f;
        
        [SerializeField]
        private GameObject hitGameObjectFx;

        [SerializeField] 
        private Transform effectSpawnPosition;
        
        [SerializeField] 
        private string hitSFX;
        
        private Dictionary<Renderer, Material> _materialMap;

        private Coroutine hitMaterialUpdateRountine;
        
        private void Start()
        {
            _materialMap = new Dictionary<Renderer, Material>();
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                if(renderer is ParticleSystemRenderer)
                    continue;
                _materialMap.Add(renderer, renderer.material);
            }
        }

        public void OnHit()
        {
            if(hitMaterialUpdateRountine != null)
                StopCoroutine(hitMaterialUpdateRountine);
            
            hitMaterialUpdateRountine = StartCoroutine(DoHitMaterialUpdate());
            
            if (hitGameObjectFx != null)
            {
                var player = GetComponent<Player>();
                bool allowShake = player != null && player.HasInputAuthority;
                Debug.Log($"Allow Shake {allowShake}");
                var pos = transform.position;
                if (effectSpawnPosition != null)
                    pos = effectSpawnPosition.position;
                
                var fx = Instantiate(hitGameObjectFx, pos, Quaternion.identity);
                if (!allowShake)
                {
                    var eff = fx.GetComponent<CFXR_Effect>();
                    if(eff == null)
                        return;
                    eff.cameraShake.enabled = false;
                }
            }

            AudioManager.Instance.PlaySFX3D(AudioConstants.HitFeedback,  transform.position,true, false);
        }

        IEnumerator DoHitMaterialUpdate()
        {
            if (hitMaterialEffect == null)
                yield break;
            
            foreach (var renderer in _materialMap.Keys)
            {
                renderer.material = hitMaterialEffect;
            }

            foreach (var go in itemsToDisableDuringHit)
            {
                go.SetActive(false);
            }
            
            yield return new WaitForSeconds(materialHitEfectDelay);
            foreach (var renderer in _materialMap.Keys)
            {
                renderer.material = _materialMap[renderer];
            }

            foreach (var go in itemsToDisableDuringHit)
            {
                go.SetActive(true);
            }
            
            hitMaterialUpdateRountine = null;
        }
    }    
}

