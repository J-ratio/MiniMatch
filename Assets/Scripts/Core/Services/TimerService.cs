using System;
using UnityEngine;
using MiniMatch.Core.Services;
using MiniMatch.Core.Events;

namespace MiniMatch.Core.Services
{
    /// <summary>
    /// Simple timer service that tracks elapsed time and publishes updates
    /// </summary>
    public class TimerService : ITimerService
    {
        public event Action<float> OnTimeUpdated;
        
        public float ElapsedTime { get; private set; }
        public bool IsRunning { get; private set; }
        
        private readonly GameEventBus _eventBus;
        
        public TimerService(GameEventBus eventBus = null)
        {
            _eventBus = eventBus ?? GameEventBus.Instance;
            Reset();
        }
        
        public void StartTimer()
        {
            if (IsRunning) return;
            
            IsRunning = true;
            Debug.Log("Timer started");
        }
        
        public void StopTimer()
        {
            if (!IsRunning) return;
            
            IsRunning = false;
            Debug.Log($"Timer stopped at {ElapsedTime:F2}s");
        }
        
        public void ResetTimer()
        {
            ElapsedTime = 0f;
            IsRunning = false;
            OnTimeUpdated?.Invoke(ElapsedTime);
            Debug.Log("Timer reset");
        }
        
        public void UpdateTimer(float deltaTime)
        {
            if (!IsRunning) return;
            
            ElapsedTime += deltaTime;
            OnTimeUpdated?.Invoke(ElapsedTime);
        }
        
        private void Reset()
        {
            ElapsedTime = 0f;
            IsRunning = false;
        }
    }
}