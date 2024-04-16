using Fusion;
using System;
using System.Collections;
using UnityEngine;

namespace GameLoop
{
    public class BossHealthDisplay : NetworkBehaviour
    {
        [SerializeField] private HealthComponent _healthComponent;

        private GameUI _gameUI;
        private float _current;
        private bool _isDead;
        private IEnumerator Instantiate()
        {
            yield return new WaitForSeconds(1.5f);
            yield return new WaitUntil(() => _healthComponent.IsInitialised);
            _gameUI =  FindObjectOfType<GameUI>();
            _current = _healthComponent.Health;
        }

        public override void Spawned()
        {
            StartCoroutine(Instantiate());
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();

            if (_healthComponent != null && _gameUI != null)
            {
                RPC_UpdateHealth(true, _healthComponent.Health / _healthComponent.MaxHealth);
            }

            if (_current > _healthComponent.Health)
            {
                _current = _healthComponent.Health;
                _gameUI.ShakeBossHealthBar();
                if(_isDead)
                    return;
                _isDead = true;
                StartCoroutine(Hide());
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_UpdateHealth(bool show, float val)
        {
            _gameUI.SetBossHealth(false, 0);
        }

        IEnumerator Hide()
        {
            yield return new WaitForSeconds(0.25f);
            _gameUI.SetBossHealth(false, 0);
        }
    }
}