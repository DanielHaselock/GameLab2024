using System;
using Fusion;
using Utils;
using UnityEngine;

namespace Networking.Behaviours
{
    public class NetworkTimer : NetworkBehaviour
    {
        [SerializeField]
        private float tickInterval=1;
        [Networked] private TickTimer _timer { get; set; }
        
        private float nextTickCheck = 0;
        private bool wasTimerRunning = false;
        
        public Action<TimeSpan> OnTimerStarted;
        public Action<TimeSpan> OnTimerTick;
        public Action OnTimerEnded;
        
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
        
        public override void Render()
        {
            base.Render();
            HandleTimerTick();
        }
    }
}