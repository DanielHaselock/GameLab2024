using System;
using System.Collections;
using Fusion;
using Networking.Utils;
using Unity.VisualScripting;
using UnityEngine;

namespace Networking.Behaviours
{
    public class NetworkManagerSynchedHelper : NetworkBehaviour
    {
        [SerializeField]
        private float tickInterval=1;
        
        [Networked, Capacity(10)] private NetworkDictionary<int, string> _userNickNames => default;
        [Networked] private TickTimer _timer { get; set; }
        [Networked] private int _readyUserCount { get; set; }
        
        private float nextTickCheck = 0;
        private bool wasTimerRunning = false;
        
        public Action<TimeSpan> OnTimerStarted;
        public Action<TimeSpan> OnTimerTick;
        public Action OnTimerEnded;

        private ChangeDetector _changeDetector;

        public Action<NetworkEvent> OnSimpleNetworkMessageRecieved;
        
        public bool AllUsersReady(int expectedPlayerCount)
        {
            return expectedPlayerCount.Equals(_readyUserCount);
        }

        public void InitialiseUser(int playerId, string nickname)
        {
            if (Runner.IsServer)
            {
                _userNickNames.Add(playerId, nickname);
                _readyUserCount += 1;
                return;
            }

            RPC_InitClientOnServer(playerId, nickname);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_InitClientOnServer(int playerId, string nickname)
        {
            _userNickNames.Add(playerId, nickname);
            _readyUserCount += 1;
        }
        
        private void Start()
        {
            DontDestroyOnLoad(this);
        }

        public override void Spawned()
        {
            base.Spawned();
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        }

        public string GetPlayerNickNameById(int id)
        {
            if (_userNickNames.ContainsKey(id))
                return _userNickNames[id];
            return string.Empty;
        }
        
        public void AddUserNickName(int id, string nickname)
        {
            _userNickNames.Set(id, nickname);
        }

        public void RemoveUserNickName(int id)
        {
            _userNickNames.Remove(id);
        }

        public void StartTimer(TimeSpan timeSpan)
        {
            if (!Runner.IsServer)
            {
                NetworkLogger.Error("Timer can only be started from the server");
                return;
            }

            _timer = TickTimer.CreateFromSeconds(Runner, (float)timeSpan.TotalSeconds);
            nextTickCheck = 0;
        }

        public void StopTimer()
        {
            ResetTimer();
        }
        
        private void ResetTimer()
        {
            wasTimerRunning = false;
            _timer = TickTimer.None;
        }
        
        private void HandleTimerTick()
        {
            if (_timer.Expired(Runner) && wasTimerRunning)
            {
                ResetTimer();
                OnTimerEnded?.Invoke();
                return;
            }
            
            if(!_timer.IsRunning)
                return;
            
            var remainingTime = Mathf.RoundToInt(_timer.RemainingTime(Runner).Value);
            
            //if remaining time is 0, ignore
            if (remainingTime <= 0)
                return;
            
            if (!wasTimerRunning)
            {
                wasTimerRunning = true;
                OnTimerStarted?.Invoke(TimeSpan.FromSeconds(remainingTime));
            }
            
            if (Runner.LocalRenderTime > nextTickCheck)
            {
                nextTickCheck = Runner.LocalRenderTime + tickInterval;
                OnTimerTick?.Invoke(TimeSpan.FromSeconds(remainingTime));
            }
        }

        
        public void SendGlobalSimpleNetworkMessage(NetworkEvent eventData)
        {
            RPC_SendGlobalSimpleNetworkMessage(eventData.EventName, eventData.EventData);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_SendGlobalSimpleNetworkMessage(string eventName, string eventData)
        {
            OnSimpleNetworkMessageRecieved?.Invoke(new NetworkEvent()
            {
                EventName = eventName,
                EventData = eventData
            });
        }
        
        public override void Render()
        {
            base.Render();
            HandleTimerTick();
        }
        
    }
}