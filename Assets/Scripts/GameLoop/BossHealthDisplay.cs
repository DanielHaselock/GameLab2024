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

        private IEnumerator Instantiate()
        {
            yield return new WaitForSeconds(1.5f);
            yield return new WaitUntil(() => _healthComponent.IsInitialised);
            _gameUI =  FindObjectOfType<GameUI>();
            _current = _healthComponent.Health;
            _healthComponent.OnHealthDepleted += OnDeath;
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
                RPC_ShakeBar();
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_UpdateHealth(bool show, float val)
        {
            _gameUI.SetBossHealth(show, val);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ShakeBar()
        {
            _current = _healthComponent.Health;
            _gameUI.ShakeBossHealthBar();
        }

        private void OnDeath(int id)
        {
            StartCoroutine(Hide());
        }

        IEnumerator Hide()
        {
            yield return new WaitForSeconds(1.0f);
            RPC_UpdateHealth(false, 0);
        }
    }
}