using System;

namespace MiniMatch.Core.Services
{
    public interface ITimerService
    {
        event Action<float> OnTimeUpdated;
        
        void StartTimer();
        void StopTimer();
        void ResetTimer();
        void UpdateTimer(float deltaTime);
        float ElapsedTime { get; }
        bool IsRunning { get; }
    }
}