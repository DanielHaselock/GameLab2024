using Fusion;
using System;
using System.Collections;
using UnityEngine;

namespace GameLoop
{
    public class BossHealthDisplay : MonoBehaviour
    {
        [SerializeField] private HealthComponent _healthComponent;

        private GameUI _gameUI;
        private float _current;
        private bool _isDead;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(1.5f);
            yield return new WaitUntil(() => _healthComponent.IsInitialised);
            _gameUI =  FindObjectOfType<GameUI>();
            _current = _healthComponent.Health;
        }

        private void Update()
        {
            if (_healthComponent != null && _gameUI != null)
            {
                UpdateHealth(true, _healthComponent.Health / _healthComponent.MaxHealth);
            }

            if (_current > _healthComponent.Health)
            {
                _current = _healthComponent.Health;
                ShakeBar();
                if (_isDead)
                    return;
                _isDead = true;
                StartCoroutine(Hide());
            }
        }
        
        private void UpdateHealth(bool show, float val)
        {
            _gameUI.SetBossHealth(show, val);
        }
        
        private void ShakeBar()
        {
            _gameUI.ShakeBossHealthBar();
        }

        IEnumerator Hide()
        {
            yield return new WaitForSeconds(0.25f);
            UpdateHealth(false, 0);
        }
    }
}