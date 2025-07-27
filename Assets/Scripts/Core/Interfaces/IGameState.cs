using System;

namespace MiniMatch.Core.Interfaces
{
    public enum GamePhase
    {
        Loading,
        Preview,
        Playing,
        Paused,
        LevelComplete,
        GameComplete
    }

    public interface IGameState
    {
        GamePhase CurrentPhase { get; }
        int CurrentLevel { get; }
        int MatchedPairs { get; }
        int TotalPairs { get; }
        float ElapsedTime { get; }
        bool CanInteract { get; }
        
        event Action<GamePhase> OnPhaseChanged;
        event Action<int> OnLevelChanged;
        event Action<float> OnTimeUpdated;
        
        void SetPhase(GamePhase phase);
        void NextLevel();
        void UpdateTime(float deltaTime);
        void SetTotalPairs(int totalPairs);
        void Reset();
    }
}