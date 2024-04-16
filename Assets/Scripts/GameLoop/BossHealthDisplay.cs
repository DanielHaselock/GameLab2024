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
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(1.5f);
            yield return new WaitUntil(() => _healthComponent.IsInitialised);
            _gameUI =  FindObjectOfType<GameUI>();
            _current = _healthComponent.Health;
            _healthComponent.OnHealthDepleted += OnDeath;
        }

        private void Update()
        {
            if (_healthComponent != null && _gameUI != null)
            {
                _gameUI.SetBossHealth(true, _healthComponent.Health / _healthComponent.MaxHealth);
            }

            if (_current > _healthComponent.Health)
            {
                _current = _healthComponent.Health;
                _gameUI.ShakeBossHealthBar();
            }
        }

        private void OnDeath(int id)
        {
            StartCoroutine(Hide());
        }

        IEnumerator Hide()
        {
            yield return new WaitForSeconds(1);
            _gameUI.SetBossHealth(false, 0);
        }
    }
}