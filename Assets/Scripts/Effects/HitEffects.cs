using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
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

        private Dictionary<Renderer, Material> _materialMap;

        private Coroutine hitMaterialUpdateRountine;
        
        private void Start()
        {
            _materialMap = new Dictionary<Renderer, Material>();
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                _materialMap.Add(renderer, renderer.material);
            }
        }

        public void OnHit()
        {
            if(hitMaterialUpdateRountine != null)
                StopCoroutine(hitMaterialUpdateRountine);
            
            hitMaterialUpdateRountine = StartCoroutine(DoHitMaterialUpdate());
            if (hitGameObjectFx != null)
                Instantiate(hitGameObjectFx, transform.position, Quaternion.identity);
            
            AudioManager.Instance.PlaySFX3D(SFXConstants.Hit,  transform.position,true, false);
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

